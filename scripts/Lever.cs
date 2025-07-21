using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// A lever that toggles its state and opens/closes connected gates.
	/// Includes handle animation, sound effect, and persistent state loading.
	/// </summary>
	public partial class Lever : Node3D
	{
		#region Exported Properties

		/// <summary>
		/// Unique identifier used for state saving/loading.
		/// </summary>
		[Export] public string LeverId { private set; get; }

		/// <summary>
		/// Array of gates this lever controls. Assigned via editor.
		/// </summary>
		[Export] private Gate[] _gatesArray;

		#endregion

		#region Constants

		private static readonly Vector3 HandleDownPosition = new(0, -0.5f, 0);
		private static readonly Vector3 HandleUpPosition = Vector3.Zero;
		private const float LeverDelaySeconds = 0.3f;

		#endregion

		#region Private Fields

		private StaticBody3D _handle;
		private AudioStreamPlayer3D _sfxPlayer;
		private Dungeon _dungeon;

		private float _cooldown = 1f;
		private bool _useBlock = false;

		/// <summary>
		/// Current state of the lever (true = down/on).
		/// </summary>
		public bool _leverOn;

		private List<Gate> _gates = new();

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the lever enters the scene tree.
		/// Initializes references, gathers assigned gates, and loads saved state.
		/// </summary>
		public override void _Ready()
		{
			_handle = GetNodeOrNull<StaticBody3D>("Handle");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			if (_gatesArray != null)
				_gates = [.. _gatesArray.Where(g => g != null)];

			if (_handle == null) GD.PrintErr("Lever: Handle node not found.");
			if (_sfxPlayer == null) GD.PrintErr("Lever: SFXPlayer node not found.");
			if (_gates.Count == 0) GD.PrintErr("Lever: No gates assigned.");

			Node main = GetTree().Root.GetNodeOrNull("Main");
			_dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");

			if (_dungeon == null)
			{
				GD.PrintErr("Lever: Dungeon reference not found.");
				return;
			}

			_dungeon.AddObject(this);
			InitializeState();
		}

		#endregion

		#region Lever Logic

		/// <summary>
		/// Flips the lever's state and toggles all connected gates.
		/// Includes a short delay and sound feedback.
		/// </summary>
		public async void ToggleLever()
		{
			if (!_useBlock)
			{
				if (_handle == null || _sfxPlayer == null || _gates.Count == 0)
					return;

				_useBlock = true;

				_leverOn = !_leverOn;
				_handle.Position = _leverOn ? HandleDownPosition : HandleUpPosition;
				_sfxPlayer.Play();

				await ToSignal(GetTree().CreateTimer(LeverDelaySeconds), SceneTreeTimer.SignalName.Timeout);

				foreach (Gate gate in _gates)
				{
					if (gate._gateOpen)
						gate.CloseGate();
					else
						gate.OpenGate();
				}

				await ToSignal(GetTree().CreateTimer(_cooldown), SceneTreeTimer.SignalName.Timeout);

				_useBlock = false;
			}
		}

		#endregion

		#region Initialization

		/// <summary>
		/// Loads the lever's state from the save system and updates handle position.
		/// </summary>
		private void InitializeState()
		{
			if (_dungeon == null || _handle == null)
				return;

			_leverOn = _dungeon.LoadObjectState("Lever", LeverId, "On");
			_handle.Position = _leverOn ? HandleDownPosition : HandleUpPosition;
		}

		#endregion
	}
}
