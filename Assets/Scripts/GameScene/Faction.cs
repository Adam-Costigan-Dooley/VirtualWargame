using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a faction/team in the game
/// </summary>
[System.Serializable]
public class Faction
{
    public string factionName;      // Name of faction (e.g., "Red Team", "Blue Team")
    public Color factionColor;      // Visual color for this faction
    public int totalResources;      // Total resources accumulated
    public List<Unit> units;        // All units belonging to this faction
    public List<string> controlledTiles; // Tiles controlled by this faction

    public Faction(string name, Color color)
    {
        factionName = name;
        factionColor = color;
        totalResources = 0;
        units = new List<Unit>();
        controlledTiles = new List<string>();
    }

    // Add a unit to this faction
    public void AddUnit(Unit unit)
    {
        if (!units.Contains(unit))
        {
            units.Add(unit);
        }
    }


    // Get all units assigned to a specific tile
    public List<Unit> GetUnitsAssignedTo(string tileID)
    {
        List<Unit> result = new List<Unit>();
        foreach (Unit unit in units)
        {
            if (unit.IsAssignedTo(tileID))
            {
                result.Add(unit);
            }
        }
        return result;
    }


    // Calculate total combat strength at a tile
    public int GetCombatStrengthAt(string tileID)
    {
        int totalStrength = 0;
        foreach (Unit unit in units)
        {
            if (unit.IsAssignedTo(tileID))
            {
                totalStrength += unit.combatStrength;
            }
        }
        return totalStrength;
    }

    // Add resources from controlled tiles
    public void AddResources(int amount)
    {
        totalResources += amount;
    }

    // Claim control of a tile
    public void ClaimTile(string tileID)
    {
        if (!controlledTiles.Contains(tileID))
        {
            controlledTiles.Add(tileID);
        }
    }

    // Lose control of a tile
    public void LoseTile(string tileID)
    {
        controlledTiles.Remove(tileID);
    }
}
