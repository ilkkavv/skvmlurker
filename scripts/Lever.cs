/*
TODO: null checks, isActive check
*/

using Godot;

namespace dungeonCrawler
{
	public partial class Lever : Node3D
	{
		[Export] private Gate _gate;
		[Export] private StaticBody3D _handle;
		[Export] private AudioStreamPlayer3D _sfxPlayer;

		private bool _leverOn = false;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		public async void ToggleLever()
		{
			if (!_leverOn)
			{
				_leverOn = true;
				_handle.Position = new Vector3(0, (float)-0.5, 0);
				_sfxPlayer.Play();
				await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
				_gate.OpenGate();
			}
			else
			{
				_leverOn = false;
				_handle.Position = new Vector3(0, 0, 0);
				_sfxPlayer.Play();
				await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
				_gate.CloseGate();
			}
		}
	}
}
