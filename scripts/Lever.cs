using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// A lever that can toggle multiple connected gates.
	/// When activated, it animates a handle, plays a sound effect,
	/// and opens or closes the gates it's connected to.
	/// </summary>
	public partial class Lever : Node3D
	{
		#region Exported Settings

		[Export] private Gate[] _gatesArray; // Assign in the editor

		#endregion

		#region Constants

		private static readonly Vector3 HandleDownPosition = new(0, -0.5f, 0);
		private static readonly Vector3 HandleUpPosition = Vector3.Zero;
		private const float LeverDelaySeconds = 0.3f;

		#endregion

		#region Private Fields

		private StaticBody3D _handle;
		private AudioStreamPlayer3D _sfxPlayer;
		private bool _leverOn = false;
		private List<Gate> _gates = new();

		#endregion

		#region Lifecycle

		public override void _Ready()
		{
			// Get internal references
			_handle = GetNodeOrNull<StaticBody3D>("Handle");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			// Gather non-null gates from array
			if (_gatesArray != null)
				_gates = [.. _gatesArray.Where(g => g != null)];

			// Warnings
			if (_handle == null) GD.PrintErr("Lever: Handle node not found.");
			if (_sfxPlayer == null) GD.PrintErr("Lever: SFXPlayer node not found.");
			if (_gates.Count == 0) GD.PrintErr("Lever: No gates assigned.");
		}

		#endregion

		#region Lever Logic

		/// <summary>
		/// Toggles the lever's state (on/off), animates the handle,
		/// plays a sound, and toggles all connected gates.
		/// </summary>
		public async void ToggleLever()
		{
			if (_handle == null || _sfxPlayer == null || _gates.Count == 0)
				return;

			// Toggle state and visuals
			_leverOn = !_leverOn;
			_handle.Position = _leverOn ? HandleDownPosition : HandleUpPosition;
			_sfxPlayer.Play();

			// Wait briefly before toggling gates
			await ToSignal(GetTree().CreateTimer(LeverDelaySeconds), SceneTreeTimer.SignalName.Timeout);

			// Toggle all gates
			foreach (Gate gate in _gates)
			{
				if (gate._gateOpen)
					gate.CloseGate();
				else
					gate.OpenGate();
			}
		}

		#endregion
	}
}
