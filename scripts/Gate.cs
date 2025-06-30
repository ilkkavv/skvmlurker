using Godot;

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

		private bool _isActive = true; // Track if the node is still alive

		public async void OpenGate(float timer = 0f)
		{
			if (_gateOpen || !_isActive)
				return;

			_gateOpen = true;

			float targetY = _openHeight;
			float currentY = _fence.Position.Y;

			while (currentY < targetY && _gateOpen && _isActive)
			{
				currentY = Mathf.Min(currentY + _stepHeight, targetY);

				if (_isActive)
					_fence.Position = new Vector3(0, currentY, 0);

				if (_isActive)
				{
					_sfxPlayer.PitchScale = 1;
					_sfxPlayer.Play();
				}

				await ToSignal(GetTree().CreateTimer(_openDelay), "timeout");
			}

			if (_gateOpen && _isActive)
			{
				_fence.Position = new Vector3(0, _openHeight, 0);
			}

			if (timer > 0)
			{
				await ToSignal(GetTree().CreateTimer(timer), "timeout");
				CloseGate();
			}
		}

		public async void CloseGate()
		{
			if (!_gateOpen || !_isActive)
				return;

			_gateOpen = false;

			float targetY = 0f;
			float currentY = _fence.Position.Y;

			while (currentY > targetY && !_gateOpen && _isActive)
			{
				currentY = Mathf.Max(currentY - _stepHeight, targetY);

				if (_isActive)
					_fence.Position = new Vector3(0, currentY, 0);

				if (_isActive)
				{
					_sfxPlayer.PitchScale = 0.8f;
					_sfxPlayer.Play();
				}

				await ToSignal(GetTree().CreateTimer(_closeDelay), "timeout");
			}

			if (!_gateOpen && _isActive)
			{
				_fence.Position = new Vector3(0, 0, 0);
			}
		}

		public override void _Ready()
		{
			if (_gateOpen && _fence != null)
			{
				_fence.Position = new Vector3(0, _openHeight, 0);
			}
		}

		public override void _ExitTree()
		{
			_isActive = false;
		}

		public override void _Process(double delta)
		{
		}
	}
}
