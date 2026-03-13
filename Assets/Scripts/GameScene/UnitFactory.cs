using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Factory class to create all hero and villain units with proper stats
/// </summary>
public static class UnitFactory
{
    /// <summary>
    /// Create all Hero faction units
    /// </summary>
    public static List<Unit> CreateHeroUnits()
    {
        List<Unit> units = new List<Unit>();

        // ELITE HEROES (2)
        units.Add(new Unit(
            "H_SENTINEL", 
            "Sentinel", 
            "Heroes", 
            UnitType.SuperHero, 
            10, // strength
            2,  // movement
            "MT", // Midtown
            "Defender",
            "Gains +5 strength when defending a tile"
        ));

        units.Add(new Unit(
            "H_VELOCITY", 
            "Velocity", 
            "Heroes", 
            UnitType.SuperHero, 
            6,  // strength
            5,  // movement (fastest unit!)
            "DK", // The Docks
            "Evasion",
            "Never gets injured when retreating from combat"
        ));

        // SPECIAL FORCES (3)
        units.Add(new Unit("H_SF1", "Special Forces Alpha", "Heroes", UnitType.Elite, 5, 3, "MT"));
        units.Add(new Unit("H_SF2", "Special Forces Bravo", "Heroes", UnitType.Elite, 5, 3, "NE"));
        units.Add(new Unit("H_SF3", "Special Forces Charlie", "Heroes", UnitType.Elite, 5, 3, "CH"));

        // COPS (6)
        units.Add(new Unit("H_COP1", "Cop #1", "Heroes", UnitType.Grunt, 3, 2, "MT"));
        units.Add(new Unit("H_COP2", "Cop #2", "Heroes", UnitType.Grunt, 3, 2, "MT"));
        units.Add(new Unit("H_COP3", "Cop #3", "Heroes", UnitType.Grunt, 3, 2, "NE"));
        units.Add(new Unit("H_COP4", "Cop #4", "Heroes", UnitType.Grunt, 3, 2, "CH"));
        units.Add(new Unit("H_COP5", "Cop #5", "Heroes", UnitType.Grunt, 3, 2, "DK"));
        units.Add(new Unit("H_COP6", "Cop #6", "Heroes", UnitType.Grunt, 3, 2, "BW"));

        // SPIES (2)
        units.Add(new Unit(
            "H_SPY1", 
            "Hero Spy Alpha", 
            "Heroes", 
            UnitType.Scout, 
            1,  // strength (cannot capture alone)
            4,  // movement
            "NS", // Northern Suburbs
            "Reconnaissance",
            "Reveals enemy unit positions in adjacent tiles"
        ));
        
        units.Add(new Unit(
            "H_SPY2", 
            "Hero Spy Beta", 
            "Heroes", 
            UnitType.Scout, 
            1,  // strength
            4,  // movement
            "DE", // Downtown East
            "Reconnaissance",
            "Reveals enemy unit positions in adjacent tiles"
        ));

        return units; // Total: 13 units
    }

    /// <summary>
    /// Create all Villain faction units
    /// </summary>
    public static List<Unit> CreateVillainUnits()
    {
        List<Unit> units = new List<Unit>();

        // ELITE VILLAINS (2)
        units.Add(new Unit(
            "V_IRONCLAD", 
            "Ironclad", 
            "Villains", 
            UnitType.SuperVillain, 
            12, // strength (strongest!)
            1,  // movement
            "DW", // Downtown West
            "Overwhelming Force",
            "Attacking units gain +3 strength"
        ));

        units.Add(new Unit(
            "V_PHANTOM", 
            "Phantom", 
            "Villains", 
            UnitType.SuperVillain, 
            7,  // strength
            3,  // movement
            "SS", // Southern Suburbs
            "Shadow Step",
            "50% chance to avoid injury in any combat"
        ));

        // MERCENARIES (3)
        units.Add(new Unit("V_MERC1", "Mercenary Squad 1", "Villains", UnitType.Elite, 6, 2, "DW"));
        units.Add(new Unit("V_MERC2", "Mercenary Squad 2", "Villains", UnitType.Elite, 6, 2, "SS"));
        units.Add(new Unit("V_MERC3", "Mercenary Squad 3", "Villains", UnitType.Elite, 6, 2, "AR"));

        // THUGS (8) - More numerous!
        units.Add(new Unit("V_THUG1", "Thug Gang #1", "Villains", UnitType.Grunt, 4, 1, "DW"));
        units.Add(new Unit("V_THUG2", "Thug Gang #2", "Villains", UnitType.Grunt, 4, 1, "DW"));
        units.Add(new Unit("V_THUG3", "Thug Gang #3", "Villains", UnitType.Grunt, 4, 1, "SS"));
        units.Add(new Unit("V_THUG4", "Thug Gang #4", "Villains", UnitType.Grunt, 4, 1, "SS"));
        units.Add(new Unit("V_THUG5", "Thug Gang #5", "Villains", UnitType.Grunt, 4, 1, "DE"));
        units.Add(new Unit("V_THUG6", "Thug Gang #6", "Villains", UnitType.Grunt, 4, 1, "AR"));
        units.Add(new Unit("V_THUG7", "Thug Gang #7", "Villains", UnitType.Grunt, 4, 1, "BW"));
        units.Add(new Unit("V_THUG8", "Thug Gang #8", "Villains", UnitType.Grunt, 4, 1, "HR"));

        // SPIES (2)
        units.Add(new Unit(
            "V_SPY1", 
            "Villain Spy Alpha", 
            "Villains", 
            UnitType.Scout, 
            1,  // strength (cannot capture alone)
            4,  // movement
            "BS", // Beach Suburbs
            "Reconnaissance",
            "Reveals enemy unit positions in adjacent tiles"
        ));
        
        units.Add(new Unit(
            "V_SPY2", 
            "Villain Spy Beta", 
            "Villains", 
            UnitType.Scout, 
            1,  // strength
            4,  // movement
            "HR", // Hampton Resort
            "Reconnaissance",
            "Reveals enemy unit positions in adjacent tiles"
        ));

        return units; // Total: 15 units
    }

    /// <summary>
    /// Get unit recruitment cost by type
    /// </summary>
    public static int GetRecruitmentCost(UnitType type, string factionName)
    {
        switch (type)
        {
            case UnitType.Grunt:
                return factionName == "Heroes" ? 5 : 4; // Cops cost 5, Thugs cost 4
            
            case UnitType.Elite:
                return 10; // Special Forces and Mercs
            
            case UnitType.Scout:
                return 8; // Spies
            
            case UnitType.SuperHero:
            case UnitType.SuperVillain:
                return -1; // Cannot recruit elite units (unique)
            
            default:
                return 999;
        }
    }

    /// <summary>
    /// Create a new unit of specified type for recruitment
    /// </summary>
    public static Unit RecruitUnit(UnitType type, string factionName, string recruitmentTile, int unitNumber)
    {
        switch (type)
        {
            case UnitType.Grunt:
                if (factionName == "Heroes")
                {
                    return new Unit($"H_COP{unitNumber}", $"Cop #{unitNumber}", "Heroes", UnitType.Grunt, 3, 2, recruitmentTile);
                }
                else
                {
                    return new Unit($"V_THUG{unitNumber}", $"Thug Gang #{unitNumber}", "Villains", UnitType.Grunt, 4, 1, recruitmentTile);
                }
            
            case UnitType.Elite:
                if (factionName == "Heroes")
                {
                    return new Unit($"H_SF{unitNumber}", $"Special Forces {GetSquadName(unitNumber)}", "Heroes", UnitType.Elite, 5, 3, recruitmentTile);
                }
                else
                {
                    return new Unit($"V_MERC{unitNumber}", $"Mercenary Squad {unitNumber}", "Villains", UnitType.Elite, 6, 2, recruitmentTile);
                }
            
            case UnitType.Scout:
                if (factionName == "Heroes")
                {
                    return new Unit(
                        $"H_SPY{unitNumber}", 
                        $"Hero Spy {GetGreekLetter(unitNumber)}", 
                        "Heroes", 
                        UnitType.Scout, 
                        1, 
                        4, 
                        recruitmentTile,
                        "Reconnaissance",
                        "Reveals enemy unit positions in adjacent tiles"
                    );
                }
                else
                {
                    return new Unit(
                        $"V_SPY{unitNumber}", 
                        $"Villain Spy {GetGreekLetter(unitNumber)}", 
                        "Villains", 
                        UnitType.Scout, 
                        1, 
                        4, 
                        recruitmentTile,
                        "Reconnaissance",
                        "Reveals enemy unit positions in adjacent tiles"
                    );
                }
            
            default:
                Debug.LogError($"Cannot recruit unit type: {type}");
                return null;
        }
    }

    /// <summary>
    /// Helper: Get squad name for numbered units
    /// </summary>
    private static string GetSquadName(int number)
    {
        string[] names = { "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel" };
        return (number - 1 < names.Length) ? names[number - 1] : $"Unit {number}";
    }

    /// <summary>
    /// Helper: Get Greek letter for numbered units
    /// </summary>
    private static string GetGreekLetter(int number)
    {
        string[] letters = { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta" };
        return (number - 1 < letters.Length) ? letters[number - 1] : $"#{number}";
    }
}
