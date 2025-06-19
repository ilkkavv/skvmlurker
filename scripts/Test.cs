using Godot;
using System.Collections.Generic;

namespace dungeon
{
    public partial class Test : Node
    {
        [Export] private string TilesPath = "res://scenes/dungeon-tiles/stone/";
        [Export] private string DungeonMapPath = "res://assets/dungeon-maps/dlvl1.csv";

        [Export] private int playerStartPosX;
        [Export] private int playerStartPosY;

        private string[][] dungeonMap;

        private PackedScene cornerTile;
        private PackedScene corridorTile;
        private PackedScene deadEndTile;
        private PackedScene wallTile;
        private PackedScene openTile;

        private static string[][] LoadMapFromCsv(string path)
        {
            var map = new List<string[]>();

            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr("Failed to open map file.");
                return null;
            }

            while (!file.EofReached())
            {
                var line = file.GetLine();
                var row = line.Split(',');
                map.Add(row);
            }

            file.Close();
            return [.. map];
        }

        private PackedScene LoadTile(string name)
        {
            string fullPath = $"{TilesPath}{name}.tscn";
            var scene = GD.Load<PackedScene>(fullPath);

            if (scene == null)
                GD.PrintErr($"Failed to load tile: {name} at path: {fullPath}");

            return scene;
        }

        private void AddDungeonTile(PackedScene dungeonTile, int x, int y, float rotation = 0)
        {
            var instance = dungeonTile.Instantiate<Node3D>();

            var walls = instance.GetNodeOrNull<MeshInstance3D>("Walls");
            if (walls != null)
            {
                walls.RotateY(Mathf.DegToRad(rotation));
            }

            instance.Position = new Vector3(y * 2, 0, x * 2);
            AddChild(instance);
        }

        private void GenerateMap()
        {
            // Check directions in this order: left, up, right, down
            var directions = new Vector2[]
            {
                new Vector2(-1, 0), // Left
                new Vector2(0, -1), // Up
                new Vector2(1, 0),  // Right
                new Vector2(0, 1)   // Down
            };

            // Corner tile patterns: walls on two sides
            var cornerPatterns = new List<(string pattern, int rotation)>
            {
                ("##..",   0),  // wall left + up
                (".##.",  90),  // wall up + right
                ("..##", 180),  // wall right + down
                ("#..#", 270)   // wall down + left
            };

            // Corridor tile patterns: walls on two opposite sides
            var corridorPatterns = new List<(string pattern, int rotation)>
            {
                (".#.#", 0),
                ("#.#.", 90)
            };

            // Dead-end tile patterns: walls on three sides
            var deadEndPatterns = new List<(string pattern, int rotation)>
            {
                ("###.", 90),  // open down
                (".###", 180),  // open left
                ("#.##", 270), // open up
                ("##.#", 0)  // open right
            };

            // Wall tile patterns: wall on one side
            var wallPatterns = new List<(string pattern, int rotation)>
            {
                (".#..", 90),  // wall up
                ("..#.", 180),  // wall right
                ("...#", 270), // wall down
                ("#...", 0)  // wall left
            };

            // Loop through the map, skip edges to avoid index out of range
            for (int x = 1; x < dungeonMap.Length - 1; x++)
            {
                for (int y = 1; y < dungeonMap[x].Length - 1; y++)
                {
                    if (dungeonMap[x][y] != ".")
                        continue; // skip walls

                    // Build neighbor string from left, up, right, down
                    string neighborPattern = "";
                    foreach (var dir in directions)
                    {
                        int nx = x + (int)dir.X;
                        int ny = y + (int)dir.Y;
                        neighborPattern += dungeonMap[nx][ny];
                    }

                    bool matched = false;

                    // Try corner patterns
                    foreach (var (pattern, rotation) in cornerPatterns)
                    {
                        if (neighborPattern == pattern)
                        {
                            AddDungeonTile(cornerTile, x, y, rotation);
                            matched = true;
                            break;
                        }
                    }

                    // Try corridor patterns
                    foreach (var (pattern, rotation) in corridorPatterns)
                    {
                        if (neighborPattern == pattern)
                        {
                            AddDungeonTile(corridorTile, x, y, rotation);
                            matched = true;
                            break;
                        }
                    }

                    // Try dead-end
                    if (!matched)
                    {
                        foreach (var (pattern, rotation) in deadEndPatterns)
                        {
                            if (neighborPattern == pattern)
                            {
                                AddDungeonTile(deadEndTile, x, y, rotation);
                                matched = true;
                                break;
                            }
                        }
                    }

                    // Try wall
                    if (!matched)
                    {
                        foreach (var (pattern, rotation) in wallPatterns)
                        {
                            if (neighborPattern == pattern)
                            {
                                AddDungeonTile(wallTile, x, y, rotation);
                                matched = true;
                                break;
                            }
                        }
                    }

                    // If nothing matches, place an open tile
                    if (!matched)
                    {
                        AddDungeonTile(openTile, x, y);
                    }
                }
            }
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            dungeonMap = LoadMapFromCsv(DungeonMapPath);

            cornerTile = LoadTile("corner-tile");
            corridorTile = LoadTile("corridor-tile");
            deadEndTile = LoadTile("dead-end-tile");
            openTile = LoadTile("open-tile");
            wallTile = LoadTile("wall-tile");

            if (cornerTile == null || corridorTile == null || deadEndTile == null || openTile == null || wallTile == null)
            {
                GD.PrintErr("One or more tiles failed to load. Aborting dungeon generation.");
                return;
            }

            GenerateMap();

            // Move player to a floor tile
            var player = GetNodeOrNull<Node3D>("Player"); // adjust path as needed
            if (player != null)
            {
                player.Position = new Vector3(playerStartPosX * 2, 0, playerStartPosY * 2);
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}
