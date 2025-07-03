using Godot;

namespace dungeonCrawler
{
	/// <summary>
	/// A pit trap that drops the player to a lower level after a short delay.
	/// Plays a sound, disables collision, and can only trigger once per runtime.
	/// </summary>
	public partial class PitTrap : Node3D
	{
		#region Exported Properties

		[Export(PropertyHint.File, "*.tscn")]
		private string _targetScenePath = "";

		[Export]
		private Vector3 _teleportPosition;

		#endregion

		#region Private Fields

		private StaticBody3D _fakeFloor;
		private AudioStreamPlayer3D _sfxPlayer;
		private Area3D _triggerArea;

		private bool _isTriggered = false;
		private Dungeon _dungeon;

		#endregion

		#region Setup

		/// <summary>
		/// Initializes references to child nodes and connects signal handlers.
		/// </summary>
		public override void _Ready()
		{
			// Get local node references
			_fakeFloor = GetNodeOrNull<StaticBody3D>("FakeFloor");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");

			if (_fakeFloor == null) GD.PrintErr("PitTrap: FakeFloor node not found.");
			if (_sfxPlayer == null) GD.PrintErr("PitTrap: SFXPlayer node not found.");
			if (_triggerArea == null) GD.PrintErr("PitTrap: TriggerArea (Area3D) node not found.");

			// Connect body entry signal
			if (_triggerArea != null)
				_triggerArea.BodyEntered += OnBodyEntered;

			// Get dungeon reference from the root
			Node main = GetTree().Root.GetNodeOrNull("Main");
			_dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");

			if (_dungeon == null)
				GD.PrintErr("PitTrap: Dungeon reference not found.");
		}

		#endregion

		#region Trigger Logic

		/// <summary>
		/// Called when a body enters the trigger area.
		/// If it's the player, the trap is activated.
		/// </summary>
		/// <param name="body">The body that entered the area.</param>
		private void OnBodyEntered(Node3D body)
		{
			if (_isTriggered || body == null)
				return;

			if (body.IsInGroup("player"))
				TriggerTrap();
		}

		/// <summary>
		/// Executes the trap:
		/// - Plays a sound
		/// - Hides and disables the fake floor
		/// - Waits 1 second
		/// - Teleports the player to another scene and location
		/// </summary>
		private async void TriggerTrap()
		{
			_isTriggered = true;

			// Play trap sound
			_sfxPlayer?.Play();

			// Hide floor and disable collision
			if (_fakeFloor != null)
			{
				_fakeFloor.Visible = false;

				var collider = _fakeFloor.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
				if (collider != null)
					collider.SetDeferred("disabled", true);
			}

			// Delay before teleport
			await ToSignal(GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);

			// Change scene and move player
			if (_dungeon != null && !string.IsNullOrEmpty(_targetScenePath))
			{
				var nextScene = ResourceLoader.Load<PackedScene>(_targetScenePath);

				if (nextScene == null)
				{
					GD.PrintErr($"PitTrap: Failed to load scene at '{_targetScenePath}'");
					return;
				}

				await _dungeon.ChangeLevel(nextScene, _teleportPosition, fallDamage: true);
			}
		}

		#endregion
	}
}
