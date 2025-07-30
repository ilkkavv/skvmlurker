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

		[Export] public string SecretButtonId { private set; get; }
		[Export] private Gate _gate;
		[Export] private float _pressDelay = 0.3f;
		[Export] private string _narration = "With a subtle touch, you trigger a hidden mechanism.";

		#endregion

		#region Private Fields

		private StaticBody3D _tile;
		private AudioStreamPlayer3D _sfxPlayer;

		/// <summary>
		/// Whether the button has already been pressed.
		/// </summary>
		public bool _pressed = false;

		private static readonly Vector3 PressedPosition = new(0, 0, -0.1f);

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the node enters the scene. Grabs references and restores state.
		/// </summary>
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

			Global.Dungeon?.AddObject(this);
			InitializeState();
		}

		#endregion

		#region Button Logic

		/// <summary>
		/// Activates the button. Moves it inward, plays a sound, and opens the gate after a short delay.
		/// Only works once.
		/// </summary>
		public async void Activate()
		{
			if (_pressed)
				return;

			_pressed = true;

			Global.MessageBox.Message(_narration, Global.Green);

			if (_tile != null)
				_tile.Position = PressedPosition;

			_sfxPlayer?.Play();

			await ToSignal(GetTree().CreateTimer(_pressDelay), SceneTreeTimer.SignalName.Timeout);

			_gate?.OpenGate();
		}

		#endregion

		#region State Persistence

		/// <summary>
		/// Loads the button's pressed state from save data, and updates visuals if already pressed.
		/// </summary>
		private void InitializeState()
		{
			if (Global.Dungeon == null || string.IsNullOrEmpty(SecretButtonId))
				return;

			_pressed = Global.Dungeon.LoadObjectState("SecretButton", SecretButtonId, "Pressed");

			if (_pressed && _tile != null)
				_tile.Position = PressedPosition;
		}

		#endregion
	}
}
