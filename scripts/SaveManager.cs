using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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

		public void SaveLevel(string levelName, Dictionary<string, bool> gatesToSave)
		{
			if (!_saveData.Levels.ContainsKey(levelName))
				_saveData.Levels[levelName] = new LevelData();

			foreach (var gate in gatesToSave)
			{
				_saveData.Levels[levelName].gate[gate.Key] = new GateState { open = gate.Value };
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
					case "gate":
						return level.gate.ContainsKey(objectId)
							? (key == "open" ? level.gate[objectId].open : defaultValue)
							: defaultValue;
					case "pitTrap":
						return level.pitTrap.ContainsKey(objectId)
							? (key == "triggered" ? level.pitTrap[objectId].triggered : defaultValue)
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
