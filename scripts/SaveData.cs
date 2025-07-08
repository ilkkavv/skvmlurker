using System.Collections.Generic;

public class SaveData
{
    public Dictionary<string, LevelData> Levels { get; set; } = new();
}

public class LevelData
{
    public Dictionary<string, GateState> gate { get; set; } = new();
    public Dictionary<string, PitTrapState> pitTrap { get; set; } = new();
}

public class GateState
{
    public bool open { get; set; }
}

public class PitTrapState
{
    public bool triggered { get; set; }
}
