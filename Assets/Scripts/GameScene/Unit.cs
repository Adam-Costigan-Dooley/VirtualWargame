using UnityEngine;

/// <summary>
/// Represents a unit that can be assigned to tiles
/// </summary>
[System.Serializable]
public class Unit
{
    public string unitID;           // Unique identifier
    public string unitName;         // Display name
    public string factionName;      // Which faction owns this unit
    public int combatStrength;      // How strong in combat
    public string currentTile;      // Tile where unit is currently located
    public string assignedTile;     // Tile unit is assigned to move to (null = not assigned)
    
    public Unit(string id, string name, string faction, int strength, string startTile)
    {
        unitID = id;
        unitName = name;
        factionName = faction;
        combatStrength = strength;
        currentTile = startTile;
        assignedTile = null;
    }

    // Assign this unit to a tile
    public void AssignToTile(string tileID)
    {
        assignedTile = tileID;
    }

    // Unassign this unit
    public void Unassign()
    {
        assignedTile = null;
    }

    // Check if unit is assigned
    public bool IsAssigned()
    {
        return !string.IsNullOrEmpty(assignedTile);
    }

    // Check if unit is assigned to a specific tile
    public bool IsAssignedTo(string tileID)
    {
        return assignedTile == tileID;
    }

    // Move unit to assigned tile (called after turn resolution)
    public void ExecuteMove()
    {
        if (IsAssigned())
        {
            currentTile = assignedTile;
            assignedTile = null;
        }
    }
}
