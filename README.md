# LiveSplit.Leaderboard

A LiveSplit desktop component that shows the top Speedrun.com runs for the game/category selected in the current splits, styled as a compact **Rank / Player / Time** table.

## Features

- Top 1–20 leaderboard entries
- Top-three trophy markers
- Automatic game/category detection from the current LiveSplit run
- Optional platform, emulator, region, variable, and subcategory filtering
- Real Time, RTA without loads, Game Time, or leaderboard-default timing
- Configurable refresh interval and row height
- Layout colors/fonts and drop shadows are respected

## Build

This repository is intended to sit next to a recursive clone of LiveSplit:

```text
parent/
├─ LiveSplit/                 # git clone --recursive https://github.com/LiveSplit/LiveSplit.git
└─ LiveSplit.Leaderboard/
```

Requirements used by current LiveSplit:

- .NET 10 SDK
- .NET Framework 4.8.1 Developer Pack

Build:

```powershell
dotnet build .\LiveSplit.Leaderboard.slnx -c Release
```

To use a different LiveSplit checkout location:

```powershell
dotnet build .\LiveSplit.Leaderboard.slnx -c Release -p:LsRoot="C:\path\to\LiveSplit"
```

Copy the resulting `LiveSplit.Leaderboard.dll` into LiveSplit's `Components` folder, restart LiveSplit, then add **Information → Leaderboard** in the Layout Editor.

## Notes

The game and category must be linked to Speedrun.com in LiveSplit's Splits Editor. The component fetches data in the background and refreshes every five minutes by default.
