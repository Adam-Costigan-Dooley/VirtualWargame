using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Manager for host to configure map layout before creating lobby
/// This screen is host-only and appears before the lobby is created
/// </summary>
public class MapLayoutSetupUI : MonoBehaviour
{
    [Header("Map Selection")]
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private TMP_Dropdown presetDropdown;
    
    [Header("Team Configuration Panel")]
    [SerializeField] private GameObject teamConfigPanel;
    [SerializeField] private Transform teamListContainer;
    [SerializeField] private GameObject teamConfigItemPrefab;
    
    [Header("Map View")]
    [SerializeField] private Image mapImage;
    [SerializeField] private Transform tileMarkersContainer;
    [SerializeField] private GameObject tileMarkerPrefab;
    
    [Header("Team Switcher")]
    [SerializeField] private TMP_Dropdown currentTeamDropdown;
    [SerializeField] private Button neutralizeButton;
    
    [Header("Buttons")]
    [SerializeField] private Button submitLayoutButton;
    [SerializeField] private Button backButton;
    
    [Header("Lobby Creation")]
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private TMP_InputField passwordInput;
    
    private MapLayout currentLayout;
    private List<TeamConfigUI> teamConfigItems = new List<TeamConfigUI>();
    private Dictionary<string, GameObject> tileMarkers = new Dictionary<string, GameObject>();
    private int currentSelectedTeamIndex = -1; // -1 = neutralize mode
    
    private void Start()
    {
        SetupMapDropdown();
        SetupPresetDropdown();
        SetupButtons();
        
        // Start with default preset
        LoadPreset(0); // 2 Teams Default
    }
    
    private void SetupMapDropdown()
    {
        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(new List<string> { "Default" });
        mapDropdown.value = 0;
        
        mapDropdown.onValueChanged.AddListener(OnMapChanged);
    }
    
    private void SetupPresetDropdown()
    {
        presetDropdown.ClearOptions();
        presetDropdown.AddOptions(new List<string> 
        { 
            "2 Teams Default",
            "Create New Layout"
        });
        presetDropdown.value = 0;
        
        presetDropdown.onValueChanged.AddListener(OnPresetChanged);
    }
    
    private void SetupButtons()
    {
        submitLayoutButton.onClick.AddListener(OnSubmitLayout);
        backButton.onClick.AddListener(OnBack);
        neutralizeButton.onClick.AddListener(OnNeutralizeMode);
        
        currentTeamDropdown.onValueChanged.AddListener(OnTeamSwitched);
    }
    
    private void OnMapChanged(int index)
    {
        // Currently only one map, but ready for expansion
        Debug.Log($"Map changed to: {mapDropdown.options[index].text}");
    }
    
    private void OnPresetChanged(int index)
    {
        LoadPreset(index);
    }
    
    private void LoadPreset(int presetIndex)
    {
        if (presetIndex == 0) // 2 Teams Default
        {
            currentLayout = MapLayout.CreateDefault2TeamPreset();
            teamConfigPanel.SetActive(false); // Hide team config for preset
            
            DisplayMapLayout();
            UpdateTeamSwitcher();
        }
        else // Create New Layout
        {
            currentLayout = new MapLayout("Default", "Create New Layout");
            teamConfigPanel.SetActive(true); // Show team config
            
            CreateTeamConfigItems();
            DisplayMapLayout();
            UpdateTeamSwitcher();
        }
    }
    
    /// <summary>
    /// Create UI items for team configuration (checkboxes, colors, resources)
    /// </summary>
    private void CreateTeamConfigItems()
    {
        // Clear existing
        foreach (var item in teamConfigItems)
        {
            Destroy(item.gameObject);
        }
        teamConfigItems.Clear();
        
        // Create item for each team type
        TeamType[] allTeams = new TeamType[]
        {
            TeamType.GovernmentHeroes,
            TeamType.CorporateHeroes,
            TeamType.StreetHeroes,
            TeamType.OldGuardVillains,
            TeamType.NewBloodVillains,
            TeamType.WildCardVillains
        };
        
        foreach (TeamType teamType in allTeams)
        {
            GameObject itemObj = Instantiate(teamConfigItemPrefab, teamListContainer);
            TeamConfigUI item = itemObj.GetComponent<TeamConfigUI>();
            
            if (item != null)
            {
                item.Initialize(teamType, this);
                teamConfigItems.Add(item);
            }
        }
    }
    
    /// <summary>
    /// Toggle a team on/off
    /// </summary>
    public void ToggleTeam(TeamType teamType, bool enabled)
    {
        if (enabled)
        {
            // Add team with default color and 0 resources
            TeamColor defaultColor = TeamConfig.GetDefaultColor(teamType);
            TeamConfig team = new TeamConfig(teamType, defaultColor, 0);
            currentLayout.teams.Add(team);
        }
        else
        {
            // Remove team
            TeamConfig toRemove = currentLayout.teams.Find(t => t.teamType == teamType);
            if (toRemove != null)
            {
                // Clear all tile assignments for this team
                int teamIndex = currentLayout.teams.IndexOf(toRemove);
                List<string> tilesToClear = new List<string>(toRemove.controlledTiles);
                foreach (string tileID in tilesToClear)
                {
                    currentLayout.AssignTile(tileID, -1); // Neutralize
                }
                
                currentLayout.teams.Remove(toRemove);
            }
        }
        
        UpdateTeamSwitcher();
        DisplayMapLayout();
    }
    
    /// <summary>
    /// Update team color
    /// </summary>
    public void SetTeamColor(TeamType teamType, TeamColor color)
    {
        TeamConfig team = currentLayout.teams.Find(t => t.teamType == teamType);
        if (team != null)
        {
            team.teamColor = color;
            DisplayMapLayout();
            UpdateTeamSwitcher();
        }
    }
    
    /// <summary>
    /// Update team starting resources
    /// </summary>
    public void SetTeamResources(TeamType teamType, int resources)
    {
        TeamConfig team = currentLayout.teams.Find(t => t.teamType == teamType);
        if (team != null)
        {
            team.startingResources = Mathf.Clamp(resources, 0, 100);
        }
    }
    
    /// <summary>
    /// Update the team switcher dropdown
    /// </summary>
    private void UpdateTeamSwitcher()
    {
        currentTeamDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        foreach (TeamConfig team in currentLayout.teams)
        {
            options.Add(team.DisplayName);
        }
        
        if (options.Count > 0)
        {
            currentTeamDropdown.AddOptions(options);
            currentTeamDropdown.value = 0;
            currentSelectedTeamIndex = 0;
        }
        else
        {
            currentSelectedTeamIndex = -1;
        }
        
        currentTeamDropdown.interactable = options.Count > 0;
        neutralizeButton.interactable = true;
    }
    
    private void OnTeamSwitched(int index)
    {
        currentSelectedTeamIndex = index;
        Debug.Log($"Now assigning tiles to: {currentLayout.teams[index].DisplayName}");
    }
    
    private void OnNeutralizeMode()
    {
        currentSelectedTeamIndex = -1;
        Debug.Log("Now neutralizing tiles (click to remove control)");
    }
    
    /// <summary>
    /// Display the map with current tile assignments
    /// </summary>
    private void DisplayMapLayout()
    {
        // Clear existing markers
        foreach (var marker in tileMarkers.Values)
        {
            Destroy(marker);
        }
        tileMarkers.Clear();
        
        // Tile offsets for proper positioning
        Dictionary<string, Vector2> tileOffsets = new Dictionary<string, Vector2>()
        {
            {"NS", new Vector2(23, 59)},
            {"NE", new Vector2(0, 0)},
            {"DK", new Vector2(-55, 36)},
            {"MT", new Vector2(-58, 78)},
            {"CH", new Vector2(69, 134)},
            {"DW", new Vector2(-161, 74)},
            {"BW", new Vector2(-309, 138)},
            {"SS", new Vector2(-89, 62)},
            {"DE", new Vector2(-240, 157)},
            {"AR", new Vector2(21, 117)},
            {"BS", new Vector2(-246, 155)},
            {"HR", new Vector2(-110, 80)}
        };
        
        // Create tile markers with colors
        Dictionary<string, Vector2> tilePositions = GetTilePositions();
        
        foreach (var kvp in tilePositions)
        {
            string tileID = kvp.Key;
            Vector2 position = kvp.Value;
            
            GameObject marker = Instantiate(tileMarkerPrefab, tileMarkersContainer);
            RectTransform rt = marker.GetComponent<RectTransform>();
            
            rt.anchorMin = position;
            rt.anchorMax = position;
            
            // Apply offset if it exists
            if (tileOffsets.ContainsKey(tileID))
            {
                rt.anchoredPosition = tileOffsets[tileID];
            }
            else
            {
                rt.anchoredPosition = Vector2.zero;
            }
            
            // Set tile name
            TextMeshProUGUI nameText = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = tileID;
            }
            
            // Set color based on ownership
            int ownerIndex = currentLayout.GetTileOwner(tileID);
            Image colorIndicator = marker.transform.Find("ColorIndicator")?.GetComponent<Image>();
            if (colorIndicator != null)
            {
                if (ownerIndex >= 0)
                {
                    colorIndicator.color = currentLayout.teams[ownerIndex].UnityColor;
                }
                else
                {
                    colorIndicator.color = Color.white; // Neutral
                }
            }
            
            // Add click handler
            Button btn = marker.GetComponent<Button>();
            if (btn != null)
            {
                string tileCopy = tileID; // Closure copy
                btn.onClick.AddListener(() => OnTileClicked(tileCopy));
            }
            
            tileMarkers[tileID] = marker;
        }
    }
    
    /// <summary>
    /// Handle tile click to assign/unassign
    /// </summary>
    private void OnTileClicked(string tileID)
    {
        if (currentLayout.teams.Count == 0)
        {
            Debug.LogWarning("No teams configured. Add teams first!");
            return;
        }
        
        currentLayout.AssignTile(tileID, currentSelectedTeamIndex);
        DisplayMapLayout();
        
        if (currentSelectedTeamIndex >= 0)
        {
            Debug.Log($"Assigned {tileID} to {currentLayout.teams[currentSelectedTeamIndex].DisplayName}");
        }
        else
        {
            Debug.Log($"Neutralized {tileID}");
        }
    }
    
    /// <summary>
    /// Get tile positions (same as GameManager)
    /// </summary>
    private Dictionary<string, Vector2> GetTilePositions()
    {
        return new Dictionary<string, Vector2>
        {
            {"NS", new Vector2(0.55f, 0.82f)},
            {"NE", new Vector2(0.35f, 0.68f)},
            {"DK", new Vector2(0.65f, 0.68f)},
            {"MT", new Vector2(0.5f, 0.55f)},
            {"CH", new Vector2(0.15f, 0.42f)},
            {"DW", new Vector2(0.45f, 0.42f)},
            {"BW", new Vector2(0.72f, 0.42f)},
            {"SS", new Vector2(0.42f, 0.28f)},
            {"DE", new Vector2(0.75f, 0.28f)},
            {"AR", new Vector2(0.55f, 0.15f)},
            {"BS", new Vector2(0.88f, 0.15f)},
            {"HR", new Vector2(0.62f, 0.05f)}
        };
    }
    
    /// <summary>
    /// Submit layout and create lobby
    /// </summary>
    private async void OnSubmitLayout()
    {
        // Validate
        if (string.IsNullOrEmpty(lobbyNameInput.text))
        {
            Debug.LogError("Please enter a lobby name");
            return;
        }
        
        if (currentLayout.teams.Count < 2)
        {
            Debug.LogError("Need at least 2 teams to start a game");
            return;
        }
        
        // Create session via NetworkManager
        bool success = await NetworkManager.Instance.CreateSession(
            lobbyNameInput.text,
            passwordInput.text,
            currentLayout
        );
        
        if (success)
        {
            Debug.Log("Lobby created! Transitioning to Team Selection...");
            // TODO: Load Team Selection scene
        }
    }
    
    private void OnBack()
    {
        // Return to main menu
        // TODO: Load main menu scene
        Debug.Log("Returning to main menu");
    }
}
