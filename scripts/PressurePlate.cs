using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// PressurePlate detects when the player steps on it using Area3D.
	/// It plays sound effects, lowers the plate visually, and opens a gate.
	/// </summary>
	public partial class PressurePlate : Node3D
	{
		#region Exported Properties

		[Export] private Gate _gate;
		[Export] private string _pressSfxPath;
		[Export] private string _releaseSfxPath;
		[Export] private float _gateOpenDuration = 0f; // If duration is 0, the gate stays open indefinitely

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
		/// Initializes node references and connects area signals.
		/// </summary>
		public override void _Ready()
		{
			// Find child nodes
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");
			_plate = GetNodeOrNull<StaticBody3D>("Plate");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			// Error checks for critical nodes
			if (_triggerArea == null)
			{
				GD.PrintErr("PressurePlate: Missing TriggerArea node.");
				return;
			}
			if (_plate == null)
				GD.PrintErr("PressurePlate: Plate node not found.");
			if (_sfxPlayer == null)
				GD.PrintErr("PressurePlate: SFXPlayer node not found.");

			// Connect signals
			_triggerArea.BodyEntered += OnBodyEntered;
			_triggerArea.BodyExited += OnBodyExited;
		}

		#endregion

		#region Signal Handlers

		/// <summary>
		/// Called when a body enters the trigger area. If it's the player, activates the plate.
		/// </summary>
		private void OnBodyEntered(Node3D body)
		{
			if (_isPressed || body == null || !body.IsInGroup("player"))
				return;

			TogglePlate(true);
		}

		/// <summary>
		/// Called when a body exits the trigger area. If it's the player, releases the plate.
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
		/// Handles visual, audio, and logic changes when the plate is pressed or released.
		/// </summary>
		/// <param name="pressed">True to press the plate, false to release it.</param>
		private async void TogglePlate(bool pressed)
		{
			_isPressed = pressed;

			// Visually move the plate down or up
			if (_plate != null)
			{
				_plate.Position = pressed ? new Vector3(0, PressDepth, 0) : Vector3.Zero;
			}

			// Load and play appropriate sound effect
			if (_sfxPlayer != null)
			{
				string sfxPath = pressed ? _pressSfxPath : _releaseSfxPath;
				AudioStream stream = GD.Load<AudioStream>(sfxPath);
				if (stream != null)
				{
					_sfxPlayer.Stream = stream;
					_sfxPlayer.Play();
				}
				else
				{
					GD.PrintErr($"PressurePlate: Could not load sound: {sfxPath}");
				}
			}

			// Open gate if assigned and plate is pressed
			if (pressed && _gate != null)
			{
				await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
				_gate.OpenGate(_gateOpenDuration);
			}
		}

		#endregion
	}
}
