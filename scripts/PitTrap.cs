using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// A pit trap that drops the player to a lower level after a delay.
	/// Once triggered, the fake floor disappears and cannot be reused.
	/// State is saved between level transitions.
	/// </summary>
	public partial class PitTrap : Node3D
	{
		#region Exported Properties

		/// <summary>
		/// Unique ID used for saving trap state.
		/// </summary>
		[Export] public string PitTrapId { private set; get; }

		/// <summary>
		/// Scene to load when the player falls into the trap.
		/// </summary>
		[Export(PropertyHint.File, "*.tscn")]
		private string _targetScenePath = "";

		/// <summary>
		/// Position where the player will be teleported.
		/// </summary>
		[Export] private Vector3 _teleportPosition;

		/// <summary>
		/// Whether the fall should deal damage to the player.
		/// </summary>
		[Export] private bool _fallDamage = true;

		#endregion

		#region Private Fields

		private StaticBody3D _fakeFloor;
		private AudioStreamPlayer3D _sfxPlayer;
		private Area3D _triggerArea;
		private Dungeon _dungeon;

		#endregion

		#region State

		/// <summary>
		/// Whether the trap has already been triggered in this session.
		/// </summary>
		public bool _isTriggered = false;

		#endregion

		#region Initialization

		/// <summary>
		/// Called when the trap enters the scene. Initializes references and sets up saved state.
		/// </summary>
		public override void _Ready()
		{
			// Get child node references
			_fakeFloor = GetNodeOrNull<StaticBody3D>("FakeFloor");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");

			if (_fakeFloor == null) GD.PrintErr($"{nameof(PitTrap)}: FakeFloor node not found.");
			if (_sfxPlayer == null) GD.PrintErr($"{nameof(PitTrap)}: SFXPlayer node not found.");
			if (_triggerArea == null) GD.PrintErr($"{nameof(PitTrap)}: TriggerArea node not found.");

			// Connect signal
			if (_triggerArea != null)
				_triggerArea.BodyEntered += OnBodyEntered;

			// Get dungeon reference
			Node main = GetTree().Root.GetNodeOrNull("Main");
			_dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");

			if (_dungeon == null)
			{
				GD.PrintErr($"{nameof(PitTrap)}: Dungeon reference not found.");
				return;
			}

			_dungeon.AddObject(this);
			InitializeState();
		}

		/// <summary>
		/// Loads trap state from saved data and updates visual/collider state.
		/// </summary>
		private void InitializeState()
		{
			if (_dungeon == null || string.IsNullOrEmpty(PitTrapId)) return;

			_isTriggered = _dungeon.LoadObjectState("PitTrap", PitTrapId, "Triggered");

			if (_isTriggered && _fakeFloor != null)
			{
				_fakeFloor.Visible = false;
				var collider = _fakeFloor.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
				collider?.SetDeferred("disabled", true);
			}
		}

		#endregion

		#region Trigger Logic

		/// <summary>
		/// Called when a physics body enters the trap area.
		/// Only triggers if the body is in the "player" group.
		/// </summary>
		/// <param name="body">The body that entered the area.</param>
		private void OnBodyEntered(Node3D body)
		{
			if (body == null || !body.IsInGroup("player"))
				return;

			TriggerTrap();
		}

		/// <summary>
		/// Triggers the trap:
		/// - Plays sound
		/// - Hides fake floor and disables its collision
		/// - After a short delay, loads target scene and teleports player
		/// </summary>
		private async void TriggerTrap()
		{
			// Handle trap only if not already triggered
			if (!_isTriggered)
			{
				_sfxPlayer?.Play();

				if (_fakeFloor != null)
				{
					_fakeFloor.Visible = false;
					var collider = _fakeFloor.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
					collider?.SetDeferred("disabled", true);
				}
			}

			_isTriggered = true;

			await ToSignal(GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);

			if (_dungeon == null || string.IsNullOrEmpty(_targetScenePath))
				return;

			var nextScene = ResourceLoader.Load<PackedScene>(_targetScenePath);
			if (nextScene == null)
			{
				GD.PrintErr($"{nameof(PitTrap)}: Failed to load scene at '{_targetScenePath}'");
				return;
			}

			await _dungeon.ChangeLevel(nextScene, _teleportPosition, fallDamage: _fallDamage);
		}

		#endregion
	}
}
