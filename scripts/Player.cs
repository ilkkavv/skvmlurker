using Godot;

namespace dungeonCrawler
{
	public partial class Player : Node3D
	{
		public bool _blockInput = true;

		private PlayerController _playerController;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			_playerController = GetNode<PlayerController>("PlayerController");
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		public void StopPlayer()
		{
			_playerController.tween.Kill();
		}
	}
}
