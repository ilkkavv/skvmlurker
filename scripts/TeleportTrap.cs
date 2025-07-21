using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// A teleport trap that moves the player instantly to a defined position and rotation when entered.
	/// Can be configured to trigger once or repeatedly. Plays a sound and visual flash when triggered.
	/// </summary>
	public partial class TeleportTrap : Node3D
	{
		#region Exported Settings

		/// <summary>Unique identifier used for save/load state tracking.</summary>
		[Export] public string TeleportTrapId { private set; get; }

		/// <summary>Target global position to teleport the player to.</summary>
		[Export] private Vector3 _targetPosition = Vector3.Zero;

		/// <summary>Y-axis rotation (in degrees) to apply to the player after teleport.</summary>
		[Export] private float _targetRotation = 0f;

		/// <summary>If true, this trap can only trigger once per runtime.</summary>
		[Export] private bool _triggerOnce = false;

		#endregion

		#region Private Fields

		private Area3D _triggerArea;
		private AudioStreamPlayer2D _sfxPlayer;
		private Dungeon _dungeon;
		private ScreenFlasher _screenFlasher;
		private Player _player;

		private const float _controlDelay = 0.25f; // Delay before regaining control
		public bool _isTriggered = false;          // Flag for one-time triggers

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the node enters the scene tree.
		/// Grabs required references and sets up signal handling.
		/// </summary>
		public override void _Ready()
		{
			if (_isTriggered)
			{
				Visible = false;
				return;
			}

			// Local node references
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer2D>("SFXPlayer");

			if (_triggerArea == null)
				GD.PrintErr("TeleportTrap: Missing TriggerArea node.");
			if (_sfxPlayer == null)
				GD.PrintErr("TeleportTrap: Missing SFXPlayer node.");
			else
				_triggerArea.BodyEntered += OnBodyEntered;

			// External references
			Node main = GetTree().Root.GetNodeOrNull("Main");
			_dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");
			_screenFlasher = main?.GetNodeOrNull<ScreenFlasher>("CanvasLayer/ScreenFlasher");

			if (_dungeon == null)
				GD.PrintErr("TeleportTrap: Dungeon not found.");
			if (_screenFlasher == null)
				GD.PrintErr("TeleportTrap: ScreenFlasher not found.");

			if (_triggerOnce)
			{
				_dungeon?.AddObject(this);
				InitializeState();
			}
		}

		#endregion

		#region Trigger Logic

		/// <summary>
		/// Fired when a body enters the trigger area. Teleports player if conditions are met.
		/// </summary>
		private void OnBodyEntered(Node3D body)
		{
			if (_isTriggered || body == null || !body.IsInGroup("player"))
				return;

			_player = body.GetParentOrNull<Player>();
			if (_player == null)
			{
				GD.PrintErr("TeleportTrap: Could not resolve Player node.");
				return;
			}

			_player.BlockInput();
			TriggerTeleport();
		}

		/// <summary>
		/// Executes teleportation, plays visual/audio effects, and delays input recovery.
		/// </summary>
		private async void TriggerTeleport()
		{
			if (_triggerOnce)
				_isTriggered = true;

			_player.StopPlayer();
			_sfxPlayer?.Play();

			_screenFlasher?.Flash(new Color(0.545f, 0.545f, 0.796f, 1f)); // Light purple flash
			_dungeon?.SetPlayerPos(_targetPosition, _targetRotation);

			await ToSignal(GetTree().CreateTimer(_controlDelay), SceneTreeTimer.SignalName.Timeout);
			_player.UnblockInput();
		}

		#endregion

		#region Save/Load

		/// <summary>
		/// Loads the trigger state for one-time traps.
		/// </summary>
		private void InitializeState()
		{
			if (_dungeon == null || string.IsNullOrEmpty(TeleportTrapId))
				return;

			_isTriggered = _dungeon.LoadObjectState("TeleportTrap", TeleportTrapId, "Triggered");
		}

		#endregion
	}
}
