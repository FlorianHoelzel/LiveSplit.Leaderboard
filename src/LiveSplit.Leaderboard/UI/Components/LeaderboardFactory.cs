using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;

[assembly: ComponentFactory(typeof(LiveSplit.Leaderboard.UI.Components.LeaderboardFactory))]

namespace LiveSplit.Leaderboard.UI.Components;

public sealed class LeaderboardFactory : IComponentFactory
{
    public string ComponentName => "Leaderboard";
    public string Description => "Displays the top Speedrun.com leaderboard entries for the current game and category.";
    public ComponentCategory Category => ComponentCategory.Information;
    public IComponent Create(LiveSplitState state) => new LeaderboardComponent(state);
    public string UpdateName => ComponentName;
    public string XMLURL => string.Empty;
    public string UpdateURL => string.Empty;
    public Version Version => new Version(0, 1, 0);
}
