using System;
using UnityEngine;

/// <summary>
/// Represents different types of units with unique roles
/// </summary>
public enum UnitType
{
    Grunt,          // Basic infantry (Cops/Thugs)
    Elite,          // Advanced soldiers (Special Forces/Mercs)
    Scout,          // Intelligence gatherers (Spies)
    SuperHero,      // Named elite heroes
    SuperVillain    // Named elite villains
}

/// <summary>
/// Unit status affecting deployment availability
/// </summary>
public enum UnitStatus
{
    Ready,          // Can be deployed normally
    Injured,        // Cannot deploy this turn
    Destroyed       // Removed from game
}

/// <summary>
/// Represents a single military unit with combat stats and special abilities
/// </summary>
[System.Serializable]
public class Unit
{
    // Identity
    public string unitID;
    public string unitName;
    public string factionName;
    public UnitType unitType;
    
    // Combat stats
    public int combatStrength;
    public int movementRange; // How many tiles can move per turn
    
    // Status
    public UnitStatus status;
    public int turnsInjured; // Countdown timer for injury
    
    // Location
    public string currentTile;  // Where unit is now
    public string assignedTile; // Where unit is ordered to move
    
    // Special abilities (for elite units)
    public bool hasSpecialAbility;
    public string specialAbilityName;
    public string specialAbilityDescription;

    /// <summary>
    /// Constructor for standard units
    /// </summary>
    public Unit(string id, string name, string faction, UnitType type, int strength, int movement, string startTile)
    {
        unitID = id;
        unitName = name;
        factionName = faction;
        unitType = type;
        combatStrength = strength;
        movementRange = movement;
        currentTile = startTile;
        assignedTile = null;
        status = UnitStatus.Ready;
        turnsInjured = 0;
        hasSpecialAbility = false;
    }

    /// <summary>
    /// Constructor for elite units with special abilities
    /// </summary>
    public Unit(string id, string name, string faction, UnitType type, int strength, int movement, 
                string startTile, string abilityName, string abilityDesc)
        : this(id, name, faction, type, strength, movement, startTile)
    {
        hasSpecialAbility = true;
        specialAbilityName = abilityName;
        specialAbilityDescription = abilityDesc;
    }

    /// <summary>
    /// Check if unit can be deployed this turn
    /// </summary>
    public bool CanDeploy()
    {
        return status == UnitStatus.Ready;
    }

    /// <summary>
    /// Assign unit to move to a tile
    /// </summary>
    public void AssignToTile(string tileID)
    {
        if (!CanDeploy())
        {
            Debug.LogWarning($"{unitName} cannot be deployed - Status: {status}");
            return;
        }
        
        assignedTile = tileID;
    }

    /// <summary>
    /// Cancel movement assignment
    /// </summary>
    public void Unassign()
    {
        assignedTile = null;
    }

    /// <summary>
    /// Check if unit is assigned to a specific tile
    /// </summary>
    public bool IsAssignedTo(string tileID)
    {
        return assignedTile == tileID;
    }

    /// <summary>
    /// Check if unit has any assignment
    /// </summary>
    public bool IsAssigned()
    {
        return assignedTile != null;
    }

    /// <summary>
    /// Execute the move (after combat resolution)
    /// </summary>
    public void ExecuteMove()
    {
        if (assignedTile != null)
        {
            currentTile = assignedTile;
            assignedTile = null;
        }
    }

    /// <summary>
    /// Mark unit as injured
    /// </summary>
    public void SetInjured()
    {
        status = UnitStatus.Injured;
        turnsInjured = 1; // Injured for 1 turn
        assignedTile = null; // Cancel any assignments
        Debug.Log($"{unitName} is injured and cannot deploy next turn!");
    }

    /// <summary>
    /// Mark unit as destroyed
    /// </summary>
    public void SetDestroyed()
    {
        status = UnitStatus.Destroyed;
        assignedTile = null;
        Debug.Log($"{unitName} has been destroyed!");
    }

    /// <summary>
    /// Process turn for this unit (recover from injury)
    /// </summary>
    public void ProcessTurn()
    {
        if (status == UnitStatus.Injured)
        {
            turnsInjured--;
            
            if (turnsInjured <= 0)
            {
                status = UnitStatus.Ready;
                turnsInjured = 0;
                Debug.Log($"{unitName} has recovered from injury!");
            }
        }
    }

    /// <summary>
    /// Get display string for UI
    /// </summary>
    public string GetDisplayName()
    {
        string statusIcon = "";
        
        switch (status)
        {
            case UnitStatus.Ready:
                statusIcon = "✓";
                break;
            case UnitStatus.Injured:
                statusIcon = $"🤕 ({turnsInjured} turn)";
                break;
            case UnitStatus.Destroyed:
                statusIcon = "💀";
                break;
        }
        
        return $"{unitName} (Str: {combatStrength}, Range: {movementRange}) {statusIcon}";
    }

    /// <summary>
    /// Get unit type display name
    /// </summary>
    public string GetTypeDisplay()
    {
        switch (unitType)
        {
            case UnitType.Grunt:
                return factionName == "Heroes" ? "Cop" : "Thug";
            case UnitType.Elite:
                return factionName == "Heroes" ? "Special Forces" : "Mercenary";
            case UnitType.Scout:
                return "Spy";
            case UnitType.SuperHero:
                return "Hero";
            case UnitType.SuperVillain:
                return "Villain";
            default:
                return "Unknown";
        }
    }
}
