using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

/// <summary>
/// Main network manager for Photon Fusion
/// Handles connection, lobby creation, joining, and session management
/// </summary>
public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }
    
    [Header("Photon Settings")]
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    
    private NetworkRunner runner;
    private GameSessionData currentSession;
    
    // Events
    public event Action OnConnectedToLobby;
    public event Action<GameSessionData> OnSessionCreated;
    public event Action<GameSessionData> OnSessionJoined;
    public event Action<List<SessionInfo>> OnSessionListReceived;
    public event Action OnDisconnected;
    
    // Local player info
    public string LocalPlayerID { get; private set; }
    public string LocalPlayerName { get; private set; }
    public bool IsHost => runner != null && runner.IsServer;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Connect to Photon servers
    /// </summary>
    public async Task<bool> ConnectToPhoton(string playerName)
    {
        LocalPlayerName = playerName;
        
        // Create NetworkRunner if it doesn't exist
        if (runner == null)
        {
            runner = Instantiate(networkRunnerPrefab);
            runner.name = "NetworkRunner";
            DontDestroyOnLoad(runner.gameObject);
        }
        
        // Start connection to Photon Cloud
        var result = await runner.JoinSessionLobby(SessionLobby.Shared);
        
        if (result.Ok)
        {
            Debug.Log("Connected to Photon Lobby");
            OnConnectedToLobby?.Invoke();
            return true;
        }
        else
        {
            Debug.LogError($"Failed to connect: {result.ShutdownReason}");
            return false;
        }
    }
    
    /// <summary>
    /// Create a new game session (Host)
    /// </summary>
    public async Task<bool> CreateSession(string lobbyName, string password, MapLayout mapLayout)
    {
        // Create session data
        currentSession = new GameSessionData(lobbyName, password, LocalPlayerName);
        currentSession.mapLayout = mapLayout;
        currentSession.maxPlayers = mapLayout.teams.Count;
        
        // Create Fusion session
        var customProps = CreateSessionProperties(currentSession);
        
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared, // Shared mode (host authoritative)
            SessionName = lobbyName,
            SessionProperties = customProps,
            PlayerCount = currentSession.maxPlayers,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        
        if (result.Ok)
        {
            LocalPlayerID = runner.LocalPlayer.ToString();
            Debug.Log($"Session created: {lobbyName}");
            OnSessionCreated?.Invoke(currentSession);
            return true;
        }
        else
        {
            Debug.LogError($"Failed to create session: {result.ShutdownReason}");
            return false;
        }
    }
    
    /// <summary>
    /// Join an existing session
    /// </summary>
    public async Task<bool> JoinSession(SessionInfo sessionInfo, string password = "")
    {
        // Check password
        string sessionPassword = GetSessionProperty<string>(sessionInfo, "Password");
        if (!string.IsNullOrEmpty(sessionPassword) && sessionPassword != password)
        {
            Debug.LogError("Incorrect password");
            return false;
        }
        
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionInfo.Name,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        
        if (result.Ok)
        {
            LocalPlayerID = runner.LocalPlayer.ToString();
            
            // Retrieve session data from session properties
            currentSession = DeserializeSessionData(sessionInfo);
            
            Debug.Log($"Joined session: {sessionInfo.Name}");
            OnSessionJoined?.Invoke(currentSession);
            return true;
        }
        else
        {
            Debug.LogError($"Failed to join session: {result.ShutdownReason}");
            return false;
        }
    }
    
    /// <summary>
    /// Get list of available sessions
    /// </summary>
    public void RefreshSessionList()
    {
        // Photon Fusion automatically updates session list via OnSessionListUpdated callback
        // This method is just a trigger for UI refresh
        Debug.Log("Refreshing session list...");
    }
    
    /// <summary>
    /// Assign local player to a team
    /// </summary>
    public void SelectTeam(int teamIndex)
    {
        if (!IsHost)
        {
            // Send RPC to host to assign team
            // TODO: Implement RPC
            Debug.Log($"Requesting team {teamIndex}");
        }
        else
        {
            // Host can assign directly
            AssignPlayerToTeam(LocalPlayerID, teamIndex);
        }
    }
    
    /// <summary>
    /// Host: Assign a player to a team
    /// </summary>
    public void AssignPlayerToTeam(string playerID, int teamIndex)
    {
        if (!IsHost) return;
        
        if (currentSession.AssignPlayerToTeam(playerID, teamIndex))
        {
            // Update session properties
            UpdateSessionProperties();
            Debug.Log($"Player {playerID} assigned to team {teamIndex}");
        }
        else
        {
            Debug.LogWarning($"Team {teamIndex} is already taken");
        }
    }
    
    /// <summary>
    /// Host: Start the game (transition from team selection to game)
    /// </summary>
    public void StartGame()
    {
        if (!IsHost) return;
        
        currentSession.isInTeamSelection = false;
        UpdateSessionProperties();
        
        // Load game scene
        // TODO: Implement scene transition
        Debug.Log("Starting game...");
    }
    
    /// <summary>
    /// Disconnect from session
    /// </summary>
    public async void Disconnect()
    {
        if (runner != null)
        {
            await runner.Shutdown();
            runner = null;
        }
        
        currentSession = null;
        OnDisconnected?.Invoke();
    }
    
    // ===== HELPER METHODS =====
    
    private Dictionary<string, SessionProperty> CreateSessionProperties(GameSessionData session)
    {
        SerializedGameSession serialized = SerializedGameSession.Serialize(session);
        
        return new Dictionary<string, SessionProperty>
        {
            { "LobbyName", serialized.lobbyName },
            { "Password", serialized.password },
            { "HostName", serialized.hostPlayerName },
            { "MaxPlayers", serialized.maxPlayers },
            { "MapName", serialized.mapName },
            { "PresetName", serialized.presetName },
            { "TeamsJSON", serialized.teamsJSON },
            { "TileOwnershipJSON", serialized.tileOwnershipJSON },
            { "PlayerAssignmentsJSON", serialized.playerAssignmentsJSON },
            { "IsInTeamSelection", serialized.isInTeamSelection },
            { "CurrentTurn", serialized.currentTurn }
        };
    }
    
    private void UpdateSessionProperties()
    {
        if (runner == null || currentSession == null) return;
        
        var props = CreateSessionProperties(currentSession);
        runner.SessionInfo.UpdateCustomProperties(props);
    }
    
    private GameSessionData DeserializeSessionData(SessionInfo sessionInfo)
    {
        SerializedGameSession serialized = new SerializedGameSession
        {
            lobbyName = GetSessionProperty<string>(sessionInfo, "LobbyName"),
            password = GetSessionProperty<string>(sessionInfo, "Password"),
            hostPlayerName = GetSessionProperty<string>(sessionInfo, "HostName"),
            maxPlayers = GetSessionProperty<int>(sessionInfo, "MaxPlayers"),
            mapName = GetSessionProperty<string>(sessionInfo, "MapName"),
            presetName = GetSessionProperty<string>(sessionInfo, "PresetName"),
            teamsJSON = GetSessionProperty<string>(sessionInfo, "TeamsJSON"),
            tileOwnershipJSON = GetSessionProperty<string>(sessionInfo, "TileOwnershipJSON"),
            playerAssignmentsJSON = GetSessionProperty<string>(sessionInfo, "PlayerAssignmentsJSON"),
            isInTeamSelection = GetSessionProperty<bool>(sessionInfo, "IsInTeamSelection"),
            currentTurn = GetSessionProperty<int>(sessionInfo, "CurrentTurn")
        };
        
        return SerializedGameSession.Deserialize(serialized);
    }
    
    private T GetSessionProperty<T>(SessionInfo sessionInfo, string key)
    {
        if (sessionInfo.Properties.TryGetValue(key, out var value))
        {
            return (T)value.PropertyValue;
        }
        return default(T);
    }
    
    // ===== FUSION CALLBACKS =====
    
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"Session list updated: {sessionList.Count} sessions");
        OnSessionListReceived?.Invoke(sessionList);
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player joined: {player}");
        
        if (IsHost)
        {
            // Send current session state to new player
            UpdateSessionProperties();
        }
    }
    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player left: {player}");
        
        if (IsHost && currentSession != null)
        {
            // Remove player's team assignment
            string playerID = player.ToString();
            if (currentSession.playerTeamAssignments.ContainsKey(playerID))
            {
                currentSession.playerTeamAssignments.Remove(playerID);
                UpdateSessionProperties();
            }
        }
    }
    
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server");
    }
    
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Disconnected: {reason}");
        OnDisconnected?.Invoke();
    }

    /// <summary>
    /// Deselect current team
    /// </summary>
    public void DeselectTeam()
    {
        if (!IsHost)
        {
            // Send RPC to host to deselect
            Debug.Log("Requesting team deselection");
        }
        else
        {
            // Host can deselect directly
            currentSession.DeassignPlayer(LocalPlayerID);
            UpdateSessionProperties();
        }
    }
    
    // Required INetworkRunnerCallbacks (empty implementations)
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
