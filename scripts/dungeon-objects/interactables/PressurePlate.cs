using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// A pressure plate that detects when the player steps on it using Area3D.
	/// Plays sound effects, visually moves the plate, and opens a connected gate.
	/// </summary>
	public partial class PressurePlate : Node3D
	{
		#region Exported Properties

		[Export] private Gate _gate;
		[Export] private bool _closeGate = false;
		[Export] private string _pressSfxPath;
		[Export] private string _releaseSfxPath;

		/// <summary>
		/// Duration in seconds to keep the gate open. If 0, the gate stays open indefinitely.
		/// </summary>
		[Export] private float _gateOpenDuration = 0f;

		#endregion

		#region Private Fields

		private Area3D _triggerArea;
		private StaticBody3D _plate;
		private AudioStreamPlayer3D _sfxPlayer;
		private bool _isPressed = false;

		private const float PressDepth = 0.05f;

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the node is added to the scene.
		/// Sets up references and connects signals.
		/// </summary>
		public override void _Ready()
		{
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");
			_plate = GetNodeOrNull<StaticBody3D>("Plate");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			if (_triggerArea == null)
				GD.PrintErr("PressurePlate: Missing TriggerArea node.");
			else
			{
				_triggerArea.BodyEntered += OnBodyEntered;
				_triggerArea.BodyExited += OnBodyExited;
			}

			if (_plate == null)
				GD.PrintErr("PressurePlate: Plate node not found.");

			if (_sfxPlayer == null)
				GD.PrintErr("PressurePlate: SFXPlayer node not found.");
		}

		#endregion

		#region Signal Handlers

		/// <summary>
		/// Called when a body enters the trigger area.
		/// Activates the plate if it's the player.
		/// </summary>
		private void OnBodyEntered(Node3D body)
		{
			if (_isPressed || body == null || !body.IsInGroup("player"))
				return;

			Global.MessageBox.Message("Thy foot falleth upon a stone plate.");
			TogglePlate(true);
		}

		/// <summary>
		/// Called when a body exits the trigger area.
		/// Deactivates the plate if it's the player.
		/// </summary>
		private void OnBodyExited(Node3D body)
		{
			if (!_isPressed || body == null || !body.IsInGroup("player"))
				return;

			TogglePlate(false);
		}

		#endregion

		#region Plate Logic

		/// <summary>
		/// Handles animation, sound, and gate interaction when the plate is pressed or released.
		/// </summary>
		/// <param name="pressed">True to press the plate; false to release it.</param>
		private async void TogglePlate(bool pressed)
		{
			_isPressed = pressed;

			// Animate plate position
			if (_plate != null)
				_plate.Position = pressed ? new Vector3(0, PressDepth, 0) : Vector3.Zero;

			// Play sound
			if (_sfxPlayer != null)
			{
				string sfxPath = pressed ? _pressSfxPath : _releaseSfxPath;
				var stream = GD.Load<AudioStream>(sfxPath);

				if (stream != null)
				{
					_sfxPlayer.Stream = stream;
					_sfxPlayer.Play();
				}
				else
				{
					GD.PrintErr($"PressurePlate: Could not load sound at path: {sfxPath}");
				}
			}

			// Open or close gate depending on _closeGate flag
			if (pressed && _gate != null)
			{
				await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);

				if (_closeGate)
				{
					// If _closeGate is true, close the gate immediately
					_gate.CloseGate();
				}
				else
				{
					// If _closeGate is false, open the gate
					_gate.OpenGate(_gateOpenDuration);
				}
			}
		}

		#endregion
	}
}
