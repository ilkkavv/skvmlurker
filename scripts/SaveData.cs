using System.Collections.Generic;

namespace DungeonCrawler
{
    /// <summary>
    /// Root object for saved data. Contains level-specific states.
    /// </summary>
    public class SaveData
    {
        public Dictionary<string, LevelData> Levels { get; set; } = new();
    }

    /// <summary>
    /// Holds saved states for all interactive objects in a level.
    /// </summary>
    public class LevelData
    {
        public Dictionary<string, GateState> Gates { get; set; } = new();
        public Dictionary<string, PitTrapState> PitTraps { get; set; } = new();
        public Dictionary<string, IllusoryWallState> IllusoryWalls { get; set; } = new();
        public Dictionary<string, LeverState> Levers { get; set; } = new();
        public Dictionary<string, SecretButtonState> SecretButtons { get; set; } = new();
        public Dictionary<string, TeleportTrapState> TeleportTraps { get; set; } = new();
    }

    /// <summary>State of a Gate object (open or closed).</summary>
    public class GateState
    {
        public bool Open { get; set; }
    }

    /// <summary>State of a PitTrap (triggered or not).</summary>
    public class PitTrapState
    {
        public bool Triggered { get; set; }
    }

    /// <summary>State of an IllusoryWall (revealed or hidden).</summary>
    public class IllusoryWallState
    {
        public bool Revealed { get; set; }
    }

    /// <summary>State of a Lever (on or off).</summary>
    public class LeverState
    {
        public bool On { get; set; }
    }

    /// <summary>State of a SecretButton (pressed or not).</summary>
    public class SecretButtonState
    {
        public bool Pressed { get; set; }
    }

    /// <summary>State of a TeleportTrap (triggered or not).</summary>
    public class TeleportTrapState
    {
        public bool Triggered { get; set; }
    }
}
