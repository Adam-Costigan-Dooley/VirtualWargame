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

    [Header("Unit Position Visualization")]
    [SerializeField] private GameObject unitPositionMarkerPrefab;
    [SerializeField] private Transform unitMarkerContainer;

    private Dictionary<string, GameObject> currentPositionMarkers = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> assignedPositionMarkers = new Dictionary<string, GameObject>();

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
        UpdateUnitPositionMarkers(); 
        
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
        Faction heroes = new Faction("Heroes", new Color(0.2f, 0.4f, 0.8f)); // Blue
        Faction villains = new Faction("Villains", new Color(0.8f, 0.2f, 0.2f)); // Red
        
        factions.Add(heroes);
        factions.Add(villains);
    }

    // Initialize all tiles based on the map
    private void InitializeTiles()
    {
        // Row 1 (Top)
        AddTile("NS", "Northern Suburbs", new Vector2(0.55f, 0.82f), 3);
        
        // Row 2  
        AddTile("NE", "North End", new Vector2(0.35f, 0.68f), 2);
        AddTile("DK", "The Docks", new Vector2(0.65f, 0.68f), 4);
        
        // Row 3
        AddTile("MT", "Midtown", new Vector2(0.5f, 0.55f), 5);
        
        // Row 4
        AddTile("CH", "Captain's Hill", new Vector2(0.15f, 0.42f), 2);
        AddTile("DW", "Downtown West", new Vector2(0.45f, 0.42f), 6);
        AddTile("BW", "Boardwalk", new Vector2(0.72f, 0.42f), 3);
        
        // Row 5
        AddTile("SS", "Southern Suburbs", new Vector2(0.42f, 0.28f), 2);
        AddTile("DE", "Downtown East", new Vector2(0.75f, 0.28f), 6);
        
        // Row 6
        AddTile("AR", "Airport & Rye", new Vector2(0.55f, 0.15f), 4);
        AddTile("BS", "Beach Suburbs", new Vector2(0.88f, 0.15f), 3);
        
        // Row 7 (Bottom)
        AddTile("HR", "Hampton Resort", new Vector2(0.62f, 0.05f), 4);

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
        Faction heroFaction = factions[0]; // Assuming Heroes = index 0
        Faction villainFaction = factions[1]; // Assuming Villains = index 1

        // Create all hero units
        List<Unit> heroUnits = UnitFactory.CreateHeroUnits();
        foreach (Unit unit in heroUnits)
        {
            heroFaction.AddUnit(unit);
            
            // Give starting tiles to factions based on unit positions
            if (!heroFaction.controlledTiles.Contains(unit.currentTile))
            {
                Tile tile = tiles[unit.currentTile];
                tile.SetController("Heroes", heroFaction.factionColor);
                heroFaction.ClaimTile(unit.currentTile);
            }
        }

        // Create all villain units
        List<Unit> villainUnits = UnitFactory.CreateVillainUnits();
        foreach (Unit unit in villainUnits)
        {
            villainFaction.AddUnit(unit);
            
            // Give starting tiles to factions
            if (!villainFaction.controlledTiles.Contains(unit.currentTile))
            {
                Tile tile = tiles[unit.currentTile];
                tile.SetController("Villains", villainFaction.factionColor);
                villainFaction.ClaimTile(unit.currentTile);
            }
        }
        
        Debug.Log($"Initialized {heroUnits.Count} hero units and {villainUnits.Count} villain units");
    }

    // Create clickable markers for each tile on the map
    private void CreateTileMarkers()
    {

        Dictionary<string, Vector2> tileOffsets = new Dictionary<string, Vector2>()
        {
            {"NS", new Vector2(23, 59)},
            {"NE", new Vector2(34, 68)},     // You didn't provide NE, so keeping at 0
            {"DK", new Vector2(-55, 36)},
            {"MT", new Vector2(-249, 80)},
            {"CH", new Vector2(69, 134)},
            {"DW", new Vector2(-161, 74)},
            {"BW", new Vector2(-309, 138)},
            {"SS", new Vector2(-89, 62)},
            {"DE", new Vector2(-240, 157)},
            {"AR", new Vector2(21, 117)},
            {"BS", new Vector2(-246, 155)},
            {"HR", new Vector2(-110, 80)}
        };

        foreach (var kvp in tiles)
        {
            Tile tile = kvp.Value;
            
            GameObject marker = Instantiate(tileMarkerPrefab, mapImage.transform);
            
            RectTransform rt = marker.GetComponent<RectTransform>();
            rt.anchorMin = tile.position;
            rt.anchorMax = tile.position;
            
            // Apply offset to center on actual map tile
            if (tileOffsets.ContainsKey(tile.tileID))
            {
                rt.anchoredPosition = tileOffsets[tile.tileID];
            }
            else
            {
                rt.anchoredPosition = Vector2.zero;
            }
            
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

        UpdateUnitPositionMarkers(); 
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
        UpdateUnitPositionMarkers(); 
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


    /// <summary>
    /// Resolve conflicts where multiple factions assigned units to same tile
    /// UPDATED: Includes casualty rolls (10% destroy, 33% injure losers; 25% injure winners)
    /// UPDATED: Includes special abilities for elite units
    /// </summary>
    private void ResolveTileConflicts()
    {
        foreach (var kvp in tiles)
        {
            Tile tile = kvp.Value;
            
            // Find which factions have units assigned here OR already present
            Dictionary<Faction, int> factionStrengths = new Dictionary<Faction, int>();
            Dictionary<Faction, List<Unit>> factionUnits = new Dictionary<Faction, List<Unit>>();
            
            foreach (Faction faction in factions)
            {
                int totalStrength = 0;
                List<Unit> unitsInvolved = new List<Unit>();
                bool isAttacking = false;
                
                // Check units ASSIGNED to move here (attackers)
                foreach (Unit unit in faction.units)
                {
                    if (unit.IsAssignedTo(tile.tileID) && unit.CanDeploy())
                    {
                        int strength = unit.combatStrength;
                        
                        // Apply special abilities
                        if (unit.unitID == "V_IRONCLAD" && unit.hasSpecialAbility)
                        {
                            // Ironclad: Attackers gain +3 strength
                            strength += 3;
                            Debug.Log($"Ironclad's Overwhelming Force: +3 strength");
                        }
                        
                        totalStrength += strength;
                        unitsInvolved.Add(unit);
                        isAttacking = true;
                    }
                }
                
                // Check units ALREADY at this tile (defenders)
                foreach (Unit unit in faction.units)
                {
                    if (unit.currentTile == tile.tileID && !unit.IsAssigned() && unit.status == UnitStatus.Ready)
                    {
                        int strength = unit.combatStrength;
                        
                        // Apply special abilities
                        if (unit.unitID == "H_SENTINEL" && unit.hasSpecialAbility && !isAttacking)
                        {
                            // Sentinel: Defenders gain +5 strength
                            strength += 5;
                            Debug.Log($"Sentinel's Defender ability: +5 strength");
                        }
                        
                        totalStrength += strength;
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
            
            // If multiple factions are present, resolve combat
            if (factionStrengths.Count > 1)
            {
                Debug.Log($"⚔️ CONFLICT at {tile.tileName}!");
                
                // Show all faction strengths
                foreach (var kvp2 in factionStrengths)
                {
                    Debug.Log($"  {kvp2.Key.factionName}: {kvp2.Value} strength ({kvp2.Value / factionUnits[kvp2.Key].Count} avg)");
                }
                
                // Find winner (highest strength)
                var winnerEntry = factionStrengths.OrderByDescending(x => x.Value).First();
                Faction winningFaction = winnerEntry.Key;
                Debug.Log($"🏆 {winningFaction.factionName} wins with {winnerEntry.Value} strength!");
                
                // Process casualties for each faction
                foreach (var factionKvp in factionUnits)
                {
                    Faction faction = factionKvp.Key;
                    List<Unit> units = factionKvp.Value;
                    bool isWinner = (faction == winningFaction);
                    
                    foreach (Unit unit in units)
                    {
                        if (isWinner)
                        {
                            // WINNER'S UNITS
                            
                            // Move to tile if attacking
                            if (unit.IsAssigned())
                            {
                                unit.ExecuteMove();
                                Debug.Log($"✓ {unit.unitName} successfully captured {tile.tileName}");
                            }
                            else
                            {
                                Debug.Log($"✓ {unit.unitName} successfully defended {tile.tileName}");
                            }
                            
                            // 25% chance of injury for winners
                            int roll = Random.Range(1, 101); // 1-100
                            
                            if (roll <= 25)
                            {
                                unit.SetInjured();
                                Debug.Log($"  🤕 {unit.unitName} was injured in the battle");
                            }
                        }
                        else
                        {
                            // LOSER'S UNITS
                            
                            // Check for Velocity's special ability (never injured when retreating)
                            bool canAvoidInjury = (unit.unitID == "H_VELOCITY" && unit.hasSpecialAbility);
                            
                            // Check for Phantom's special ability (50% avoid injury)
                            bool hasPhantomAbility = (unit.unitID == "V_PHANTOM" && unit.hasSpecialAbility);
                            
                            int roll = Random.Range(1, 101); // 1-100
                            
                            // 10% chance of destruction
                            if (roll <= 10)
                            {
                                if (canAvoidInjury)
                                {
                                    Debug.Log($"  ⚡ {unit.unitName} uses Evasion - avoided destruction!");
                                    unit.Unassign();
                                }
                                else if (hasPhantomAbility && Random.Range(0, 2) == 0) // 50% chance
                                {
                                    Debug.Log($"  👻 {unit.unitName} uses Shadow Step - avoided destruction!");
                                    unit.Unassign();
                                }
                                else
                                {
                                    unit.SetDestroyed();
                                    Debug.Log($"  💀 {unit.unitName} was destroyed!");
                                }
                            }
                            // 33% chance of injury (cumulative: 10 + 33 = 43)
                            else if (roll <= 43)
                            {
                                if (canAvoidInjury)
                                {
                                    Debug.Log($"  ⚡ {unit.unitName} uses Evasion - avoided injury!");
                                    unit.Unassign();
                                }
                                else if (hasPhantomAbility && Random.Range(0, 2) == 0) // 50% chance
                                {
                                    Debug.Log($"  👻 {unit.unitName} uses Shadow Step - avoided injury!");
                                    unit.Unassign();
                                }
                                else
                                {
                                    unit.SetInjured();
                                    Debug.Log($"  🤕 {unit.unitName} was injured!");
                                }
                                
                                // Cancel movement if assigned
                                if (unit.IsAssigned())
                                {
                                    unit.Unassign();
                                }
                            }
                            // 57% chance - retreat unharmed
                            else
                            {
                                Debug.Log($"  ↩️ {unit.unitName} retreated safely");
                                
                                // Cancel movement if assigned
                                if (unit.IsAssigned())
                                {
                                    unit.Unassign();
                                }
                            }
                        }
                    }
                }
            }
            else if (factionStrengths.Count == 1)
            {
                // No conflict - single faction controls
                var faction = factionStrengths.First().Key;
                List<Unit> units = factionUnits[faction];
                
                foreach (Unit unit in units)
                {
                    if (unit.IsAssigned())
                    {
                        unit.ExecuteMove();
                        Debug.Log($"➡️ {unit.unitName} moved to {tile.tileName} (no opposition)");
                    }
                    // Units already here just stay (no message needed)
                }
            }
        }
    }

    /// <summary>
    /// Update tile control based on which units are present
    /// UPDATED: Scouts cannot capture tiles alone
    /// </summary>
    private void UpdateTileControl()
    {
        foreach (var kvp in tiles)
        {
            Tile tile = kvp.Value;
            
            // Find which faction has non-scout units at this tile
            Faction newController = null;
            
            foreach (Faction faction in factions)
            {
                bool hasNonScoutUnit = false;
                
                foreach (Unit unit in faction.units)
                {
                    // Check if unit is at this tile, alive, and not a scout
                    if (unit.currentTile == tile.tileID && 
                        unit.status != UnitStatus.Destroyed && 
                        unit.unitType != UnitType.Scout)
                    {
                        hasNonScoutUnit = true;
                        break;
                    }
                }
                
                if (hasNonScoutUnit)
                {
                    newController = faction;
                    break; // Found a controlling faction
                }
            }
            
            // Only update if there's a NEW controller (someone captured it)
            if (newController != null && tile.controllingFaction != newController.factionName)
            {
                Debug.Log($"Territory Change: {newController.factionName} captured {tile.tileName}!");
                
                // Remove from old faction if it was controlled
                if (tile.controllingFaction != null)
                {
                    Faction oldFaction = factions.Find(f => f.factionName == tile.controllingFaction);
                    oldFaction?.LoseTile(tile.tileID);
                }
                
                // Add to new faction
                tile.SetController(newController.factionName, newController.factionColor);
                newController.ClaimTile(tile.tileID);
            }
            // If no one is there, tile keeps its current control
            // No change needed - it stays as it was
        }
        
        UpdateTileVisuals();
    }
    /// <summary>
    /// Update visual appearance of tile markers based on faction control
    /// </summary>
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

    /// <summary>
    /// Process turn for all units (recover from injuries, etc.)
    /// UPDATED: Call ProcessTurn on each unit
    /// </summary>
    private void ProcessNextTurn()
    {
        Debug.Log($"⏰ Processing Turn {currentTurn}...");
        
        // Process unit status (injuries, etc.)
        foreach (Faction faction in factions)
        {
            foreach (Unit unit in faction.units)
            {
                unit.ProcessTurn();
            }
            
            // Remove destroyed units from faction
            faction.units.RemoveAll(u => u.status == UnitStatus.Destroyed);
        }
        
        ResolveTileConflicts();
        UpdateTileControl();
        GenerateResources();
        
        currentTurn++;
        UpdateUI();
        UpdateUnitPositionMarkers(); 
        
        Debug.Log($"✅ Turn {currentTurn - 1} complete!");
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

    /// <summary>
    /// Update all unit position markers on the map
    /// Called after any unit assignment change or turn resolution
    /// </summary>
    private void UpdateUnitPositionMarkers()
    {
        // Clear existing markers
        ClearAllUnitMarkers();
        
        // Create markers for all units in both factions
        foreach (Faction faction in factions)
        {
            foreach (Unit unit in faction.units)
            {
                // Skip destroyed units
                if (unit.status == UnitStatus.Destroyed)
                    continue;
                
                // Create marker at current position
                if (!string.IsNullOrEmpty(unit.currentTile) && tiles.ContainsKey(unit.currentTile))
                {
                    CreateCurrentPositionMarker(unit, faction);
                }
                
                // Create marker at assigned position (if different from current)
                if (unit.IsAssigned() && tiles.ContainsKey(unit.assignedTile) && unit.assignedTile != unit.currentTile)
                {
                    CreateAssignedPositionMarker(unit, faction);
                }
            }
        }
    }

    /// <summary>
    /// Create a marker showing unit's current position
    /// </summary>
    private void CreateCurrentPositionMarker(Unit unit, Faction faction)
    {
        // Find the tile marker to copy its exact positioning setup
        GameObject tileMarker = FindTileMarkerByID(unit.currentTile);
        if (tileMarker == null) return;
        
        RectTransform tileRT = tileMarker.GetComponent<RectTransform>();
        
        GameObject marker = Instantiate(unitPositionMarkerPrefab, tileMarker.transform);
        RectTransform rt = marker.GetComponent<RectTransform>();
        
        // Set to center of parent (tile marker) with no stretching
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        
        // Now offset is purely relative to tile marker center
        Vector2 unitOffset = CalculateUnitOffset(unit.currentTile, unit.unitID);
        rt.anchoredPosition = unitOffset;
        
        rt.sizeDelta = new Vector2(15, 15);
        
        Image img = marker.GetComponent<Image>();
        if (img != null)
        {
            img.color = faction.factionColor;
        }
        
        currentPositionMarkers[unit.unitID] = marker;
    }


    /// <summary>
    /// Create a marker showing where unit is assigned to move
    /// </summary>
    private void CreateAssignedPositionMarker(Unit unit, Faction faction)
    {
        // Find the tile marker to copy its exact positioning setup
        GameObject tileMarker = FindTileMarkerByID(unit.assignedTile);
        if (tileMarker == null) return;
        
        RectTransform tileRT = tileMarker.GetComponent<RectTransform>();
        
        GameObject marker = Instantiate(unitPositionMarkerPrefab, tileMarker.transform);
        RectTransform rt = marker.GetComponent<RectTransform>();
        
        // Set to center of parent (tile marker) with no stretching
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        
        // Now offset is purely relative to tile marker center
        Vector2 unitOffset = CalculateUnitOffset(unit.assignedTile, unit.unitID);
        rt.anchoredPosition = unitOffset;
        
        rt.sizeDelta = new Vector2(15, 15);
        
        Image img = marker.GetComponent<Image>();
        if (img != null)
        {
            Color transparentColor = faction.factionColor;
            transparentColor.a = 0.4f;
            img.color = transparentColor;
        }
        
        assignedPositionMarkers[unit.unitID] = marker;
    }
    
    /// <summary>
    /// Find the tile marker GameObject by tile ID
    /// </summary>
    private GameObject FindTileMarkerByID(string tileID)
    {
        foreach (Transform child in mapImage.transform)
        {
            TextMeshProUGUI nameText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null && nameText.text == tileID)
            {
                return child.gameObject;
            }
        }
        return null;
    }
    /// <summary>
    /// Calculate offset for unit marker so multiple units at same tile don't overlap
    /// </summary>
    private Vector2 CalculateUnitOffset(string tileID, string unitID)
    {
        // Count how many units we've ALREADY POSITIONED at this tile
        // (not checking the dictionaries since they're empty during initial creation)
        int unitCount = 0;
        
        foreach (Faction faction in factions)
        {
            foreach (Unit unit in faction.units)
            {
                if (unit.status == UnitStatus.Destroyed)
                    continue;
                
                // Don't count the current unit we're positioning
                if (unit.unitID == unitID)
                    continue;
                
                // Count units at current position
                if (unit.currentTile == tileID)
                {
                    unitCount++;
                }
                
                // Count units assigned to this position
                if (unit.assignedTile == tileID)
                {
                    unitCount++;
                }
            }
        }
        
        // Arrange in a circle around the tile marker
        float radius = 30f;
        float angle = (unitCount * 45f) * Mathf.Deg2Rad; // 45 degrees apart
        
        Vector2 offset = new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius
        );
        
        return offset;
    }
    /// <summary>
    /// Clear all unit position markers from the map
    /// </summary>
    private void ClearAllUnitMarkers()
    {
        // Clear current position markers
        foreach (var marker in currentPositionMarkers.Values)
        {
            if (marker != null)
                Destroy(marker);
        }
        currentPositionMarkers.Clear();
        
        // Clear assigned position markers
        foreach (var marker in assignedPositionMarkers.Values)
        {
            if (marker != null)
                Destroy(marker);
        }
        assignedPositionMarkers.Clear();
    }

}

