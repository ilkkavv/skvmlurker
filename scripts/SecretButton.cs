/*
TODO: null checks, isActive check
*/

using Godot;

namespace dungeonCrawler
{
	public partial class SecretButton : Node3D
	{
		[Export] private Gate _gate;
		[Export] private StaticBody3D _tile;
		[Export] private AudioStreamPlayer3D _sfxPlayer;

		private bool _pressed = false;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		public async void Activate()
		{
			if (!_pressed)
			{
				_pressed = true;
				_tile.Position = new Vector3(0, 0, -0.1f);
				_sfxPlayer.Play();
				await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
				_gate.OpenGate();
			}
		}
	}
}
