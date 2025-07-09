using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Controls a vertically sliding gate. Supports sound effects, smooth step animation,
	/// open/close delays, and persistent state loading. Prevents async actions after freeing.
	/// </summary>
	public partial class Gate : Node3D
	{
		#region Exported Properties

		[Export] public string GateId { private set; get; }

		[Export] public bool _gateOpen = false;
		[Export] private float _openHeight = 1.4f;
		[Export] private float _stepHeight = 0.2f;
		[Export] private float _openDelay = 0.2f;
		[Export] private float _closeDelay = 0.1f;

		#endregion

		#region Private Fields

		private Dungeon _dungeon;
		private StaticBody3D _gateBody;
		private AudioStreamPlayer3D _sfxPlayer;

		// Prevents timers from running after scene unload
		private bool _isActive = true;

		#endregion

		#region Lifecycle

		/// <summary>
		/// Initializes node references, registers with the dungeon system, and sets gate state.
		/// </summary>
		public override void _Ready()
		{
			Node main = GetTree().Root.GetNodeOrNull("Main");
			_dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");

			_gateBody = GetNodeOrNull<StaticBody3D>("GateBody");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			if (_dungeon == null) GD.PrintErr("Gate: Dungeon not found.");
			if (_gateBody == null) GD.PrintErr("Gate: GateBody node not found.");
			if (_sfxPlayer == null) GD.PrintErr("Gate: SFXPlayer node not found.");
			if (string.IsNullOrEmpty(GateId)) GD.PrintErr("Gate: ID not set.");

			_dungeon?.AddObject(this);
			InitializeState();
		}

		/// <summary>
		/// Deactivates the gate to prevent async actions after it's freed.
		/// </summary>
		public override void _ExitTree()
		{
			_isActive = false;
		}

		#endregion

		#region Gate Control

		/// <summary>
		/// Opens the gate by moving the gate body upward in steps.
		/// Optionally auto-closes after a timer delay.
		/// </summary>
		/// <param name="timer">Delay before automatically closing (0 = don't close).</param>
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
				_gateOpen = false;
				await ToSignal(GetTree().CreateTimer(timer), SceneTreeTimer.SignalName.Timeout);
				CloseGate();
			}
		}

		/// <summary>
		/// Closes the gate by moving the gate body downward in steps.
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

		#endregion

		#region Initialization & Audio

		/// <summary>
		/// Loads the saved gate state from the dungeon system and adjusts position accordingly.
		/// </summary>
		private void InitializeState()
		{
			if (_dungeon == null || string.IsNullOrEmpty(GateId)) return;

			_gateOpen = _dungeon.LoadObjectState("Gate", GateId, "Open");

			if (_gateOpen && _gateBody != null)
				_gateBody.Position = new Vector3(0, _openHeight, 0);
		}

		/// <summary>
		/// Plays a gate movement sound with a specified pitch scale.
		/// </summary>
		private void PlaySound(float pitch)
		{
			if (_sfxPlayer == null || !_isActive)
				return;

			_sfxPlayer.PitchScale = pitch;
			_sfxPlayer.Play();
		}

		#endregion
	}
}
