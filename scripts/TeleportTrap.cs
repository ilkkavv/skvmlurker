using Godot;

namespace dungeonCrawler
{
	/// <summary>
	/// A teleport trap that instantly moves the player to a target position and rotation upon entering a trigger area.
	/// Optionally limits the trap to trigger only once. Plays a sound and screen flash when activated.
	/// </summary>
	public partial class TeleportTrap : Node3D
	{
		#region Exported Settings

		[Export] private Vector3 _targetPosition = Vector3.Zero;  // Destination position for the player
		[Export] private float _targetRotation = 0f;              // Y-axis rotation to apply after teleport
		[Export] private bool _triggerOnce = false;               // Whether the trap can be triggered only once

		#endregion

		#region Private Fields

		private Area3D _triggerArea;
		private AudioStreamPlayer2D _sfxPlayer;
		private Dungeon _dungeon;
		private ScreenFader _screenFader;
		private Player _player;

		private bool _isTriggered = false; // Internal flag to prevent multiple triggers

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the node enters the scene tree.
		/// Resolves child nodes and dependencies, and connects signals.
		/// </summary>
		public override void _Ready()
		{
			// Skip setup if already triggered (e.g., if trap is one-time use)
			if (_isTriggered)
			{
				Visible = false;
				return;
			}

			// Resolve internal node references
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer2D>("SFXPlayer");

			if (_triggerArea == null)
				GD.PrintErr("TeleportTrap: Missing TriggerArea node.");

			if (_sfxPlayer == null)
				GD.PrintErr("TeleportTrap: Missing SFXPlayer node.");

			// Connect trigger signal
			if (_triggerArea != null)
				_triggerArea.BodyEntered += OnBodyEntered;

			// Resolve references to world systems
			Node main = GetTree().Root.GetNodeOrNull("Main");
			_dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");
			_screenFader = main?.GetNodeOrNull<ScreenFader>("CanvasLayer/ScreenFader");

			if (_dungeon == null)
				GD.PrintErr("TeleportTrap: Dungeon not found.");

			if (_screenFader == null)
				GD.PrintErr("TeleportTrap: ScreenFader not found.");
		}

		#endregion

		#region Trigger Logic

		/// <summary>
		/// Called when a body enters the trigger area.
		/// Triggers the teleport if the body is the player.
		/// </summary>
		private void OnBodyEntered(Node3D body)
		{
			// Ignore if already used or invalid body
			if (_isTriggered || body == null)
				return;

			// Check for player tag
			if (body.IsInGroup("player"))
			{
				// Resolve the Player script (assumes trigger is child of PlayerController)
				_player = body.GetParentOrNull<Player>();

				if (_player == null)
				{
					GD.PrintErr("TeleportTrap: Could not resolve Player script.");
					return;
				}

				TriggerTeleport();
			}
		}

		/// <summary>
		/// Executes the teleport: stops input, plays effect, moves player, and restores input.
		/// </summary>
		private void TriggerTeleport()
		{
			// Disable future triggers if set to single use
			if (_triggerOnce)
				_isTriggered = true;

			// Freeze player and play effect
			_player?.StopPlayer();
			_player?.BlockInput();
			_sfxPlayer?.Play();

			// Screen flash for teleport effect
			_screenFader?.Flash(new Color(0.545f, 0.545f, 0.796f, 1f)); // Light purple

			// Move player
			_dungeon?.SetPlayerPos(_targetPosition, _targetRotation);

			// Restore player control
			_player?.UnblockInput();
		}

		#endregion
	}
}
