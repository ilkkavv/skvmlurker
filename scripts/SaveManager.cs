using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using Godot;

namespace DungeonCrawler
{
	public partial class SaveManager : Node
	{
		private string _path = ProjectSettings.GlobalizePath("user://save.json");
		private SaveData _saveData = new();

		public override void _Ready()
		{
			if (File.Exists(_path))
			{
				string json = File.ReadAllText(_path);
				_saveData = JsonSerializer.Deserialize<SaveData>(json) ?? new SaveData();
			}
		}

		public void SaveLevel(string levelName, Dictionary<string, bool> gatesToSave,
			Dictionary<string, bool> pitTrapsToSave, Dictionary<string, bool> illusoryWallsToSave,
			Dictionary<string, bool> leversToSave, Dictionary<string, bool> secretButtonsToSave,
			Dictionary<string, bool> teleportTrapsToSave)
		{
			if (!_saveData.Levels.ContainsKey(levelName))
				_saveData.Levels[levelName] = new LevelData();

			foreach (var gate in gatesToSave)
			{
				_saveData.Levels[levelName].gate[gate.Key] = new GateState { Open = gate.Value };
			}
			foreach (var pitTrap in pitTrapsToSave)
			{
				_saveData.Levels[levelName].pitTrap[pitTrap.Key] = new PitTrapState { Triggered = pitTrap.Value };
			}
			foreach (var illusoryWall in illusoryWallsToSave)
			{
				_saveData.Levels[levelName].illusoryWall[illusoryWall.Key] = new IllusoryWallState { Revealed = illusoryWall.Value };
			}
			foreach (var lever in leversToSave)
			{
				_saveData.Levels[levelName].lever[lever.Key] = new LeverState { On = lever.Value };
			}
			foreach (var secretButton in secretButtonsToSave)
			{
				_saveData.Levels[levelName].secretButton[secretButton.Key] = new SecretButtonState { Pressed = secretButton.Value };
			}
			foreach (var teleportTrap in teleportTrapsToSave)
			{
				_saveData.Levels[levelName].teleportTrap[teleportTrap.Key] = new TeleportTrapState { Triggered = teleportTrap.Value };
			}

			string updatedJson = JsonSerializer.Serialize(_saveData, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(_path, updatedJson);
		}

		public bool LoadBoolValue(string levelName, string objectType, string objectId, string key, bool defaultValue = false)
		{
			if (!_saveData.Levels.ContainsKey(levelName))
				return defaultValue;

			var level = _saveData.Levels[levelName];

			try
			{
				switch (objectType)
				{
					case "Gate":
						return level.gate.ContainsKey(objectId)
							? (key == "Open" ? level.gate[objectId].Open : defaultValue)
							: defaultValue;
					case "PitTrap":
						return level.pitTrap.ContainsKey(objectId)
							? (key == "Triggered" ? level.pitTrap[objectId].Triggered : defaultValue)
							: defaultValue;
					case "IllusoryWall":
						return level.illusoryWall.ContainsKey(objectId)
							? (key == "Revealed" ? level.illusoryWall[objectId].Revealed : defaultValue)
							: defaultValue;
					case "Lever":
						return level.lever.ContainsKey(objectId)
							? (key == "On" ? level.lever[objectId].On : defaultValue)
							: defaultValue;
					case "SecretButton":
						return level.secretButton.ContainsKey(objectId)
							? (key == "Pressed" ? level.secretButton[objectId].Pressed : defaultValue)
							: defaultValue;
					case "TeleportTrap":
						return level.teleportTrap.ContainsKey(objectId)
							? (key == "Triggered" ? level.teleportTrap[objectId].Triggered : defaultValue)
							: defaultValue;
					default:
						return defaultValue;
				}
			}
			catch
			{
				return defaultValue;
			}
		}
	}
}
