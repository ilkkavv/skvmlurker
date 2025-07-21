using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Handles saving and loading state data for interactive dungeon elements such as traps, gates, levers, etc.
	/// Persists data to a JSON file located in the user directory.
	/// </summary>
	public partial class SaveManager : Node
	{
		#region Fields

		/// <summary>Path to the save file in the user's directory.</summary>
		private readonly string _path = ProjectSettings.GlobalizePath("user://save.json");

		/// <summary>Holds all saved level data in memory.</summary>
		private SaveData _saveData = new();

		#endregion

		#region Godot Lifecycle

		/// <summary>
		/// On startup, loads save file from disk if it exists.
		/// </summary>
		public override void _Ready()
		{
			if (File.Exists(_path))
			{
				string json = File.ReadAllText(_path);
				_saveData = JsonSerializer.Deserialize<SaveData>(json) ?? new SaveData();
			}
		}

		#endregion

		#region Save Logic

		/// <summary>
		/// Saves state data for a specific level, categorized by object type and ID.
		/// </summary>
		/// <param name="levelName">Name of the level being saved.</param>
		public void SaveLevel(
			string levelName,
			Dictionary<string, bool> gatesToSave,
			Dictionary<string, bool> pitTrapsToSave,
			Dictionary<string, bool> illusoryWallsToSave,
			Dictionary<string, bool> leversToSave,
			Dictionary<string, bool> secretButtonsToSave,
			Dictionary<string, bool> teleportTrapsToSave,
			Dictionary<string, bool> chestsToSave)
		{
			if (!_saveData.Levels.ContainsKey(levelName))
				_saveData.Levels[levelName] = new LevelData();

			var level = _saveData.Levels[levelName];

			foreach (var gate in gatesToSave)
				level.Gates[gate.Key] = new GateState { Open = gate.Value };

			foreach (var pitTrap in pitTrapsToSave)
				level.PitTraps[pitTrap.Key] = new PitTrapState { Triggered = pitTrap.Value };

			foreach (var wall in illusoryWallsToSave)
				level.IllusoryWalls[wall.Key] = new IllusoryWallState { Revealed = wall.Value };

			foreach (var lever in leversToSave)
				level.Levers[lever.Key] = new LeverState { On = lever.Value };

			foreach (var button in secretButtonsToSave)
				level.SecretButtons[button.Key] = new SecretButtonState { Pressed = button.Value };

			foreach (var trap in teleportTrapsToSave)
				level.TeleportTraps[trap.Key] = new TeleportTrapState { Triggered = trap.Value };

			foreach (var chest in chestsToSave)
				level.Chests[chest.Key] = new ChestState { Open = chest.Value };

			// Write to disk as formatted JSON
			string json = JsonSerializer.Serialize(_saveData, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(_path, json);
		}

		#endregion

		#region Load Logic

		/// <summary>
		/// Loads a boolean value for a specific object within a level.
		/// </summary>
		/// <param name="levelName">Name of the level to query.</param>
		/// <param name="objectType">Type of object (e.g., "Gate", "PitTrap").</param>
		/// <param name="objectId">Unique identifier of the object.</param>
		/// <param name="key">Property to retrieve (e.g., "Open", "Triggered").</param>
		/// <param name="defaultValue">Fallback if key is not found.</param>
		/// <returns>The saved value or default if missing.</returns>
		public bool LoadBoolValue(string levelName, string objectType, string objectId, string key, bool defaultValue = false)
		{
			if (!_saveData.Levels.TryGetValue(levelName, out var level))
				return defaultValue;

			try
			{
				return objectType switch
				{
					"Gate" => level.Gates.TryGetValue(objectId, out var g) ? (key == "Open" ? g.Open : defaultValue) : defaultValue,
					"PitTrap" => level.PitTraps.TryGetValue(objectId, out var p) ? (key == "Triggered" ? p.Triggered : defaultValue) : defaultValue,
					"IllusoryWall" => level.IllusoryWalls.TryGetValue(objectId, out var w) ? (key == "Revealed" ? w.Revealed : defaultValue) : defaultValue,
					"Lever" => level.Levers.TryGetValue(objectId, out var l) ? (key == "On" ? l.On : defaultValue) : defaultValue,
					"SecretButton" => level.SecretButtons.TryGetValue(objectId, out var b) ? (key == "Pressed" ? b.Pressed : defaultValue) : defaultValue,
					"TeleportTrap" => level.TeleportTraps.TryGetValue(objectId, out var t) ? (key == "Triggered" ? t.Triggered : defaultValue) : defaultValue,
					"Chest" => level.Chests.TryGetValue(objectId, out var t) ? (key == "Open" ? t.Open : defaultValue) : defaultValue,
					_ => defaultValue
				};
			}
			catch
			{
				return defaultValue;
			}
		}

		#endregion
	}
}
