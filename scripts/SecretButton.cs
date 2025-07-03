using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// A hidden button that, when activated, presses inward,
	/// plays a sound, and opens a connected gate.
	/// Only activates once.
	/// </summary>
	public partial class SecretButton : Node3D
	{
		#region Exported Settings

		[Export] private Gate _gate;
		[Export] private float _pressDelay = 0.3f;

		#endregion

		#region Private Fields

		private StaticBody3D _tile;
		private AudioStreamPlayer3D _sfxPlayer;
		private bool _pressed = false;

		private static readonly Vector3 PressedPosition = new(0, 0, -0.1f);
		private static readonly Vector3 DefaultPosition = Vector3.Zero;

		#endregion

		#region Lifecycle

		public override void _Ready()
		{
			_tile = GetNodeOrNull<StaticBody3D>("Tile");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			if (_tile == null)
				GD.PrintErr("SecretButton: Tile node not found.");
			if (_sfxPlayer == null)
				GD.PrintErr("SecretButton: SFXPlayer node not found.");
			if (_gate == null)
				GD.PrintErr("SecretButton: Gate reference not assigned.");
		}

		#endregion

		#region Button Logic

		/// <summary>
		/// Presses the tile, plays a sound, and opens the gate. Can only be triggered once.
		/// </summary>
		public async void Activate()
		{
			if (_pressed)
				return;

			_pressed = true;

			if (_tile != null)
				_tile.Position = PressedPosition;

			_sfxPlayer?.Play();

			await ToSignal(GetTree().CreateTimer(_pressDelay), SceneTreeTimer.SignalName.Timeout);

			_gate?.OpenGate();
		}

		#endregion
	}
}
