using UnityEngine;
using System.Collections.Generic;

// Represents a single tile on the game map
[System.Serializable]
public class Tile
{
    public string tileID;           // Unique identifier (e.g., "DW", "MT", "NS")
    public string tileName;         // Display name (e.g., "Downtown West", "Midtown")
    public Vector2 position;        // Position on the map image
    public int resourceValue;       // Resource points this tile generates
    public string controllingFaction; // Which faction controls this tile (null = neutral or unknown when fog of war is implemented)
    public List<string> adjacentTiles; // List of adjacent tile IDs
    
    // Visual representation
    public Color factionColor = Color.white; // Color of controlling faction

    public Tile(string id, string name, Vector2 pos, int resources)
    {
        tileID = id;
        tileName = name;
        position = pos;
        resourceValue = resources;
        controllingFaction = null;
        adjacentTiles = new List<string>();
        factionColor = Color.white;
    }

    // Check if this tile is adjacent to another tile
    public bool IsAdjacentTo(string otherTileID)
    {
        return adjacentTiles.Contains(otherTileID);
    }

    // Set the controlling faction and update color
    public void SetController(string factionName, Color color)
    {
        controllingFaction = factionName;
        factionColor = color;
    }

    // Clear control (make neutral)
    public void ClearControl()
    {
        controllingFaction = null;
        factionColor = Color.white;
    }
}
