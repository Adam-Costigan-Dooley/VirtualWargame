using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Session data for a multiplayer game lobby
/// This data is synced across all players via Photon
/// </summary>
[Serializable]
public class GameSessionData
{
    // Lobby info
    public string lobbyName;
    public string password; // Empty = no password
    public string hostPlayerName;
    public int maxPlayers; // 2-6 based on teams selected
    
    // Map configuration
    public MapLayout mapLayout;
    
    // Player assignments
    public Dictionary<string, int> playerTeamAssignments; // PlayerID -> TeamIndex
    
    // Game state
    public bool isInTeamSelection; // true = team selection, false = game started
    public int currentTurn;
    
    public GameSessionData(string name, string pass, string host)
    {
        lobbyName = name;
        password = pass;
        hostPlayerName = host;
        maxPlayers = 2; // Default, updated when teams are selected
        
        mapLayout = MapLayout.CreateDefault2TeamPreset();
        playerTeamAssignments = new Dictionary<string, int>();
        
        isInTeamSelection = true;
        currentTurn = 1;
    }
    
    /// <summary>
    /// Check if a team is already assigned to a player
    /// </summary>
    public bool IsTeamTaken(int teamIndex)
    {
        return playerTeamAssignments.ContainsValue(teamIndex);
    }
    
    /// <summary>
    /// Assign a player to a team
    /// </summary>
    public bool AssignPlayerToTeam(string playerID, int teamIndex)
    {
        // Check if team is already taken
        if (IsTeamTaken(teamIndex))
            return false;
        
        // Remove player's old assignment if exists
        if (playerTeamAssignments.ContainsKey(playerID))
        {
            playerTeamAssignments.Remove(playerID);
        }
        
        // Assign to new team
        playerTeamAssignments[playerID] = teamIndex;
        return true;
    }

    /// <summary>
    /// Remove a player's team assignment
    /// </summary>
    public void DeassignPlayer(string playerID)
    {
        if (playerTeamAssignments.ContainsKey(playerID))
        {
            playerTeamAssignments.Remove(playerID);
        }
    }
    
    /// <summary>
    /// Get team index for a player (-1 = not assigned)
    /// </summary>
    public int GetPlayerTeam(string playerID)
    {
        return playerTeamAssignments.ContainsKey(playerID) ? playerTeamAssignments[playerID] : -1;
    }
    
    /// <summary>
    /// Check if all players have selected teams
    /// </summary>
    public bool AllPlayersReady(int connectedPlayerCount)
    {
        return playerTeamAssignments.Count == connectedPlayerCount && 
               playerTeamAssignments.Count >= 2; // Minimum 2 players
    }
}

/// <summary>
/// Serializable version for Photon Custom Properties
/// (Photon doesn't support complex objects directly)
/// </summary>
[Serializable]
public class SerializedGameSession
{
    public string lobbyName;
    public string password;
    public string hostPlayerName;
    public int maxPlayers;
    public string mapName;
    public string presetName;
    
    // Serialized team data (JSON)
    public string teamsJSON;
    public string tileOwnershipJSON;
    public string playerAssignmentsJSON;
    
    public bool isInTeamSelection;
    public int currentTurn;
    
    /// <summary>
    /// Convert GameSessionData to serializable format
    /// </summary>
    public static SerializedGameSession Serialize(GameSessionData data)
    {
        SerializedGameSession serialized = new SerializedGameSession();
        
        serialized.lobbyName = data.lobbyName;
        serialized.password = data.password;
        serialized.hostPlayerName = data.hostPlayerName;
        serialized.maxPlayers = data.maxPlayers;
        
        serialized.mapName = data.mapLayout.mapName;
        serialized.presetName = data.mapLayout.presetName;
        
        // Serialize complex data to JSON
        serialized.teamsJSON = JsonUtility.ToJson(new TeamListWrapper { teams = data.mapLayout.teams });
        serialized.tileOwnershipJSON = JsonUtility.ToJson(new TileOwnershipWrapper { ownership = data.mapLayout.tileOwnership });
        serialized.playerAssignmentsJSON = JsonUtility.ToJson(new PlayerAssignmentWrapper { assignments = data.playerTeamAssignments });
        
        serialized.isInTeamSelection = data.isInTeamSelection;
        serialized.currentTurn = data.currentTurn;
        
        return serialized;
    }
    
    /// <summary>
    /// Convert serialized data back to GameSessionData
    /// </summary>
    public static GameSessionData Deserialize(SerializedGameSession serialized)
    {
        GameSessionData data = new GameSessionData(
            serialized.lobbyName,
            serialized.password,
            serialized.hostPlayerName
        );
        
        data.maxPlayers = serialized.maxPlayers;
        data.mapLayout.mapName = serialized.mapName;
        data.mapLayout.presetName = serialized.presetName;
        
        // Deserialize JSON
        TeamListWrapper teamWrapper = JsonUtility.FromJson<TeamListWrapper>(serialized.teamsJSON);
        data.mapLayout.teams = teamWrapper.teams;
        
        TileOwnershipWrapper tileWrapper = JsonUtility.FromJson<TileOwnershipWrapper>(serialized.tileOwnershipJSON);
        data.mapLayout.tileOwnership = tileWrapper.ownership;
        
        PlayerAssignmentWrapper playerWrapper = JsonUtility.FromJson<PlayerAssignmentWrapper>(serialized.playerAssignmentsJSON);
        data.playerTeamAssignments = playerWrapper.assignments;
        
        data.isInTeamSelection = serialized.isInTeamSelection;
        data.currentTurn = serialized.currentTurn;
        
        return data;
    }
}

// Wrapper classes for JSON serialization (Unity doesn't serialize dictionaries/lists directly)
[Serializable]
public class TeamListWrapper
{
    public List<TeamConfig> teams;
}

[Serializable]
public class TileOwnershipWrapper
{
    public Dictionary<string, int> ownership;
}

[Serializable]
public class PlayerAssignmentWrapper
{
    public Dictionary<string, int> assignments;
}
