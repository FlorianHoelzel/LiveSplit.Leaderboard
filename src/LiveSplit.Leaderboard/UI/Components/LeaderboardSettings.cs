using LiveSplit.UI;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.Leaderboard.UI.Components;

public sealed class LeaderboardSettings : UserControl
{
    private readonly NumericUpDown startRank, entryCount, surroundingAbove, surroundingBelow, refreshMinutes, rowHeight, rankWidth, timeWidth, alternateOpacity;
    private readonly CheckBox filterPlatform, filterRegion, filterVariables, filterSubcategories, showHeader, alternateRows, surroundingMode, showMilliseconds, hoursOnlyWhenNeeded, showCountryFlag, highlightBold, showHighlightBackground;
    private readonly TextBox highlightUsername;
    private readonly ComboBox timingMethod, rankAlignment, playerAlignment, timeAlignment, timeFormat, playerNameMode;
    private readonly Button headerColorButton, rowColorButton, rankColorButton, timeColorButton, backgroundColorButton, alternateColorButton, highlightTextColorButton, highlightBackgroundColorButton;

    public int StartRank { get => (int)startRank.Value; set => startRank.Value = Clamp(value, startRank.Minimum, startRank.Maximum); }
    public int EntryCount { get => (int)entryCount.Value; set => entryCount.Value = Clamp(value, entryCount.Minimum, entryCount.Maximum); }
    public int SurroundingAbove { get => (int)surroundingAbove.Value; set => surroundingAbove.Value = Clamp(value, surroundingAbove.Minimum, surroundingAbove.Maximum); }
    public int SurroundingBelow { get => (int)surroundingBelow.Value; set => surroundingBelow.Value = Clamp(value, surroundingBelow.Minimum, surroundingBelow.Maximum); }
    public int RefreshMinutes { get => (int)refreshMinutes.Value; set => refreshMinutes.Value = Clamp(value, refreshMinutes.Minimum, refreshMinutes.Maximum); }
    public int RowHeight { get => (int)rowHeight.Value; set => rowHeight.Value = Clamp(value, rowHeight.Minimum, rowHeight.Maximum); }
    public int RankWidth { get => (int)rankWidth.Value; set => rankWidth.Value = Clamp(value, rankWidth.Minimum, rankWidth.Maximum); }
    public int TimeWidth { get => (int)timeWidth.Value; set => timeWidth.Value = Clamp(value, timeWidth.Minimum, timeWidth.Maximum); }
    public int AlternateOpacity { get => (int)alternateOpacity.Value; set => alternateOpacity.Value = Clamp(value, alternateOpacity.Minimum, alternateOpacity.Maximum); }
    public bool FilterPlatform { get => filterPlatform.Checked; set => filterPlatform.Checked = value; }
    public bool FilterRegion { get => filterRegion.Checked; set => filterRegion.Checked = value; }
    public bool FilterVariables { get => filterVariables.Checked; set => filterVariables.Checked = value; }
    public bool FilterSubcategories { get => filterSubcategories.Checked; set => filterSubcategories.Checked = value; }
    public bool ShowHeader { get => showHeader.Checked; set => showHeader.Checked = value; }
    public bool AlternateRows { get => alternateRows.Checked; set => alternateRows.Checked = value; }
    public bool SurroundingMode { get => surroundingMode.Checked; set => surroundingMode.Checked = value; }
    public bool ShowMilliseconds { get => showMilliseconds.Checked; set => showMilliseconds.Checked = value; }
    public bool HoursOnlyWhenNeeded { get => hoursOnlyWhenNeeded.Checked; set => hoursOnlyWhenNeeded.Checked = value; }
    public bool ShowCountryFlag { get => showCountryFlag.Checked; set => showCountryFlag.Checked = value; }
    public bool HighlightBold { get => highlightBold.Checked; set => highlightBold.Checked = value; }
    public bool ShowHighlightBackground { get => showHighlightBackground.Checked; set => showHighlightBackground.Checked = value; }
    public string HighlightUsername { get => highlightUsername.Text.Trim(); set => highlightUsername.Text = value ?? ""; }
    public string TimingMethod { get => Selected(timingMethod, "Leaderboard Default"); set => Select(timingMethod, value); }
    public string RankAlignment { get => Selected(rankAlignment, "Left"); set => Select(rankAlignment, value); }
    public string PlayerAlignment { get => Selected(playerAlignment, "Left"); set => Select(playerAlignment, value); }
    public string TimeAlignment { get => Selected(timeAlignment, "Right"); set => Select(timeAlignment, value); }
    public string TimeFormat { get => Selected(timeFormat, "Colon (1:23:45)"); set => Select(timeFormat, value); }
    public string PlayerNameMode { get => Selected(playerNameMode, "Speedrun.com username"); set => Select(playerNameMode, value); }

    public Color HeaderTextColor { get; set; } = Color.White;
    public Color RowTextColor { get; set; } = Color.White;
    public Color RankTextColor { get; set; } = Color.White;
    public Color TimeTextColor { get; set; } = Color.White;
    public Color BackgroundColor { get; set; } = Color.Transparent;
    public Color AlternateRowColor { get; set; } = Color.White;
    public Color HighlightTextColor { get; set; } = Color.White;
    public Color HighlightBackgroundColor { get; set; } = Color.FromArgb(100, 70, 130, 180);
    public LayoutMode Mode { get; set; }

    public LeaderboardSettings()
    {
        Dock = DockStyle.Fill;
        MinimumSize = new Size(420, 430);
        startRank = Number(1, 10000, 1); entryCount = Number(1, 100, 5);
        surroundingAbove = Number(0, 50, 2); surroundingBelow = Number(0, 50, 2);
        refreshMinutes = Number(1, 60, 5); rowHeight = Number(18, 60, 27);
        rankWidth = Number(35, 200, 70); timeWidth = Number(50, 250, 85); alternateOpacity = Number(0, 255, 28);
        filterPlatform = Check("Match platform and emulator"); filterRegion = Check("Match region");
        filterVariables = Check("Match category variables", true); filterSubcategories = Check("Match subcategories", true);
        showHeader = Check("Show Rank / Player / Time header", true); alternateRows = Check("Alternate row shading", true);
        surroundingMode = Check("Show entries surrounding highlighted runner"); showMilliseconds = Check("Show milliseconds");
        hoursOnlyWhenNeeded = Check("Show hours only when needed", true); showCountryFlag = Check("Show country flag");
        highlightBold = Check("Bold highlighted runner", true); showHighlightBackground = Check("Show highlighted runner background", true); highlightUsername = new TextBox { Width = 180 };
        timingMethod = Combo("Leaderboard Default", "Real Time", "Real Time Without Loads", "Game Time");
        rankAlignment = Combo("Left", "Center", "Right"); playerAlignment = Combo("Left", "Center", "Right"); playerAlignment.SelectedItem = "Left";
        timeAlignment = Combo("Left", "Center", "Right"); timeAlignment.SelectedItem = "Right";
        timeFormat = Combo("Colon (1:23:45)", "Words (1h 23m 45s)");
        playerNameMode = Combo("Speedrun.com username", "International name", "Japanese name");

        headerColorButton = ColorButton(() => HeaderTextColor, c => HeaderTextColor = c);
        rowColorButton = ColorButton(() => RowTextColor, c => RowTextColor = c);
        rankColorButton = ColorButton(() => RankTextColor, c => RankTextColor = c);
        timeColorButton = ColorButton(() => TimeTextColor, c => TimeTextColor = c);
        backgroundColorButton = ColorButton(() => BackgroundColor, c => BackgroundColor = c, true);
        alternateColorButton = ColorButton(() => AlternateRowColor, c => AlternateRowColor = c);
        highlightTextColorButton = ColorButton(() => HighlightTextColor, c => HighlightTextColor = c);
        highlightBackgroundColorButton = ColorButton(() => HighlightBackgroundColor, c => HighlightBackgroundColor = c, true);

        var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(12, 5) };
        tabs.TabPages.Add(CreateEntriesPage());
        tabs.TabPages.Add(CreateAppearancePage());
        tabs.TabPages.Add(CreateContentPage());
        Controls.Add(tabs);

        surroundingMode.CheckedChanged += (_, __) => UpdateEnabledStates();
        alternateRows.CheckedChanged += (_, __) => UpdateEnabledStates();
        showHeader.CheckedChanged += (_, __) => UpdateEnabledStates();
        showHighlightBackground.CheckedChanged += (_, __) => UpdateEnabledStates();
        RefreshColorButtons();
        UpdateEnabledStates();
    }

    private TabPage CreateEntriesPage()
    {
        var page = Page("Entries");
        var stack = Stack();
        var range = Group("Displayed entries");
        AddRow(range, "Start at rank", startRank);
        AddRow(range, "Number of entries", entryCount);
        AddWide(range, surroundingMode);
        AddRow(range, "Highlighted username", highlightUsername);
        AddRow(range, "Entries above", surroundingAbove);
        AddRow(range, "Entries below", surroundingBelow);
        AddGroup(stack, range);

        var updating = Group("Updating");
        AddRow(updating, "Refresh every", LabeledControl(refreshMinutes, "minutes"));
        AddGroup(stack, updating);
        page.Controls.Add(stack);
        return page;
    }

    private TabPage CreateAppearancePage()
    {
        var page = Page("Appearance");
        var stack = Stack();
        var layout = Group("Rows and columns");
        AddWide(layout, showHeader);
        AddRow(layout, "Row height", LabeledControl(rowHeight, "pixels"));
        AddRow(layout, "Rank column", LabeledControl(rankWidth, "pixels"));
        AddRow(layout, "Time column", LabeledControl(timeWidth, "pixels"));
        AddRow(layout, "Rank alignment", rankAlignment);
        AddRow(layout, "Player alignment", playerAlignment);
        AddRow(layout, "Time alignment", timeAlignment);
        AddGroup(stack, layout);

        var colors = Group("Colors");
        AddRow(colors, "Header text", headerColorButton);
        AddRow(colors, "Player text", rowColorButton);
        AddRow(colors, "Rank text", rankColorButton);
        AddRow(colors, "Time text", timeColorButton);
        AddRow(colors, "Background", backgroundColorButton);
        AddWide(colors, alternateRows);
        AddRow(colors, "Alternate row", alternateColorButton);
        AddRow(colors, "Alternate opacity", LabeledControl(alternateOpacity, "0–255"));
        AddGroup(stack, colors);

        var highlight = Group("Highlighted runner");
        AddRow(highlight, "Text", highlightTextColorButton);
        AddWide(highlight, showHighlightBackground);
        AddRow(highlight, "Background", highlightBackgroundColorButton);
        AddWide(highlight, highlightBold);
        AddGroup(stack, highlight);
        page.Controls.Add(stack);
        return page;
    }

    private TabPage CreateContentPage()
    {
        var page = Page("Content");
        var stack = Stack();
        var time = Group("Time");
        AddRow(time, "Timing method", timingMethod);
        AddRow(time, "Format", timeFormat);
        AddWide(time, showMilliseconds);
        AddWide(time, hoursOnlyWhenNeeded);
        AddGroup(stack, time);

        var players = Group("Player names");
        AddRow(players, "Display name", playerNameMode);
        AddWide(players, showCountryFlag);
        AddGroup(stack, players);

        var filters = Group("Leaderboard filters");
        AddDescription(filters, "Use the current run's metadata to narrow the leaderboard.");
        AddWide(filters, filterPlatform);
        AddWide(filters, filterRegion);
        AddWide(filters, filterVariables);
        AddWide(filters, filterSubcategories);
        AddGroup(stack, filters);
        page.Controls.Add(stack);
        return page;
    }

    private void UpdateEnabledStates()
    {
        surroundingAbove.Enabled = surroundingMode.Checked;
        surroundingBelow.Enabled = surroundingMode.Checked;
        alternateColorButton.Enabled = alternateRows.Checked;
        alternateOpacity.Enabled = alternateRows.Checked;
        headerColorButton.Enabled = showHeader.Checked;
        highlightBackgroundColorButton.Enabled = showHighlightBackground.Checked;
    }

    private Button ColorButton(Func<Color> get, Action<Color> set, bool alpha = false)
    {
        var b = new Button { Width = 104, Height = 25, Text = "Change…" };
        b.Click += (_, __) => { using var d = new ColorDialog { Color = get(), FullOpen = true, AllowFullOpen = true }; if (d.ShowDialog(this) == DialogResult.OK) { set(alpha ? d.Color : Color.FromArgb(255, d.Color)); RefreshColorButtons(); } };
        return b;
    }
    private void RefreshColorButtons()
    {
        SetButton(headerColorButton, HeaderTextColor); SetButton(rowColorButton, RowTextColor); SetButton(rankColorButton, RankTextColor); SetButton(timeColorButton, TimeTextColor); SetButton(backgroundColorButton, BackgroundColor); SetButton(alternateColorButton, AlternateRowColor); SetButton(highlightTextColorButton, HighlightTextColor); SetButton(highlightBackgroundColorButton, HighlightBackgroundColor);
    }
    private static void SetButton(Button b, Color c) { b.BackColor = c.A == 0 ? SystemColors.Control : Color.FromArgb(255, c); b.ForeColor = b.BackColor.GetBrightness() < .45f ? Color.White : Color.Black; }
    private static CheckBox Check(string text, bool value = false) => new() { Text = text, AutoSize = true, Checked = value };
    private static ComboBox Combo(params object[] values) { var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190 }; c.Items.AddRange(values); c.SelectedIndex = 0; return c; }
    private static string Selected(ComboBox c, string fallback) => c.SelectedItem?.ToString() ?? fallback;
    private static void Select(ComboBox c, string value) { if (c.Items.Contains(value)) c.SelectedItem = value; else c.SelectedIndex = 0; }
    private static NumericUpDown Number(decimal min, decimal max, decimal value) => new() { Minimum = min, Maximum = max, Value = value, Width = 80 };
    private static decimal Clamp(decimal value, decimal min, decimal max) => Math.Max(min, Math.Min(max, value));
    private static TabPage Page(string text) => new() { Text = text, Padding = new Padding(8), AutoScroll = true, UseVisualStyleBackColor = true };
    private static TableLayoutPanel Stack()
    {
        var stack = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, GrowStyle = TableLayoutPanelGrowStyle.AddRows };
        stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return stack;
    }
    private static TableLayoutPanel Group(string text)
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, Padding = new Padding(10, 6, 10, 8), Tag = text };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        return table;
    }
    private static void AddGroup(TableLayoutPanel stack, TableLayoutPanel content)
    {
        int row = stack.RowCount++;
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var group = new GroupBox { Text = (string)content.Tag, Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(6), Margin = new Padding(0, 0, 0, 9) };
        group.Controls.Add(content);
        stack.Controls.Add(group, 0, row);
    }
    private static Control LabeledControl(Control control, string suffix)
    {
        var panel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        control.Margin = new Padding(0, 0, 6, 0);
        panel.Controls.Add(control);
        panel.Controls.Add(new Label { Text = suffix, AutoSize = true, Margin = new Padding(0, 5, 0, 0), ForeColor = SystemColors.GrayText });
        return panel;
    }
    private static void AddRow(TableLayoutPanel t, string label, Control control)
    {
        int r = t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 12, 5) }, 0, r);
        control.Anchor = AnchorStyles.Left;
        control.Margin = new Padding(3, 3, 3, 5);
        t.Controls.Add(control, 1, r);
    }
    private static void AddWide(TableLayoutPanel t, Control c)
    {
        int r = t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        c.Margin = new Padding(3, 4, 3, 5);
        t.Controls.Add(c, 0, r);
        t.SetColumnSpan(c, 2);
    }
    private static void AddDescription(TableLayoutPanel t, string text)
    {
        AddWide(t, new Label { Text = text, AutoSize = true, ForeColor = SystemColors.GrayText, MaximumSize = new Size(330, 0) });
    }

    public XmlNode GetSettings(XmlDocument d)
    {
        var p = d.CreateElement("Settings"); Add(d,p,"Version",3);
        Add(d,p,"StartRank",StartRank); Add(d,p,"EntryCount",EntryCount); Add(d,p,"SurroundingMode",SurroundingMode); Add(d,p,"SurroundingAbove",SurroundingAbove); Add(d,p,"SurroundingBelow",SurroundingBelow); Add(d,p,"HighlightUsername",HighlightUsername);
        Add(d,p,"RefreshMinutes",RefreshMinutes); Add(d,p,"RowHeight",RowHeight); Add(d,p,"RankWidth",RankWidth); Add(d,p,"TimeWidth",TimeWidth); Add(d,p,"FilterPlatform",FilterPlatform); Add(d,p,"FilterRegion",FilterRegion); Add(d,p,"FilterVariables",FilterVariables); Add(d,p,"FilterSubcategories",FilterSubcategories); Add(d,p,"ShowHeader",ShowHeader); Add(d,p,"AlternateRows",AlternateRows); Add(d,p,"AlternateOpacity",AlternateOpacity); Add(d,p,"TimingMethod",TimingMethod);
        Add(d,p,"RankAlignment",RankAlignment); Add(d,p,"PlayerAlignment",PlayerAlignment); Add(d,p,"TimeAlignment",TimeAlignment); Add(d,p,"TimeFormat",TimeFormat); Add(d,p,"ShowMilliseconds",ShowMilliseconds); Add(d,p,"HoursOnlyWhenNeeded",HoursOnlyWhenNeeded); Add(d,p,"PlayerNameMode",PlayerNameMode); Add(d,p,"ShowCountryFlag",ShowCountryFlag); Add(d,p,"HighlightBold",HighlightBold); Add(d,p,"ShowHighlightBackground",ShowHighlightBackground);
        AddColor(d,p,"HeaderTextColor",HeaderTextColor); AddColor(d,p,"RowTextColor",RowTextColor); AddColor(d,p,"RankTextColor",RankTextColor); AddColor(d,p,"TimeTextColor",TimeTextColor); AddColor(d,p,"BackgroundColor",BackgroundColor); AddColor(d,p,"AlternateRowColor",AlternateRowColor); AddColor(d,p,"HighlightTextColor",HighlightTextColor); AddColor(d,p,"HighlightBackgroundColor",HighlightBackgroundColor); return p;
    }
    public void SetSettings(XmlNode n)
    {
        StartRank=Read(n,"StartRank",1); EntryCount=Read(n,"EntryCount",Read(n,"TopCount",5)); SurroundingMode=Read(n,"SurroundingMode",false); SurroundingAbove=Read(n,"SurroundingAbove",2); SurroundingBelow=Read(n,"SurroundingBelow",2); HighlightUsername=Read(n,"HighlightUsername",""); RefreshMinutes=Read(n,"RefreshMinutes",5); RowHeight=Read(n,"RowHeight",27); RankWidth=Read(n,"RankWidth",70); TimeWidth=Read(n,"TimeWidth",85);
        FilterPlatform=Read(n,"FilterPlatform",false); FilterRegion=Read(n,"FilterRegion",false); FilterVariables=Read(n,"FilterVariables",true); FilterSubcategories=Read(n,"FilterSubcategories",true); ShowHeader=Read(n,"ShowHeader",true); AlternateRows=Read(n,"AlternateRows",true); AlternateOpacity=Read(n,"AlternateOpacity",28); TimingMethod=Read(n,"TimingMethod","Leaderboard Default"); RankAlignment=Read(n,"RankAlignment","Left"); PlayerAlignment=Read(n,"PlayerAlignment","Left"); TimeAlignment=Read(n,"TimeAlignment","Right"); TimeFormat=Read(n,"TimeFormat","Colon (1:23:45)"); ShowMilliseconds=Read(n,"ShowMilliseconds",false); HoursOnlyWhenNeeded=Read(n,"HoursOnlyWhenNeeded",true); PlayerNameMode=Read(n,"PlayerNameMode","Speedrun.com username"); ShowCountryFlag=Read(n,"ShowCountryFlag",false); HighlightBold=Read(n,"HighlightBold",true); ShowHighlightBackground=Read(n,"ShowHighlightBackground",true);
        HeaderTextColor=ReadColor(n,"HeaderTextColor",Color.White); RowTextColor=ReadColor(n,"RowTextColor",Color.White); RankTextColor=ReadColor(n,"RankTextColor",Color.White); TimeTextColor=ReadColor(n,"TimeTextColor",Color.White); BackgroundColor=ReadColor(n,"BackgroundColor",Color.Transparent); AlternateRowColor=ReadColor(n,"AlternateRowColor",Color.White); HighlightTextColor=ReadColor(n,"HighlightTextColor",Color.White); HighlightBackgroundColor=ReadColor(n,"HighlightBackgroundColor",Color.FromArgb(100,70,130,180)); RefreshColorButtons();
    }
    public int GetSettingsHashCode() => GetSettings(new XmlDocument()).InnerXml.GetHashCode();
    private static void Add(XmlDocument d, XmlElement p, string n, object v) { var e=d.CreateElement(n); e.InnerText=Convert.ToString(v,CultureInfo.InvariantCulture); p.AppendChild(e); }
    private static void AddColor(XmlDocument d, XmlElement p, string n, Color c) => Add(d,p,n,c.ToArgb());
    private static Color ReadColor(XmlNode n,string key,Color f) => Color.FromArgb(Read(n,key,f.ToArgb()));
    private static T Read<T>(XmlNode n,string key,T f) { string s=n?[key]?.InnerText; if(string.IsNullOrWhiteSpace(s)) return f; try { if(typeof(T)==typeof(bool)) return (T)(object)bool.Parse(s); if(typeof(T)==typeof(int)) return (T)(object)int.Parse(s,CultureInfo.InvariantCulture); if(typeof(T)==typeof(string)) return (T)(object)s; } catch{} return f; }
}
