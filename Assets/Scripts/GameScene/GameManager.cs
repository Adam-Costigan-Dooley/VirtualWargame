using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using System.Linq;

// Main game manager - handles map, tiles, units, and turn resolution
public class GameManager : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private Image mapImage;
    [SerializeField] private GameObject tileMarkerPrefab;
    [SerializeField] private float mapPanSpeed = 1f;
    
    [Header("UI References")]
    [SerializeField] private GameObject unitSelectionPanel;
    [SerializeField] private Transform unitListContainer;
    [SerializeField] private GameObject unitButtonPrefab;
    [SerializeField] private TextMeshProUGUI selectedTileText;
    [SerializeField] private Button nextTurnButton;
    [SerializeField] private Button closeUnitPanelButton;
    [SerializeField] private TextMeshProUGUI turnNumberText;
    [SerializeField] private TextMeshProUGUI redResourcesText;
    [SerializeField] private TextMeshProUGUI blueResourcesText;

    // Game state
    private Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();
    private List<Faction> factions = new List<Faction>();
    private Tile selectedTile;
    private int currentTurn = 1;
    
    // Camera panning
    private Vector2 dragStartPosition;
    private Vector2 mapStartPosition;
    private bool isDragging = false;
    private RectTransform mapRectTransform;

    private void Start()
    {
        // Initialize game
        InitializeFactions();
        InitializeTiles();
        InitializeUnits();
        CreateTileMarkers();
        
        // UI
        nextTurnButton.onClick.AddListener(ProcessNextTurn);
        if (closeUnitPanelButton != null)
        {
            closeUnitPanelButton.onClick.AddListener(CloseUnitPanel);
        }
        unitSelectionPanel.SetActive(false);
        
        // Camera panning
        mapRectTransform = mapImage.GetComponent<RectTransform>();
        
        UpdateUI();
    }

    private void Update()
    {
        HandleMapPanning();
    }

    // Handle click and drag to pan the map
    private void HandleMapPanning()
    {
        // Get current mouse state
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Check for mouse button press (left click)
        if (mouse.leftButton.wasPressedThisFrame)
        {
            // Check if clicking over a UI element
            if (!IsPointerOverUIElement())
            {
                // Clicking on map background - close unit panel
                CloseUnitPanel();
            }
            else if (IsPointerOverMap() && !IsPointerOverTile())
            {
                // Start dragging the map
                isDragging = true;
                dragStartPosition = mouse.position.ReadValue();
                mapStartPosition = mapRectTransform.anchoredPosition;
            }
        }

        // While dragging
        if (isDragging && mouse.leftButton.isPressed)
        {
            Vector2 currentPosition = mouse.position.ReadValue();
            Vector2 difference = currentPosition - dragStartPosition;
            
            // Apply the drag
            mapRectTransform.anchoredPosition = mapStartPosition + difference * mapPanSpeed;
        }

        // Stop dragging
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    // Check if mouse is over any UI element
    private bool IsPointerOverUIElement()
    {
        var mouse = Mouse.current;
        if (mouse == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = mouse.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    // Check if mouse is specifically over the map image
    private bool IsPointerOverMap()
    {
        var mouse = Mouse.current;
        if (mouse == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = mouse.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == mapImage.gameObject)
            {
                return true;
            }
        }
        return false;
    }

    // Check if mouse is over a tile marker
    private bool IsPointerOverTile()
    {
        var mouse = Mouse.current;
        if (mouse == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = mouse.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.name.Contains("TileMarker") || 
                result.gameObject.GetComponentInParent<Button>() != null)
            {
                return true;
            }
        }
        return false;
    }

    // Close the unit selection panel
    private void CloseUnitPanel()
    {
        unitSelectionPanel.SetActive(false);
        selectedTile = null;
        selectedTileText.text = "Select a tile...";
    }

    // Create the two factions (Red and Blue teams)
    private void InitializeFactions()
    {
        Faction redTeam = new Faction("Red Team", new Color(0.8f, 0.2f, 0.2f));
        Faction blueTeam = new Faction("Blue Team", new Color(0.2f, 0.4f, 0.8f));
        
        factions.Add(redTeam);
        factions.Add(blueTeam);
    }

    // Initialize all tiles based on the map
    private void InitializeTiles()
    {
        // Create tiles with positions
        //To-Do replace this entire thing with a more modular system for future scenario system. Placeholder for unit testing.
        
        // Row 1 (Top)
        AddTile("NS", "Northern Suburbs", new Vector2(0.7f, 0.9f), 3);
        
        // Row 2
        AddTile("NE", "North End", new Vector2(0.3f, 0.75f), 2);
        AddTile("DK", "The Docks", new Vector2(0.7f, 0.75f), 4);
        
        // Row 3
        AddTile("MT", "Midtown", new Vector2(0.5f, 0.6f), 5);
        
        // Row 4
        AddTile("CH", "Captain's Hill", new Vector2(0.1f, 0.45f), 2);
        AddTile("DW", "Downtown West", new Vector2(0.5f, 0.45f), 6);
        AddTile("BW", "Boardwalk", new Vector2(0.8f, 0.45f), 3);
        
        // Row 5
        AddTile("SS", "Southern Suburbs", new Vector2(0.4f, 0.3f), 2);
        AddTile("DE", "Downtown East", new Vector2(0.85f, 0.3f), 6);
        
        // Row 6
        AddTile("AR", "Airport & Rye", new Vector2(0.6f, 0.15f), 4);
        AddTile("BS", "Beach Suburbs", new Vector2(0.95f, 0.15f), 3);
        
        // Row 7 (Bottom)
        AddTile("HR", "Hampton Resort", new Vector2(0.7f, 0.05f), 4);

        // Set up adjacencies
        SetupAdjacencies();
    }

    private void AddTile(string id, string name, Vector2 normalizedPos, int resources)
    {
        Tile tile = new Tile(id, name, normalizedPos, resources);
        tiles[id] = tile;
    }

    private void SetupAdjacencies()
    {
        tiles["NS"].adjacentTiles.AddRange(new[] { "NE", "DK" });
        tiles["NE"].adjacentTiles.AddRange(new[] { "NS", "MT", "CH" });
        tiles["DK"].adjacentTiles.AddRange(new[] { "NS", "MT", "BW" });
        tiles["MT"].adjacentTiles.AddRange(new[] { "NE", "DK", "DW" });
        tiles["CH"].adjacentTiles.AddRange(new[] { "NE", "DW" });
        tiles["DW"].adjacentTiles.AddRange(new[] { "MT", "CH", "SS", "BW", "DE" });
        tiles["BW"].adjacentTiles.AddRange(new[] { "DK", "DW", "DE" });
        tiles["SS"].adjacentTiles.AddRange(new[] { "DW", "AR" });
        tiles["DE"].adjacentTiles.AddRange(new[] { "DW", "BW", "BS", "AR" });
        tiles["AR"].adjacentTiles.AddRange(new[] { "SS", "DE", "HR" });
        tiles["BS"].adjacentTiles.AddRange(new[] { "DE" });
        tiles["HR"].adjacentTiles.AddRange(new[] { "AR" });
    }

    // Create two template units for each faction
    private void InitializeUnits()
    {
        Faction redTeam = factions[0];
        Faction blueTeam = factions[1];

        // Red Team units
        Unit redUnit1 = new Unit("R1", "Red Infantry A", "Red Team", 5, "DW");
        Unit redUnit2 = new Unit("R2", "Red Tank", "Red Team", 8, "DW");
        redTeam.AddUnit(redUnit1);
        redTeam.AddUnit(redUnit2);

        // Blue Team units
        Unit blueUnit1 = new Unit("B1", "Blue Infantry A", "Blue Team", 5, "MT");
        Unit blueUnit2 = new Unit("B2", "Blue Tank", "Blue Team", 8, "MT");
        blueTeam.AddUnit(blueUnit1);
        blueTeam.AddUnit(blueUnit2);

        // Give starting tiles to factions
        tiles["DW"].SetController("Red Team", redTeam.factionColor);
        redTeam.ClaimTile("DW");
        
        tiles["MT"].SetController("Blue Team", blueTeam.factionColor);
        blueTeam.ClaimTile("MT");
    }

    // Create clickable markers for each tile on the map
    private void CreateTileMarkers()
    {
        foreach (var kvp in tiles)
        {
            Tile tile = kvp.Value;
            
            GameObject marker = Instantiate(tileMarkerPrefab, mapImage.transform);
            
            RectTransform rt = marker.GetComponent<RectTransform>();
            rt.anchorMin = tile.position;
            rt.anchorMax = tile.position;
            rt.anchoredPosition = Vector2.zero;
            
            Button markerButton = marker.GetComponent<Button>();
            markerButton.onClick.AddListener(() => OnTileClicked(tile));
            
            TextMeshProUGUI nameText = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = tile.tileID;
            }
            
            Image colorIndicator = marker.transform.Find("ColorIndicator")?.GetComponent<Image>();
            if (colorIndicator != null)
            {
                colorIndicator.color = tile.factionColor;
            }
        }
    }

    // Handle when a tile is clicked
    private void OnTileClicked(Tile tile)
    {
        selectedTile = tile;
        selectedTileText.text = $"Selected: {tile.tileName} ({tile.tileID})";
        
        ShowUnitSelectionPanel();
    }

    // Show the unit selection panel with current player's units
    private void ShowUnitSelectionPanel()
    {
        unitSelectionPanel.SetActive(true);
        
        // Clear existing unit buttons
        foreach (Transform child in unitListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get current player's faction (for now, always Red Team)
        Faction playerFaction = factions[0]; // Red Team
        
        // ONLY show units if this faction HAS units
        if (playerFaction.units.Count == 0)
        {
            Debug.Log("No units available for this faction");
            return;
        }
        
        // Create button for each unit
        foreach (Unit unit in playerFaction.units)
        {
            GameObject unitButton = Instantiate(unitButtonPrefab, unitListContainer);
            
            // Set unit name
            TextMeshProUGUI unitNameText = unitButton.transform.Find("UnitName").GetComponent<TextMeshProUGUI>();
            if (unitNameText != null)
            {
                unitNameText.text = $"{unit.unitName} (Str: {unit.combatStrength})";
            }
            
            // Set button text and functionality
            Button assignButton = unitButton.transform.Find("AssignButton").GetComponent<Button>();
            if (assignButton != null)
            {
                TextMeshProUGUI buttonText = assignButton.GetComponentInChildren<TextMeshProUGUI>();
                
                // Determine button state
                if (unit.IsAssignedTo(selectedTile.tileID))
                {
                    if (buttonText != null) buttonText.text = "UNASSIGN";
                    assignButton.onClick.RemoveAllListeners();
                    assignButton.onClick.AddListener(() => UnassignUnit(unit));
                }
                else if (unit.IsAssigned())
                {
                    if (buttonText != null) buttonText.text = "REASSIGN";
                    assignButton.onClick.RemoveAllListeners();
                    assignButton.onClick.AddListener(() => ReassignUnit(unit, selectedTile.tileID));
                }
                else
                {
                    if (buttonText != null) buttonText.text = "ASSIGN";
                    assignButton.onClick.RemoveAllListeners();
                    assignButton.onClick.AddListener(() => AssignUnit(unit, selectedTile.tileID));
                }
            }
        }
    }

    private void AssignUnit(Unit unit, string tileID)
    {
        unit.AssignToTile(tileID);
        Debug.Log($"{unit.unitName} assigned to {tileID}");
        ShowUnitSelectionPanel(); // Refresh UI
    }

    private void UnassignUnit(Unit unit)
    {
        unit.Unassign();
        Debug.Log($"{unit.unitName} unassigned");
        ShowUnitSelectionPanel(); // Refresh UI
    }

    private void ReassignUnit(Unit unit, string newTileID)
    {
        unit.AssignToTile(newTileID);
        Debug.Log($"{unit.unitName} reassigned to {newTileID}");
        ShowUnitSelectionPanel(); // Refresh UI
    }

    private void ProcessNextTurn()
    {
        Debug.Log($"Processing Turn {currentTurn}...");
        
        ResolveTileConflicts();
        UpdateTileControl();
        GenerateResources();
        
        currentTurn++;
        UpdateUI();
        
        Debug.Log($"Turn {currentTurn - 1} complete!");
    }

    private void ResolveTileConflicts()
    {
        foreach (var kvp in tiles)
        {
            Tile tile = kvp.Value;
            
            // Find which factions have units assigned here or already present
            // ToDo- Update assignment system to showcase where units are defending
            Dictionary<Faction, int> factionStrengths = new Dictionary<Faction, int>();
            Dictionary<Faction, List<Unit>> factionUnits = new Dictionary<Faction, List<Unit>>();
            
            foreach (Faction faction in factions)
            {
                int totalStrength = 0;
                List<Unit> unitsInvolved = new List<Unit>();
                
                foreach (Unit unit in faction.units)
                {
                    if (unit.IsAssignedTo(tile.tileID))
                    {
                        totalStrength += unit.combatStrength;
                        unitsInvolved.Add(unit);
                    }
                }
                
                foreach (Unit unit in faction.units)
                {
                    if (unit.currentTile == tile.tileID && !unit.IsAssigned())
                    {
                        totalStrength += unit.combatStrength;
                        unitsInvolved.Add(unit);
                        Debug.Log($"{unit.unitName} defending {tile.tileName}");
                    }
                }
                
                if (totalStrength > 0)
                {
                    factionStrengths[faction] = totalStrength;
                    factionUnits[faction] = unitsInvolved;
                }
            }
            
            if (factionStrengths.Count > 1)
            {
                Debug.Log($"CONFLICT at {tile.tileName}!");
                
                foreach (var kvp2 in factionStrengths)
                {
                    Debug.Log($"  {kvp2.Key.factionName}: {kvp2.Value} strength");
                }
                
                // Find winner (highest strength for now, tiered combat outcomes todo)
                var winner = factionStrengths.OrderByDescending(x => x.Value).First();
                Debug.Log($"{winner.Key.factionName} wins with strength {winner.Value}!");
                
                // Process results for each faction
                foreach (var factionKvp in factionUnits)
                {
                    Faction faction = factionKvp.Key;
                    List<Unit> units = factionKvp.Value;
                    
                    foreach (Unit unit in units)
                    {
                        if (faction == winner.Key)
                        {
                            // Winner's units stay/move to tile
                            if (unit.IsAssigned())
                            {
                                unit.ExecuteMove(); // Move to tile
                                Debug.Log($"{unit.unitName} successfully captured {tile.tileName}");
                            }
                            else
                            {
                                // Already here, just stay
                                Debug.Log($"{unit.unitName} successfully defended {tile.tileName}");
                            }
                        }
                        else
                        {
                            // Loser's units are defeated
                            if (unit.IsAssigned())
                            {
                                unit.Unassign(); // Cancel movement
                                Debug.Log($"{unit.unitName} defeated at {tile.tileName} (attack failed)");
                            }
                            else
                            {
                                Debug.Log($"{unit.unitName} defeated at {tile.tileName} (defense failed)");
                                // TODO: Implement unit destruction/removal/Injuries
                            }
                        }
                    }
                }
            }
            else if (factionStrengths.Count == 1)
            {
                // No conflict
                var faction = factionStrengths.First().Key;
                List<Unit> units = factionUnits[faction];
                
                foreach (Unit unit in units)
                {
                    if (unit.IsAssigned())
                    {
                        unit.ExecuteMove();
                        Debug.Log($"{unit.unitName} moved to {tile.tileName} (no opposition)");
                    }
                }
            }
        }
    }


    private void UpdateTileControl()
    {
        foreach (var kvp in tiles)
        {
            Tile tile = kvp.Value;
            
            Faction controllingFaction = null;
            
            foreach (Faction faction in factions)
            {
                foreach (Unit unit in faction.units)
                {
                    if (unit.currentTile == tile.tileID)
                    {
                        controllingFaction = faction;
                        break;
                    }
                }
                if (controllingFaction != null) break;
            }
            
            if (controllingFaction != null)
            {
                if (tile.controllingFaction != controllingFaction.factionName)
                {
                    Debug.Log($"{controllingFaction.factionName} captured {tile.tileName}!");
                    
                    if (tile.controllingFaction != null)
                    {
                        Faction oldFaction = factions.Find(f => f.factionName == tile.controllingFaction);
                        oldFaction?.LoseTile(tile.tileID);
                    }
                    
                    tile.SetController(controllingFaction.factionName, controllingFaction.factionColor);
                    controllingFaction.ClaimTile(tile.tileID);
                }
            }
        }
        
        UpdateTileVisuals();
    }

    private void UpdateTileVisuals()
    {
        foreach (Transform child in mapImage.transform)
        {
            Button markerButton = child.GetComponent<Button>();
            if (markerButton != null)
            {
                TextMeshProUGUI nameText = child.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    string tileID = nameText.text;
                    if (tiles.ContainsKey(tileID))
                    {
                        Tile tile = tiles[tileID];
                        Image colorIndicator = child.Find("ColorIndicator")?.GetComponent<Image>();
                        if (colorIndicator != null)
                        {
                            colorIndicator.color = tile.factionColor;
                        }
                    }
                }
            }
        }
    }

    private void GenerateResources()
    {
        foreach (Faction faction in factions)
        {
            int resourcesGenerated = 0;
            
            foreach (string tileID in faction.controlledTiles)
            {
                if (tiles.ContainsKey(tileID))
                {
                    resourcesGenerated += tiles[tileID].resourceValue;
                }
            }
            
            faction.AddResources(resourcesGenerated);
            Debug.Log($"{faction.factionName} generated {resourcesGenerated} resources (Total: {faction.totalResources})");
        }
    }

    private void UpdateUI()
    {
        turnNumberText.text = $"Turn: {currentTurn}";
        redResourcesText.text = $"Red Team: {factions[0].totalResources} Resources";
        blueResourcesText.text = $"Blue Team: {factions[1].totalResources} Resources";
    }

    private void OnDestroy()
    {
        nextTurnButton.onClick.RemoveListener(ProcessNextTurn);
        if (closeUnitPanelButton != null)
        {
            closeUnitPanelButton.onClick.RemoveListener(CloseUnitPanel);
        }
    }
}