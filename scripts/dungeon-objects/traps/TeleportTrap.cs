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

		[Export] public string TeleportTrapId { private set; get; }
		[Export] private Vector3 _targetPosition = Vector3.Zero;
		[Export] private float _targetRotation = 0f;
		[Export] private bool _triggerOnce = false;
		[Export] private string _narration = "In the blink of an eye, the walls shift — you're struck by disorientation.";

		#endregion

		#region Private Fields

		private Area3D _triggerArea;
		private AudioStreamPlayer2D _sfxPlayer;

		private const float _controlDelay = 0.25f;
		public bool _isTriggered = false;

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

			if (_triggerOnce)
			{
				Global.Dungeon?.AddObject(this);
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

			Global.Player.BlockInput();
			TriggerTeleport();
		}

		/// <summary>
		/// Executes teleportation, plays visual/audio effects, and delays input recovery.
		/// </summary>
		private async void TriggerTeleport()
		{
			if (_triggerOnce)
				_isTriggered = true;

			Global.MessageBox.Message(_narration, Global.Blue);

			Global.Player.StopPlayer();
			_sfxPlayer?.Play();

			Global.ScreenFlasher?.Flash(Global.Blue);
			Global.Dungeon?.SetPlayerPos(_targetPosition, _targetRotation);

			await ToSignal(GetTree().CreateTimer(_controlDelay), SceneTreeTimer.SignalName.Timeout);
			Global.Player.UnblockInput();
		}

		#endregion

		#region Save/Load

		/// <summary>
		/// Loads the trigger state for one-time traps.
		/// </summary>s
		private void InitializeState()
		{
			if (string.IsNullOrEmpty(TeleportTrapId))
				return;

			_isTriggered = Global.Dungeon.LoadObjectState("TeleportTrap", TeleportTrapId, "Triggered");
		}

		#endregion
	}
}
