using System.Collections.Generic;

namespace DungeonCrawler
{
    public class SaveData
    {
        public Dictionary<string, LevelData> Levels { get; set; } = new();
    }

    public class LevelData
    {
        public Dictionary<string, GateState> gate { get; set; } = new();
        public Dictionary<string, PitTrapState> pitTrap { get; set; } = new();
        public Dictionary<string, IllusoryWallState> illusoryWall { get; set; } = new();
        public Dictionary<string, LeverState> lever { get; set; } = new();
        public Dictionary<string, SecretButtonState> secretButton { get; set; } = new();
        public Dictionary<string, TeleportTrapState> teleportTrap { get; set; } = new();
    }

    public class GateState
    {
        public bool Open { get; set; }
    }

    public class PitTrapState
    {
        public bool Triggered { get; set; }
    }

    public class IllusoryWallState
    {
        public bool Revealed { get; set; }
    }

    public class LeverState
    {
        public bool On { get; set; }
    }

    public class SecretButtonState
    {
        public bool Pressed { get; set; }
    }

    public class TeleportTrapState
    {
        public bool Triggered { get; set; }
    }
}

