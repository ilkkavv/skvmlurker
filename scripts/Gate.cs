using Godot;
using System;

namespace dungeonCrawler
{
	public partial class Gate : Node3D
	{
		[Export] private StaticBody3D _fence;
		[Export] private AudioStreamPlayer3D _sfxPlayer;

		[Export] private bool _gateOpen = false;
		[Export] private float _openHeight = 1.4f;
		[Export] private float _stepHeight = 0.2f;
		[Export] private float _openDelay = 0.2f;
		[Export] private float _closeDelay = 0.1f;

		public async void OpenGate()
		{
			if (_gateOpen)
				return;

			_gateOpen = true;

			float targetY = _openHeight;
			float currentY = _fence.Position.Y;

			while (currentY < targetY)
			{
				currentY = Mathf.Min(currentY + _stepHeight, targetY);
				_fence.Position = new Vector3(0, currentY, 0);

				_sfxPlayer.PitchScale = 1;
				_sfxPlayer.Play();

				await ToSignal(GetTree().CreateTimer(_openDelay), "timeout");
			}

			_fence.Position = new Vector3(0, _openHeight, 0);
		}

		public async void CloseGate()
		{
			if (!_gateOpen)
				return;

			_gateOpen = false;

			float targetY = 0f;
			float currentY = _fence.Position.Y;

			while (currentY > targetY)
			{
				currentY = Mathf.Max(currentY - _stepHeight, targetY);
				_fence.Position = new Vector3(0, currentY, 0);

				_sfxPlayer.PitchScale = 0.8f;
				_sfxPlayer.Play();

				await ToSignal(GetTree().CreateTimer(_closeDelay), "timeout");
			}

			_fence.Position = new Vector3(0, 0, 0);
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			if (_gateOpen)
			{
				_fence.Position = new Vector3(0, _openHeight, 0);
			}
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (Input.IsActionJustPressed("Test"))
			{
				OpenGate();
			}

			if (Input.IsActionJustPressed("Close"))
			{
				CloseGate();
			}
		}
	}
}
