using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using SpeedrunComSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.Leaderboard.UI.Components;

[GlobalFontConsumer(GlobalFont.TextFont)]
public sealed class LeaderboardComponent : IComponent
{
    private readonly LiveSplitState state; private readonly SpeedrunComClient client; private readonly GraphicsCache cache = new(); private readonly object dataLock = new();
    private ReadOnlyCollection<Record> records = new(new List<Record>()); private readonly Dictionary<string, Image> flagImages = new(StringComparer.OrdinalIgnoreCase); private TimeStamp lastUpdate; private bool loading; private string errorMessage;
    public LeaderboardSettings Settings { get; } = new(); public string ComponentName => "Leaderboard";
    public float PaddingTop=>0; public float PaddingLeft=>0; public float PaddingBottom=>0; public float PaddingRight=>0;
    public float VerticalHeight => (Settings.ShowHeader ? Settings.RowHeight : 0) + DisplayRowCount * Settings.RowHeight;
    private int DisplayRowCount => Settings.SurroundingMode ? Settings.SurroundingAbove + Settings.SurroundingBelow + 1 : Settings.EntryCount;
    public float MinimumWidth=>180; public float HorizontalWidth=>320; public float MinimumHeight=>VerticalHeight; public IDictionary<string,Action> ContextMenuControls=>null;
    public LeaderboardComponent(LiveSplitState state) { this.state=state; client=new SpeedrunComClient(userAgent:Updates.UpdateHelper.UserAgent,maxCacheElements:0); }

    public void Update(IInvalidator invalidator, LiveSplitState currentState,float width,float height,LayoutMode mode)
    {
        cache.Restart(); cache["Game"]=currentState.Run.GameName; cache["Category"]=currentState.Run.CategoryName; cache["Start"]=Settings.StartRank; cache["Count"]=Settings.EntryCount; cache["Surround"]=Settings.SurroundingMode; cache["User"]=Settings.HighlightUsername; cache["Above"]=Settings.SurroundingAbove; cache["Below"]=Settings.SurroundingBelow;
        cache["Platform"]=Settings.FilterPlatform?currentState.Run.Metadata.PlatformName:null; cache["Region"]=Settings.FilterRegion?currentState.Run.Metadata.RegionName:null; cache["Emulator"]=Settings.FilterPlatform?(bool?)currentState.Run.Metadata.UsesEmulator:null; cache["Variables"]=(Settings.FilterVariables||Settings.FilterSubcategories)?string.Join(",",currentState.Run.Metadata.VariableValueNames.Values):null; cache["Timing"]=Settings.TimingMethod;
        if(cache.HasChanged) BeginRefresh(); else if(lastUpdate!=null && TimeStamp.Now-lastUpdate>=TimeSpan.FromMinutes(Settings.RefreshMinutes)) BeginRefresh(); invalidator?.Invalidate(0,0,width,height);
    }
    private void BeginRefresh(){if(loading)return;loading=true;errorMessage=null;Task.Factory.StartNew(RefreshLeaderboard);}
    private void RefreshLeaderboard()
    {
        try
        {
            lastUpdate=TimeStamp.Now; var m=state?.Run?.Metadata; if(m?.Game==null||m.Category==null){lock(dataLock)records=new(new List<Record>());errorMessage="Set a Speedrun.com game and category";return;}
            IEnumerable<VariableValue> vf=null; if(Settings.FilterVariables||Settings.FilterSubcategories) vf=m.VariableValues.Values.Where(v=>v!=null&&(v.Variable.IsSubcategory?Settings.FilterSubcategories:Settings.FilterVariables));
            string platform=Settings.FilterPlatform&&m.Platform!=null?m.Platform.ID:null, region=Settings.FilterRegion&&m.Region!=null?m.Region.ID:null; EmulatorsFilter emulator=EmulatorsFilter.NotSet; if(Settings.FilterPlatform) emulator=m.UsesEmulator?EmulatorsFilter.OnlyEmulators:EmulatorsFilter.NoEmulators;
            int requested=Settings.SurroundingMode?200:Math.Min(10000,Settings.StartRank+Settings.EntryCount-1);
            var board=client.Leaderboards.GetLeaderboardForFullGameCategory(m.Game.ID,m.Category.ID,top:requested,platformId:platform,regionId:region,emulatorsFilter:emulator,variableFilters:vf,orderBy:GetTimingMethodOverride()); lock(dataLock) records=board?.Records??new(new List<Record>()); if(Settings.ShowCountryFlag) LoadFlags(records);
        }
        catch(Exception ex){System.Diagnostics.Debug.WriteLine(ex);errorMessage="Leaderboard unavailable";lock(dataLock)records=new(new List<Record>());} finally{loading=false;}
    }
    private SpeedrunComSharp.TimingMethod? GetTimingMethodOverride()=>Settings.TimingMethod switch{"Real Time"=>SpeedrunComSharp.TimingMethod.RealTime,"Real Time Without Loads"=>SpeedrunComSharp.TimingMethod.RealTimeWithoutLoads,"Game Time"=>SpeedrunComSharp.TimingMethod.GameTime,_=>null};
    private TimeSpan? GetTime(Record r)=>GetTimingMethodOverride() switch{SpeedrunComSharp.TimingMethod.RealTime=>r.Times.RealTime,SpeedrunComSharp.TimingMethod.RealTimeWithoutLoads=>r.Times.RealTimeWithoutLoads,SpeedrunComSharp.TimingMethod.GameTime=>r.Times.GameTime,_=>r.Times.Primary};
    public void DrawVertical(Graphics g,LiveSplitState s,float width,System.Drawing.Region clip)=>Draw(g,s,width,VerticalHeight); public void DrawHorizontal(Graphics g,LiveSplitState s,float height,System.Drawing.Region clip)=>Draw(g,s,HorizontalWidth,height);

    private void Draw(Graphics g,LiveSplitState s,float width,float height)
    {
        g.SmoothingMode=SmoothingMode.HighQuality; g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit; int rh=Settings.RowHeight,y=0; Font normal=s.LayoutSettings.TextFont; using Font bold=new(normal,FontStyle.Bold); using Font header=new(normal,FontStyle.Bold); Color shadow=Color.FromArgb(160,Color.Black);
        if(Settings.BackgroundColor.A>0) Fill(g,new RectangleF(0,0,width,height),Settings.BackgroundColor);
        float rankW=Math.Min(Settings.RankWidth,Math.Max(20,width-40)),timeW=Math.Min(Settings.TimeWidth,Math.Max(20,width-rankW-20)),gap=7; float playerX=gap+rankW,playerW=Math.Max(10,width-rankW-timeW-gap*2),timeX=width-timeW-gap;
        if(Settings.ShowHeader){DrawText(g,"Rank",header,Settings.HeaderTextColor,shadow,new RectangleF(gap,y,rankW,rh),Align(Settings.RankAlignment),s.LayoutSettings.DropShadows);DrawText(g,"Player",header,Settings.HeaderTextColor,shadow,new RectangleF(playerX,y,playerW,rh),Align(Settings.PlayerAlignment),s.LayoutSettings.DropShadows);DrawText(g,"Time",header,Settings.HeaderTextColor,shadow,new RectangleF(timeX,y,timeW,rh),Align(Settings.TimeAlignment),s.LayoutSettings.DropShadows);y+=rh;}
        List<Record> all; lock(dataLock) all=records.ToList(); var rows=SelectRows(all);
        if(loading&&all.Count==0){Centered(g,"Loading leaderboard…",normal,Settings.RowTextColor,shadow,width,y,rh,s.LayoutSettings.DropShadows);return;} if(rows.Count==0){Centered(g,errorMessage??"No leaderboard entries",normal,Settings.RowTextColor,shadow,width,y,rh,s.LayoutSettings.DropShadows);return;}
        for(int i=0;i<rows.Count;i++)
        {
            var item=rows[i]; bool highlighted=IsHighlighted(item.Record); if(Settings.AlternateRows&&i%2==1) Fill(g,new RectangleF(0,y,width,rh),Color.FromArgb(Settings.AlternateOpacity,Settings.AlternateRowColor)); if(highlighted&&Settings.HighlightBackgroundColor.A>0) Fill(g,new RectangleF(0,y,width,rh),Settings.HighlightBackgroundColor);
            Font f=highlighted&&Settings.HighlightBold?bold:normal; Color pc=highlighted?Settings.HighlightTextColor:Settings.RowTextColor,rc=highlighted?Settings.HighlightTextColor:Settings.RankTextColor,tc=highlighted?Settings.HighlightTextColor:Settings.TimeTextColor;
            DrawText(g,Ordinal(item.Rank),f,rc,shadow,new RectangleF(gap,y,rankW,rh),Align(Settings.RankAlignment),s.LayoutSettings.DropShadows); DrawPlayer(g,item.Record,f,pc,shadow,new RectangleF(playerX,y,playerW,rh),Align(Settings.PlayerAlignment),s.LayoutSettings.DropShadows); DrawText(g,FormatTime(GetTime(item.Record)),f,tc,shadow,new RectangleF(timeX,y,timeW,rh),Align(Settings.TimeAlignment),s.LayoutSettings.DropShadows); y+=rh;
        }
    }
    private sealed class DisplayRecord{public Record Record;public int Rank;}
    private List<DisplayRecord> SelectRows(List<Record> all)
    {
        var ranked=all.Select((r,i)=>new DisplayRecord{Record=r,Rank=GetRank(r,i+1)}).ToList(); if(Settings.SurroundingMode&&!string.IsNullOrWhiteSpace(Settings.HighlightUsername)){int pos=ranked.FindIndex(x=>IsHighlighted(x.Record));if(pos>=0){int start=Math.Max(0,pos-Settings.SurroundingAbove),count=Settings.SurroundingAbove+Settings.SurroundingBelow+1;if(start+count>ranked.Count)start=Math.Max(0,ranked.Count-count);return ranked.Skip(start).Take(count).ToList();}}
        return ranked.Where(x=>x.Rank>=Settings.StartRank).Take(Settings.EntryCount).ToList();
    }
    private static int GetRank(Record r,int fallback){object v=Prop(r,"Place")??Prop(r,"Rank")??Prop(r,"Position");return v is int i?i:int.TryParse(Convert.ToString(v),out i)?i:fallback;}
    private bool IsHighlighted(Record r)=>!string.IsNullOrWhiteSpace(Settings.HighlightUsername)&&r.Players.Any(p=>string.Equals(p.Name,Settings.HighlightUsername,StringComparison.OrdinalIgnoreCase)||string.Equals(NameByMode(p),Settings.HighlightUsername,StringComparison.OrdinalIgnoreCase));
    private string PlayerText(Record r)=>string.Join(" & ",r.Players.Select(NameByMode));
    private void DrawPlayer(Graphics g, Record record, Font font, Color color, Color shadow, RectangleF rect, StringAlignment alignment, bool dropShadow)
    {
        string text=PlayerText(record); string code=record.Players.Select(CountryCode).FirstOrDefault(c=>!string.IsNullOrWhiteSpace(c)); Image flag=null;
        if(Settings.ShowCountryFlag&&!string.IsNullOrWhiteSpace(code)){lock(dataLock)flagImages.TryGetValue(code,out flag);}
        if(flag==null){DrawText(g,text,font,color,shadow,rect,alignment,dropShadow);return;}
        float flagH=Math.Min(14f,rect.Height-6f),flagW=flagH*1.5f,spacing=5f; SizeF textSize=g.MeasureString(text,font,int.MaxValue,StringFormat.GenericTypographic); float groupW=Math.Min(rect.Width,flagW+spacing+textSize.Width);
        float startX=alignment==StringAlignment.Center?rect.X+(rect.Width-groupW)/2f:alignment==StringAlignment.Far?rect.Right-groupW:rect.X; float fy=rect.Y+(rect.Height-flagH)/2f;
        g.DrawImage(flag,new RectangleF(startX,fy,flagW,flagH)); var textRect=new RectangleF(startX+flagW+spacing,rect.Y,Math.Max(1,rect.Right-(startX+flagW+spacing)),rect.Height); DrawText(g,text,font,color,shadow,textRect,StringAlignment.Near,dropShadow);
    }
    private void LoadFlags(IEnumerable<Record> source)
    {
        foreach(string code in source.SelectMany(r=>r.Players).Select(CountryCode).Where(c=>!string.IsNullOrWhiteSpace(c)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            lock(dataLock){if(flagImages.ContainsKey(code))continue;}
            try
            {
                using var web=new WebClient(); web.Headers[HttpRequestHeader.UserAgent]=Updates.UpdateHelper.UserAgent; byte[] bytes=web.DownloadData("https://flagcdn.com/w40/"+code.ToLowerInvariant()+".png"); using var ms=new MemoryStream(bytes); using var loaded=Image.FromStream(ms); var copy=new Bitmap(loaded); lock(dataLock)flagImages[code]=copy;
            }
            catch(Exception ex){System.Diagnostics.Debug.WriteLine("Could not load flag "+code+": "+ex.Message);}
        }
    }
    private string NameByMode(object p)
    {
        string fallback=Convert.ToString(Prop(p,"Name"))??"Unknown"; object names=Prop(p,"Names"); if(Settings.PlayerNameMode=="International name") return Text(Prop(names,"International"))??Text(Prop(p,"InternationalName"))??fallback; if(Settings.PlayerNameMode=="Japanese name") return Text(Prop(names,"Japanese"))??Text(Prop(p,"JapaneseName"))??fallback; return fallback;
    }
    private static string CountryCode(object p)
    {
        object user=Prop(p,"User")??p; object location=Prop(user,"Location")??Prop(p,"Location"); object country=Prop(location,"Country")??Prop(user,"Country")??Prop(p,"Country");
        string code=Text(Prop(country,"Code"))??Text(Prop(country,"ID"))??Text(Prop(location,"CountryCode"))??Text(Prop(user,"CountryCode"))??Text(Prop(p,"CountryCode"));
        if(string.IsNullOrWhiteSpace(code)||code.Length!=2||code.Any(c=>!char.IsLetter(c)))return null; return code.ToUpperInvariant();
    }
    private string FormatTime(TimeSpan? value)
    {
        if(value==null)return "—"; var t=value.Value; bool hours=!Settings.HoursOnlyWhenNeeded||t.TotalHours>=1; string frac=Settings.ShowMilliseconds?"."+t.Milliseconds.ToString("000"):""; if(Settings.TimeFormat.StartsWith("Words")) return (hours?((int)t.TotalHours)+"h ":"")+t.Minutes+"m "+t.Seconds+"s"+(Settings.ShowMilliseconds?" "+t.Milliseconds.ToString("000")+"ms":""); return hours?$"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}{frac}":$"{t.Minutes}:{t.Seconds:00}{frac}";
    }
    private static object Prop(object o,string name)=>o?.GetType().GetProperty(name,BindingFlags.Public|BindingFlags.Instance|BindingFlags.IgnoreCase)?.GetValue(o); private static string Text(object o){string s=Convert.ToString(o);return string.IsNullOrWhiteSpace(s)?null:s;}
    private static StringAlignment Align(string s)=>s=="Center"?StringAlignment.Center:s=="Right"?StringAlignment.Far:StringAlignment.Near; private static string Ordinal(int v){int m=v%100;if(m>=11&&m<=13)return v+"th";return v+(v%10==1?"st":v%10==2?"nd":v%10==3?"rd":"th");}
    private static void Fill(Graphics g,RectangleF r,Color c){using var b=new SolidBrush(c);g.FillRectangle(b,r);} private static void Centered(Graphics g,string text,Font f,Color c,Color sh,float w,float y,float h,bool ds)=>DrawText(g,text,f,c,sh,new RectangleF(5,y,w-10,h),StringAlignment.Center,ds);
    private static void DrawText(Graphics g,string text,Font f,Color c,Color sh,RectangleF r,StringAlignment a,bool ds){using var sf=new StringFormat{Alignment=a,LineAlignment=StringAlignment.Center,Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap};if(ds){using var sb=new SolidBrush(sh);var rr=r;rr.Offset(1,1);g.DrawString(text,f,sb,rr,sf);}using var b=new SolidBrush(c);g.DrawString(text,f,b,r,sf);}
    public Control GetSettingsControl(LayoutMode mode){Settings.Mode=mode;return Settings;} public XmlNode GetSettings(XmlDocument d)=>Settings.GetSettings(d); public void SetSettings(XmlNode n)=>Settings.SetSettings(n); public int GetSettingsHashCode()=>Settings.GetSettingsHashCode(); public void Dispose(){lock(dataLock){foreach(var image in flagImages.Values)image.Dispose();flagImages.Clear();}}
}
