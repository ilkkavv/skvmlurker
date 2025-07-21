using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace DungeonCrawler
{
	[Tool]
	public partial class DungeonLevelGeneratorPlugin : EditorPlugin
	{
		private Control _panel;
		private string _csvPath;
		private string _floorScenePath;
		private string _wallScenePath;
		private string _ceilingScenePath;
		private string _waterScenePath;
		private string _ditchWallScenePath;
		private int _instanceCounter = 1;

		public override void _EnterTree()
		{
			var panelScene = GD.Load<PackedScene>("res://addons/dungeon-level-generator/dungeon-level-generator-panel.tscn");
			_panel = panelScene.Instantiate<Control>();
			AddControlToDock(DockSlot.RightUl, _panel);

			_panel.GetNode<Button>("VBoxContainer/PickCSVButton").Pressed += () =>
				_panel.GetNode<FileDialog>("VBoxContainer/CSVPicker").Popup();

			_panel.GetNode<FileDialog>("VBoxContainer/CSVPicker").FileSelected += (string path) =>
			{
				_csvPath = path;
				_panel.GetNode<Label>("VBoxContainer/CSVPathLabel").Text = path;
			};

			ConnectScenePicker("PickFloorSceneButton", "FloorScenePickerDialog", "FloorScenePathLabel", path => _floorScenePath = path);
			ConnectScenePicker("PickWallSceneButton", "WallScenePickerDialog", "WallScenePathLabel", path => _wallScenePath = path);
			ConnectScenePicker("PickCeilingSceneButton", "CeilingScenePickerDialog", "CeilingScenePathLabel", path => _ceilingScenePath = path);
			ConnectScenePicker("PickWaterSceneButton", "WaterScenePickerDialog", "WaterScenePathLabel", path => _waterScenePath = path);
			ConnectScenePicker("PickDitchWallSceneButton", "DitchWallScenePickerDialog", "DitchWallScenePathLabel", path => _ditchWallScenePath = path);

			_panel.GetNode<Button>("VBoxContainer/GenerateFloorsButton").Pressed += () => Generate("floor");
			_panel.GetNode<Button>("VBoxContainer/GenerateWallsButton").Pressed += () => Generate("wall");
			_panel.GetNode<Button>("VBoxContainer/GenerateCeilingsButton").Pressed += () => Generate("ceiling");
			_panel.GetNode<Button>("VBoxContainer/GenerateWaterButton").Pressed += () => Generate("water");
			_panel.GetNode<Button>("VBoxContainer/GenerateDitchWallsButton").Pressed += () => Generate("ditchwall");
		}

		public override void _ExitTree()
		{
			RemoveControlFromDocks(_panel);
			_panel.QueueFree();
		}

		private void ConnectScenePicker(string buttonName, string dialogName, string labelName, Action<string> setter)
		{
			var button = _panel.GetNode<Button>($"VBoxContainer/{buttonName}");
			var dialog = _panel.GetNode<FileDialog>($"VBoxContainer/{dialogName}");
			var label = _panel.GetNode<Label>($"VBoxContainer/{labelName}");

			button.Pressed += () => dialog.Popup();
			dialog.FileSelected += (string path) =>
			{
				setter(path);
				label.Text = path;
			};
		}

		private void Generate(string type)
		{
			_instanceCounter = 1;

			if (string.IsNullOrEmpty(_csvPath)) { ShowError("CSV file not selected!"); return; }
			if (type == "floor" && string.IsNullOrEmpty(_floorScenePath)) { ShowError("Floor scene not selected."); return; }
			if (type == "wall" && string.IsNullOrEmpty(_wallScenePath)) { ShowError("Wall scene not selected."); return; }
			if (type == "ceiling" && string.IsNullOrEmpty(_ceilingScenePath)) { ShowError("Ceiling scene not selected."); return; }
			if (type == "water" && string.IsNullOrEmpty(_waterScenePath)) { ShowError("Water scene not selected."); return; }
			if (type == "ditchwall" && string.IsNullOrEmpty(_ditchWallScenePath)) { ShowError("Ditch Wall scene not selected."); return; }

			var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
			if (selected.Count == 0 || selected[0] is not Node3D root)
			{
				ShowError("Select a Node3D as parent before generating.");
				return;
			}

			var map = ParseCsv(_csvPath);
			var parent = new Node3D { Name = type.Capitalize() + "s" };
			root.AddChild(parent);
			parent.Owner = GetTree().EditedSceneRoot;

			for (int row = 0; row < map.Length; row++)
			{
				for (int col = 0; col < map[row].Length; col++)
				{
					string tile = map[row][col];
					Vector3 pos = new(col * 2, 0, row * 2);  // ✅ No offset

					string up = row > 0 ? map[row - 1][col] : "";
					string down = row < map.Length - 1 ? map[row + 1][col] : "";
					string left = col > 0 ? map[row][col - 1] : "";
					string right = col < map[row].Length - 1 ? map[row][col + 1] : "";

					switch (type)
					{
						case "floor":
							if (tile == ".")
								InstanceScene(_floorScenePath, parent, pos, Vector3.Zero);
							break;

						case "wall":
							if (tile == ".")
							{
								if (up == "#") InstanceScene(_wallScenePath, parent, pos, new Vector3(0, 0, 0));
								if (left == "#") InstanceScene(_wallScenePath, parent, pos, new Vector3(0, 90, 0));
								if (right == "#") InstanceScene(_wallScenePath, parent, pos, new Vector3(0, -90, 0));
								if (down == "#") InstanceScene(_wallScenePath, parent, pos, new Vector3(0, 180, 0));
							}
							break;

						case "ceiling":
							if (tile != "#")
								InstanceScene(_ceilingScenePath, parent, pos, Vector3.Zero);
							break;

						case "water":
							if (tile == "~")
								InstanceScene(_waterScenePath, parent, pos, Vector3.Zero);
							break;

						case "ditchwall":
							if (tile == "~")
							{
								if (up != "~") InstanceScene(_ditchWallScenePath, parent, pos, Vector3.Zero);
								if (up != "." && up != "~") InstanceScene(_wallScenePath, parent, pos, Vector3.Zero);

								if (left != "~") InstanceScene(_ditchWallScenePath, parent, pos, new Vector3(0, 90, 0));
								if (left != "." && left != "~") InstanceScene(_wallScenePath, parent, pos, new Vector3(0, 90, 0));

								if (right != "~") InstanceScene(_ditchWallScenePath, parent, pos, new Vector3(0, -90, 0));
								if (right != "." && right != "~") InstanceScene(_wallScenePath, parent, pos, new Vector3(0, -90, 0));

								if (down != "~") InstanceScene(_ditchWallScenePath, parent, pos, new Vector3(0, 180, 0));
								if (down != "." && down != "~") InstanceScene(_wallScenePath, parent, pos, new Vector3(0, 180, 0));
							}
							break;
					}
				}
			}

			GD.Print($"{type.Capitalize()}s generated.");
		}

		private string[][] ParseCsv(string path)
		{
			var data = new List<string[]>();
			foreach (string line in File.ReadAllLines(ProjectSettings.GlobalizePath(path)))
			{
				data.Add(line.Split(","));
			}
			return data.ToArray();
		}

		private void InstanceScene(string scenePath, Node parent, Vector3 position, Vector3 rotationDegrees)
		{
			var scene = GD.Load<PackedScene>(scenePath);
			if (scene == null) return;

			var instance = scene.Instantiate<Node3D>();
			instance.Position = position;
			instance.RotationDegrees = rotationDegrees;

			string prefix = parent.Name.ToString().TrimEnd('s'); // Convert to string first
			instance.Name = $"{prefix}{_instanceCounter++}";

			parent.AddChild(instance);
			instance.Owner = GetTree().EditedSceneRoot;
		}

		private void ShowError(string message)
		{
			GD.PrintErr(message);
		}
	}
}
