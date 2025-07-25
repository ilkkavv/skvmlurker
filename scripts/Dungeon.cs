using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// The main controller for managing the dungeon, level transitions,
	/// and save/load logic for all interactive dungeon objects.
	/// </summary>
	public partial class Dungeon : Node3D
	{
		#region Exported Configuration

		[Export(PropertyHint.File, "*.tscn")]
		private string _startLevelPath;

		[Export] private Vector3 _playerStartPos = Vector3.Zero;
		[Export] private float _fadeTime = 0.5f;

		#endregion

		#region Runtime References

		private Player _player;
		private ScreenFader _screenFader;
		private SaveManager _saveManager;
		private Node3D _currentLevel;

		#endregion

		#region Tracked Dungeon Objects

		private List<Gate> _gates = new();
		private List<PitTrap> _pitTraps = new();
		private List<IllusoryWall> _illusoryWalls = new();
		private List<Lever> _levers = new();
		private List<SecretButton> _secretButtons = new();
		private List<TeleportTrap> _teleportTraps = new();
		private List<Chest> _chests = new();

		#endregion

		#region Lifecycle

		/// <summary>
		/// Initializes references and loads the starting level on scene entry.
		/// </summary>
		public override void _Ready()
		{
			Node main = GetTree().Root.GetNodeOrNull("Main");
			if (main == null)
			{
				GD.PrintErr("Dungeon: 'Main' node not found.");
				return;
			}

			_player = main.GetNodeOrNull<Player>("CanvasLayer/SubViewportContainer/SubViewport/Player");
			if (_player == null)
			{
				GD.PrintErr("Dungeon: Player not found.");
				return;
			}

			_screenFader = main.GetNodeOrNull<ScreenFader>("CanvasLayer/ScreenFader");
			if (_screenFader == null)
			{
				GD.PrintErr("Dungeon: ScreenFader not found.");
				return;
			}

			_saveManager = main.GetNodeOrNull<SaveManager>("SaveManager");
			if (_saveManager == null)
			{
				GD.PrintErr("Dungeon: SaveManager not found.");
				return;
			}

			if (string.IsNullOrEmpty(_startLevelPath))
			{
				GD.PrintErr("Dungeon: No start level path specified.");
				return;
			}

			var startScene = ResourceLoader.Load<PackedScene>(_startLevelPath);
			if (startScene == null)
			{
				GD.PrintErr($"Dungeon: Failed to load scene from path: {_startLevelPath}");
				return;
			}

			StartNewGame();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Positions the player at the specified location and Y-rotation.
		/// </summary>
		public void SetPlayerPos(Vector3 position, float rotation)
		{
			_player.GlobalPosition = position;
			_player.GlobalRotationDegrees = new Vector3(0, rotation, 0);
		}

		/// <summary>
		/// Transitions to a new level scene, saving current object states
		/// and restoring them after load. Handles fade and optional fall damage.
		/// </summary>
		public async Task ChangeLevel(PackedScene targetScene, Vector3 newPlayerPos,
			float? newPlayerRot = null, bool fallDamage = false, string message = "")
		{
			if (targetScene == null)
			{
				GD.PrintErr("Dungeon: Target scene is null.");
				return;
			}

			_player.BlockInput();
			_screenFader.FadeToBlack(_fadeTime);
			await ToSignal(GetTree().CreateTimer(_fadeTime), SceneTreeTimer.SignalName.Timeout);

			if (_currentLevel != null)
				SaveObjectStates();

			ClearObjectLists();

			if (_currentLevel != null)
			{
				RemoveChild(_currentLevel);
				_currentLevel.QueueFree();
				_currentLevel = null;
			}

			var newLevelInstance = targetScene.Instantiate() as Node3D;
			if (newLevelInstance == null)
			{
				GD.PrintErr("Dungeon: Loaded level is not a Node3D.");
				return;
			}

			string scenePath = targetScene.ResourcePath;
			if (string.IsNullOrEmpty(scenePath))
				scenePath = _startLevelPath;

			_currentLevel = newLevelInstance;
			AddChild(_currentLevel);

			GD.Print($"Dungeon: Loaded level: {_currentLevel.Name}");

			float finalRot = newPlayerRot ?? _player.GlobalRotationDegrees.Y;
			SetPlayerPos(newPlayerPos, finalRot);

			if (fallDamage)
				_player.TakeDamage(1, 6);

			if (_player.Hp > 0)
			{
				_screenFader.FadeBack(_fadeTime);
				await ToSignal(GetTree().CreateTimer(_fadeTime), SceneTreeTimer.SignalName.Timeout);

				if (message != "") Global.MessageBox.Message(message);
				_player.UnblockInput();
			}
		}

		/// <summary>
		/// Registers a dungeon object for later state saving and initialization.
		/// </summary>
		public void AddObject(Node3D obj)
		{
			switch (obj)
			{
				case Gate gate:
					_gates.Add(gate);
					break;
				case PitTrap pitTrap:
					_pitTraps.Add(pitTrap);
					break;
				case IllusoryWall wall:
					_illusoryWalls.Add(wall);
					break;
				case Lever lever:
					_levers.Add(lever);
					break;
				case SecretButton secretButton:
					_secretButtons.Add(secretButton);
					break;
				case TeleportTrap teleportTrap:
					_teleportTraps.Add(teleportTrap);
					break;
				case Chest chest:
					_chests.Add(chest);
					break;
				default:
					GD.PrintErr($"Dungeon: Unhandled object type in AddObject: {obj.GetType().Name}");
					break;
			}
		}

		/// <summary>
		/// Loads a boolean state for a specific object from the current level's saved data.
		/// </summary>
		public bool LoadObjectState(string objectType, string objectId, string key)
		{
			if (_currentLevel == null)
				return false;

			return _saveManager.LoadBoolValue(_currentLevel.Name, objectType, objectId, key);
		}

		/// <summary>
		/// Starts a new game by loading the initial level and positioning the player.
		/// </summary>
		public void StartNewGame()
		{
			if (string.IsNullOrEmpty(_startLevelPath))
			{
				GD.PrintErr("Dungeon: No start level path specified.");
				return;
			}

			var startScene = ResourceLoader.Load<PackedScene>(_startLevelPath);
			if (startScene == null)
			{
				GD.PrintErr($"Dungeon: Failed to load scene from path: {_startLevelPath}");
				return;
			}

			Global.MessageBox.ClearMessages();
			_saveManager.WipeAllLevels();
			_ = ChangeLevel(startScene, _playerStartPos, 0f, message: "Ye step into the gloom — welcome to the dungeon."); // Fire-and-forget load
			_player.Spawn();
		}

		#endregion

		#region State Management

		/// <summary>
		/// Saves the state of all registered dungeon objects into persistent storage.
		/// </summary>
		private void SaveObjectStates()
		{
			Dictionary<string, bool> gatesToSave = [];
			Dictionary<string, bool> pitTrapsToSave = [];
			Dictionary<string, bool> illusoryWallsToSave = [];
			Dictionary<string, bool> leversToSave = [];
			Dictionary<string, bool> secretButtonsToSave = [];
			Dictionary<string, bool> teleportTrapsToSave = [];
			Dictionary<string, bool> chestsToSave = [];

			foreach (var gate in _gates)
				gatesToSave[gate.GateId] = gate._gateOpen;

			foreach (var pitTrap in _pitTraps)
				pitTrapsToSave[pitTrap.PitTrapId] = pitTrap._isTriggered;

			foreach (var wall in _illusoryWalls)
				illusoryWallsToSave[wall.IllusoryWallId] = wall._isRevealed;

			foreach (var lever in _levers)
				leversToSave[lever.LeverId] = lever._leverOn;

			foreach (var button in _secretButtons)
				secretButtonsToSave[button.SecretButtonId] = button._pressed;

			foreach (var trap in _teleportTraps)
				teleportTrapsToSave[trap.TeleportTrapId] = trap._isTriggered;

			foreach (var chest in _chests)
				chestsToSave[chest.ChestId] = chest._chestOpen;

			_saveManager.SaveLevel(
				_currentLevel.Name,
				gatesToSave,
				pitTrapsToSave,
				illusoryWallsToSave,
				leversToSave,
				secretButtonsToSave,
				teleportTrapsToSave,
				chestsToSave
			);
		}

		/// <summary>
		/// Clears all internal object references in preparation for scene unloading.
		/// </summary>
		private void ClearObjectLists()
		{
			_gates.Clear();
			_pitTraps.Clear();
			_illusoryWalls.Clear();
			_levers.Clear();
			_secretButtons.Clear();
			_teleportTraps.Clear();
		}

		#endregion
	}
}
