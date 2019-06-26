using System;

namespace AStarPathfinding
{
    /// <summary>
    /// Výčet možných příznaků uzlu při prohledávání
    /// </summary>
    [Flags]
    public enum NodeFlags
    {
        Walkable     = 0x1,     // Průchozí
        Impassable   = 0x2,     // Neprůchozí
        OnOpenList   = 0x4,     // Nachází se na openListu
        OnClosedList = 0x8,     // Nachází se na closedListu
    }

    /// <summary>
    /// Výčet možných stavů čtverců při vykreslování
    /// </summary>
    public enum TileState
    {
        Waklable     =  0x1,
        Impassable   =  0x2,
        OnOpenList   =  0x4,
        OnClosedList =  0x8,
        Start        = 0x10,
        End          = 0x20,
        FinalPath    = 0x40
    }

    /// <summary>
    /// Výčet možných stavů UI
    /// </summary>
    public enum UIState
    {
        Creation,            // Uživatel tvoří mapu
        Pathfinding,         // Algoritmus běží
        Result               // Algoritmus skončil, zobrazení výsledků
    }
}
