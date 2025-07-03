using Godot;

namespace dungeonCrawler
{
	/// <summary>
	/// Controls a gate that moves vertically to open and close.
	/// Supports sound effects, delays, and smooth step animation.
	/// Includes _isActive flag to prevent async issues on scene change.
	/// </summary>
	public partial class Gate : Node3D
	{
		#region Exported Settings

		[Export] public bool _gateOpen = false;
		[Export] private float _openHeight = 1.4f;
		[Export] private float _stepHeight = 0.2f;
		[Export] private float _openDelay = 0.2f;
		[Export] private float _closeDelay = 0.1f;

		#endregion

		#region Private Fields

		private StaticBody3D _gateBody;
		private AudioStreamPlayer3D _sfxPlayer;
		private bool _isActive = true;

		#endregion

		#region Lifecycle

		public override void _Ready()
		{
			// Get internal nodes
			_gateBody = GetNodeOrNull<StaticBody3D>("GateBody");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			if (_gateBody == null) GD.PrintErr("Gate: GateBody node not found.");
			if (_sfxPlayer == null) GD.PrintErr("Gate: SFXPlayer node not found.");

			// If gate should start open, position it accordingly
			if (_gateOpen && _gateBody != null)
			{
				_gateBody.Position = new Vector3(0, _openHeight, 0);
			}
		}

		public override void _ExitTree()
		{
			// Used to prevent async timers from running after this node is freed
			_isActive = false;
		}

		#endregion

		#region Gate Control

		/// <summary>
		/// Opens the gate by raising the gate body in steps.
		/// Optionally auto-closes after a delay.
		/// </summary>
		/// <param name="timer">Optional timer to auto-close after delay.</param>
		public async void OpenGate(float timer = 0f)
		{
			if (_gateOpen || !_isActive || _gateBody == null)
				return;

			_gateOpen = true;

			float targetY = _openHeight;
			float currentY = _gateBody.Position.Y;

			while (currentY < targetY && _gateOpen && _isActive)
			{
				currentY = Mathf.Min(currentY + _stepHeight, targetY);
				_gateBody.Position = new Vector3(0, currentY, 0);

				PlaySound(1f); // Normal pitch
				await ToSignal(GetTree().CreateTimer(_openDelay), SceneTreeTimer.SignalName.Timeout);
			}

			_gateBody.Position = new Vector3(0, _openHeight, 0);

			if (timer > 0)
			{
				await ToSignal(GetTree().CreateTimer(timer), SceneTreeTimer.SignalName.Timeout);
				CloseGate();
			}
		}

		/// <summary>
		/// Closes the gate by lowering the gate body in steps.
		/// </summary>
		public async void CloseGate()
		{
			if (!_gateOpen || !_isActive || _gateBody == null)
				return;

			_gateOpen = false;

			float targetY = 0f;
			float currentY = _gateBody.Position.Y;

			while (currentY > targetY && !_gateOpen && _isActive)
			{
				currentY = Mathf.Max(currentY - _stepHeight, targetY);
				_gateBody.Position = new Vector3(0, currentY, 0);

				PlaySound(0.8f); // Lower pitch
				await ToSignal(GetTree().CreateTimer(_closeDelay), SceneTreeTimer.SignalName.Timeout);
			}

			_gateBody.Position = new Vector3(0, 0, 0);
		}

		/// <summary>
		/// Plays the gate's movement sound at specified pitch.
		/// </summary>
		private void PlaySound(float pitch)
		{
			if (_sfxPlayer == null || !_isActive) return;

			_sfxPlayer.PitchScale = pitch;
			_sfxPlayer.Play();
		}

		#endregion
	}
}
