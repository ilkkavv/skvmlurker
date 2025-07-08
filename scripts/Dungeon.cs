using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace DungeonCrawler
{
	public partial class Dungeon : Node3D
	{
		#region Fields and Configuration

		[Export(PropertyHint.File, "*.tscn")]
		private string _startLevelPath;

		[Export] private float _fadeTime = 0.5f;

		private Player _player;
		private ScreenFader _screenFader;
		private Node3D _currentLevel;
		private SaveManager _saveManager;

		private List<Gate> _gates = new();

		#endregion

		#region Godot Lifecycle

		/// <summary>
		/// Called when the node enters the scene tree. Initializes references and loads the starting level.
		/// </summary>
		public override void _Ready()
		{
			// Resolve required references from the scene
			var main = GetTree().Root.GetNodeOrNull("Main");
			if (main == null)
			{
				GD.PrintErr("Dungeon: 'Main' node not found.");
				return;
			}

			_player = main.GetNodeOrNull<Player>("CanvasLayer/SubViewportContainer/SubViewport/Player");
			if (_player == null)
			{
				GD.PrintErr("Dungeon: Failed to find Player node at 'CanvasLayer/SubViewportContainer/SubViewport/Player'.");
				return;
			}

			_screenFader = main.GetNodeOrNull<ScreenFader>("CanvasLayer/ScreenFader");
			if (_screenFader == null)
			{
				GD.PrintErr("Dungeon: Failed to find ScreenFader node at 'CanvasLayer/ScreenFader'.");
				return;
			}

			_saveManager = main.GetNodeOrNull<SaveManager>("SaveManager");
			if (_saveManager == null)
			{
				GD.PrintErr("Dungeon: Failed to find SaveManager node at 'Main/SaveManager'.");
				return;
			}

			// Load initial level
			if (string.IsNullOrEmpty(_startLevelPath))
			{
				GD.PrintErr("Dungeon: No start level path specified.");
				return;
			}

			PackedScene startScene = ResourceLoader.Load<PackedScene>(_startLevelPath);
			if (startScene == null)
			{
				GD.PrintErr($"Dungeon: Failed to load scene from path: {_startLevelPath}");
				return;
			}

			_ = ChangeLevel(startScene, Vector3.Zero, 0f); // Fire and forget
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Sets the player's global position and Y rotation in degrees.
		/// </summary>
		public void SetPlayerPos(Vector3 position, float rotation)
		{
			_player.GlobalPosition = position;
			_player.GlobalRotationDegrees = new Vector3(0, rotation, 0);
		}

		/// <summary>
		/// Changes the current level to the given scene and repositions the player.
		/// Handles fade out/in transitions and optional fall damage.
		/// </summary>
		/// <param name="targetScene">Scene to load as the new level.</param>
		/// <param name="newPlayerPos">Where to place the player.</param>
		/// <param name="newPlayerRot">Optional new Y rotation.</param>
		/// <param name="fallDamage">Whether fall damage should be triggered.</param>
		public async Task ChangeLevel(PackedScene targetScene, Vector3 newPlayerPos, float? newPlayerRot = null, bool fallDamage = false)
		{
			if (targetScene == null)
			{
				GD.PrintErr("Dungeon: Target scene is null.");
				return;
			}

			_player.BlockInput();

			// Fade to black and wait
			_screenFader.FadeToBlack(_fadeTime);
			await ToSignal(GetTree().CreateTimer(_fadeTime), SceneTreeTimer.SignalName.Timeout);

			// Save object states after fade but before clearing
			if (_currentLevel != null)
				SaveObjectStates();
			// Reset object lists
			ClearObjectLists();

			// Remove existing level if any
			if (_currentLevel != null)
			{
				RemoveChild(_currentLevel);
				_currentLevel.QueueFree();
				_currentLevel = null;
			}

			// Instantiate and prepare new level
			Node newLevelInstance = targetScene.Instantiate();
			Node3D newLevelAsNode3D = newLevelInstance as Node3D;

			if (newLevelAsNode3D == null)
			{
				GD.PrintErr("[Dungeon] Loaded level is not a Node3D!");
				return;
			}

			// Assign level name from file path
			string scenePath = targetScene.ResourcePath;
			if (string.IsNullOrEmpty(scenePath))
			{
				GD.PrintErr("[Dungeon] Target scene has no ResourcePath! Assigning fallback name.");
				scenePath = _startLevelPath;
			}

			string levelName = scenePath.GetFile().GetBaseName();

			_currentLevel = newLevelAsNode3D;
			AddChild(_currentLevel);

			GD.Print($"[Dungeon] Loaded level: {_currentLevel.Name}");

			// Position and rotate player
			float finalRot = newPlayerRot ?? _player.GlobalRotationDegrees.Y;
			SetPlayerPos(newPlayerPos, finalRot);

			InitializeObjectStates();

			// Player takes fall damage
			if (fallDamage)
			{
				_player.TakeDamage(1, 6);
			}

			// Fade back in and wait
			_screenFader.FadeBack(_fadeTime);
			await ToSignal(GetTree().CreateTimer(_fadeTime), SceneTreeTimer.SignalName.Timeout);

			// Re-enable player input
			_player.UnblockInput();
		}

		public void AddGate(Gate gate)
		{
			GD.Print($"[Dungeon] Registered gate: {gate.GateId}");
			_gates.Add(gate);
		}

		public bool LoadObjectState(string objectType, string objectId, string key)
		{
			if (_currentLevel == null)
			{
				return false;
			}

			return _saveManager.LoadBoolValue(_currentLevel.Name, objectType, objectId, key);
		}

		#endregion

		private void SaveObjectStates()
		{
			GD.Print($"[Dungeon] Saving {_gates.Count} gates in level '{_currentLevel?.Name}'");

			Dictionary<string, bool> gatesToSave = [];

			foreach (var entry in _gates)
			{
				gatesToSave.Add(entry.GateId, entry._gateOpen);
			}

			_saveManager.SaveLevel(_currentLevel.Name, gatesToSave);
		}

		private void InitializeObjectStates()
		{
			GD.Print($"[Dungeon] Initializing {_gates.Count} gate states for level '{_currentLevel?.Name}'");
			foreach (var gate in _gates)
			{
				GD.Print($"[Dungeon] -> Initializing gate {gate.GateId}");
				gate.InitializeState();
			}
		}

		private void ClearObjectLists()
		{
			_gates.Clear();
		}
	}
}
