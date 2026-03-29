using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Available team types in the game
/// </summary>
public enum TeamType
{
    GovernmentHeroes,
    CorporateHeroes,
    StreetHeroes,
    OldGuardVillains,
    NewBloodVillains,
    WildCardVillains
}

/// <summary>
/// Available team colors
/// </summary>
public enum TeamColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange,
    Black,
    White
}

/// <summary>
/// Configuration for a single team in the game
/// </summary>
[Serializable]
public class TeamConfig
{
    public TeamType teamType;
    public TeamColor teamColor;
    public int startingResources;
    public List<string> controlledTiles; // Tile IDs this team starts with
    
    // Derived properties
    public string TeamName => GetTeamName(teamType);
    public Color UnityColor => GetUnityColor(teamColor);
    public string DisplayName => $"{TeamName} ({teamColor})";
    
    public TeamConfig(TeamType type, TeamColor color, int resources = 0)
    {
        teamType = type;
        teamColor = color;
        startingResources = resources;
        controlledTiles = new List<string>();
    }
    
    /// <summary>
    /// Get display name for team type
    /// </summary>
    public static string GetTeamName(TeamType type)
    {
        switch (type)
        {
            case TeamType.GovernmentHeroes:
                return "Government Heroes";
            case TeamType.CorporateHeroes:
                return "Corporate Heroes";
            case TeamType.StreetHeroes:
                return "Street Heroes";
            case TeamType.OldGuardVillains:
                return "Old Guard Villains";
            case TeamType.NewBloodVillains:
                return "New Blood Villains";
            case TeamType.WildCardVillains:
                return "Wild Card Villains";
            default:
                return "Unknown Team";
        }
    }
    
    /// <summary>
    /// Get Unity Color from TeamColor enum
    /// </summary>
    public static Color GetUnityColor(TeamColor color)
    {
        switch (color)
        {
            case TeamColor.Red:
                return new Color(0.8f, 0.2f, 0.2f);
            case TeamColor.Blue:
                return new Color(0.2f, 0.4f, 0.8f);
            case TeamColor.Green:
                return new Color(0.2f, 0.7f, 0.3f);
            case TeamColor.Yellow:
                return new Color(0.9f, 0.8f, 0.2f);
            case TeamColor.Purple:
                return new Color(0.6f, 0.2f, 0.8f);
            case TeamColor.Orange:
                return new Color(0.9f, 0.5f, 0.1f);
            case TeamColor.Black:
                return new Color(0.15f, 0.15f, 0.15f);
            case TeamColor.White:
                return new Color(0.9f, 0.9f, 0.9f);
            default:
                return Color.gray;
        }
    }
    
    /// <summary>
    /// Get default color for team type
    /// </summary>
    public static TeamColor GetDefaultColor(TeamType type)
    {
        switch (type)
        {
            case TeamType.GovernmentHeroes:
                return TeamColor.Blue;
            case TeamType.CorporateHeroes:
                return TeamColor.Yellow;
            case TeamType.StreetHeroes:
                return TeamColor.Green;
            case TeamType.OldGuardVillains:
                return TeamColor.Red;
            case TeamType.NewBloodVillains:
                return TeamColor.Purple;
            case TeamType.WildCardVillains:
                return TeamColor.Orange;
            default:
                return TeamColor.White;
        }
    }
}

/// <summary>
/// Map layout configuration for a game session
/// </summary>
[Serializable]
public class MapLayout
{
    public string mapName;
    public string presetName; // "2 Teams Default" or "Custom"
    public List<TeamConfig> teams;
    
    // Tile control: TileID -> TeamIndex in teams list (-1 = neutral)
    public Dictionary<string, int> tileOwnership;
    
    public MapLayout(string map = "Default", string preset = "2 Teams Default")
    {
        mapName = map;
        presetName = preset;
        teams = new List<TeamConfig>();
        tileOwnership = new Dictionary<string, int>();
    }
    
    /// <summary>
    /// Create the default 2-team preset
    /// </summary>
    public static MapLayout CreateDefault2TeamPreset()
    {
        MapLayout layout = new MapLayout("Default", "2 Teams Default");
        
        // Add Government Heroes (Blue)
        TeamConfig heroes = new TeamConfig(TeamType.GovernmentHeroes, TeamColor.Blue, 0);
        heroes.controlledTiles = new List<string> { "NS", "NE", "MT", "CH", "DK", "BW" };
        layout.teams.Add(heroes);
        
        // Add Old Guard Villains (Red)
        TeamConfig villains = new TeamConfig(TeamType.OldGuardVillains, TeamColor.Red, 0);
        villains.controlledTiles = new List<string> { "DW", "SS", "DE", "AR", "BS", "HR" };
        layout.teams.Add(villains);
        
        // Set tile ownership
        for (int i = 0; i < layout.teams.Count; i++)
        {
            foreach (string tileID in layout.teams[i].controlledTiles)
            {
                layout.tileOwnership[tileID] = i;
            }
        }
        
        return layout;
    }
    
    /// <summary>
    /// Assign a tile to a team
    /// </summary>
    public void AssignTile(string tileID, int teamIndex)
    {
        if (teamIndex < 0 || teamIndex >= teams.Count)
        {
            // Remove tile (make neutral)
            if (tileOwnership.ContainsKey(tileID))
            {
                int oldTeam = tileOwnership[tileID];
                teams[oldTeam].controlledTiles.Remove(tileID);
                tileOwnership.Remove(tileID);
            }
        }
        else
        {
            // Remove from old team if owned
            if (tileOwnership.ContainsKey(tileID))
            {
                int oldTeam = tileOwnership[tileID];
                teams[oldTeam].controlledTiles.Remove(tileID);
            }
            
            // Assign to new team
            tileOwnership[tileID] = teamIndex;
            if (!teams[teamIndex].controlledTiles.Contains(tileID))
            {
                teams[teamIndex].controlledTiles.Add(tileID);
            }
        }
    }
    
    /// <summary>
    /// Get which team owns a tile (-1 = neutral)
    /// </summary>
    public int GetTileOwner(string tileID)
    {
        return tileOwnership.ContainsKey(tileID) ? tileOwnership[tileID] : -1;
    }
}
