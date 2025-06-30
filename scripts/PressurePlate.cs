/*
TODO: null checks, isActive check
*/

using Godot;
using System;

namespace dungeonCrawler
{
	public partial class PressurePlate : Node3D
	{
		[Export] private Gate _gate;
		[Export] private StaticBody3D _plate;
		[Export] private RayCast3D _raycast;

		[Export] private AudioStreamPlayer3D _sfxPlayer;
		[Export] private string _pathToPress;
		[Export] private string _pathToRelease;

		[Export] private float _gateTimer = 0;

		private bool _platePressed = false;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			CheckForPlayer();
		}

		private void CheckForPlayer()
		{
			if (_raycast.IsColliding() && !_platePressed)
			{
				var collider = _raycast.GetCollider();

				if (collider is Node colliderNode)
				{
					if (colliderNode.IsInGroup("player"))
					{
						ToggleSwitch();
					}
				}
			}
			else if (!_raycast.IsColliding() && _platePressed)
			{
				ToggleSwitch();
			}
		}

		private async void ToggleSwitch()
		{
			if (!_platePressed)
			{
				_platePressed = true;

				_sfxPlayer.Stream = GD.Load<AudioStream>(_pathToPress);
				_sfxPlayer.Play();

				_plate.Position = new Vector3(0, (float)-0.05, 0);

				await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
				_gate.OpenGate(_gateTimer);
			}
			else
			{
				_platePressed = false; ;

				_sfxPlayer.Stream = GD.Load<AudioStream>(_pathToRelease);
				_sfxPlayer.Play();

				_plate.Position = new Vector3(0, 0, 0);
			}
		}
	}
}
