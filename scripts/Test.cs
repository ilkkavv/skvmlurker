using Godot;
using System;

namespace dungeon
{
    public partial class Test : Node
    {
        private PackedScene tileScene;

        string[][] dungeonMap = new string[][]
        {
            new string[] { "#", "#", ".", ".", ".", ".", ".", ".", ".", "." },
            new string[] { "#", "#", ".", "#", "#", "#", "#", "#", "#", "#" },
            new string[] { "#", "#", ".", "#", "#", "#", "#", "#", "#", "#" },
            new string[] { ".", ".", ".", "#", "#", "#", "#", "#", "#", "#" },
            new string[] { "#", "#", ".", ".", ".", "#", "#", "#", "#", "#" },
            new string[] { "#", "#", ".", ".", ".", "#", "#", "#", "#", "#" },
            new string[] { "#", "#", ".", ".", ".", ".", ".", ".", "#", "#" },
            new string[] { "#", "#", ".", "#", "#", "#", "#", ".", "#", "#" },
            new string[] { "#", "#", ".", "#", "#", "#", "#", ".", "#", "#" },
            new string[] { "#", "#", ".", ".", ".", ".", ".", ".", "#", "#" }
        };

        private async void GenerateMap()
        {
            for (int x = 0; x < dungeonMap.Length; x++)
            {
                for (int y = 0; y < dungeonMap[x].Length; y++)
                {
                    if (dungeonMap[x][y] == ".")
                    {
                        Node3D tile = (Node3D)tileScene.Instantiate();
                        tile.Position = new Vector3(y * 2, 0, x * 2);
                        AddChild(tile);

                        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                    }
                }
            }
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            tileScene = GD.Load<PackedScene>("res://scenes/open-tile.tscn");

            GenerateMap();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}
