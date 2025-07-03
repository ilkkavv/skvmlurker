using Godot;

namespace dungeonCrawler
{
	public partial class TeleportTrap : Node3D
	{
		#region Exported Settings

		[Export] private Vector3 _targetPosition = Vector3.Zero;
		[Export] private float _targetRotation = 0f;
		[Export] private bool _triggerOnce = false;

		#endregion

		#region Private Fields

		private Area3D _triggerArea;
		private AudioStreamPlayer2D _sfxPlayer;
		private Dungeon _dungeon;
		private ScreenFader _screenFader;
		private Player _player;

		private bool _isTriggered = false;

		#endregion

		#region Lifecycle

		public override void _Ready()
		{
			if (_isTriggered)
			{
				Visible = false;
				return;
			}

			// Get references
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer2D>("SFXPlayer");

			if (_triggerArea == null) GD.PrintErr("TeleportTrap: Missing TriggerArea node.");
			if (_sfxPlayer == null) GD.PrintErr("TeleportTrap: Missing SFXPlayer node.");

			if (_triggerArea != null)
				_triggerArea.BodyEntered += OnBodyEntered;

			// Find dungeon and screen fader
			Node main = GetTree().Root.GetNodeOrNull("Main");
			_dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");
			_screenFader = main?.GetNodeOrNull<ScreenFader>("CanvasLayer/ScreenFader");

			if (_dungeon == null) GD.PrintErr("TeleportTrap: Dungeon not found.");
			if (_screenFader == null) GD.PrintErr("TeleportTrap: ScreenFader not found.");
		}

		#endregion

		#region Trigger Logic

		private void OnBodyEntered(Node3D body)
		{
			if (_isTriggered || body == null)
				return;

			if (body.IsInGroup("player"))
			{
				_player = body.GetParentOrNull<Player>();

				if (_player == null)
				{
					GD.PrintErr("TeleportTrap: Could not resolve Player script.");
					return;
				}

				TriggerTeleport();
			}
		}

		private void TriggerTeleport()
		{
			if (_triggerOnce)
				_isTriggered = true;

			_player.StopPlayer();
			_player.BlockInput();

			_sfxPlayer?.Play();
			_screenFader?.Flash(new Color(0.545f, 0.545f, 0.796f, 1f));

			_dungeon?.SetPlayerPos(_targetPosition, _targetRotation);

			_player.UnblockInput();
		}

		#endregion
	}
}
