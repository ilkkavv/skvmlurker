using Godot;

namespace dungeonCrawler
{
	public partial class TeleportTrap : Node3D
	{
		[Export] private RayCast3D _raycast;
		[Export] private AudioStreamPlayer2D _sfxPlayer;
		[Export] private Vector3 _newPlayerPos = new Vector3(0, 0, 0);
		[Export] private float _newPlayerRot;
		[Export] private bool _triggerOnce = false;

		private Dungeon _dungeon;
		private ScreenFader _screenFader;
		private bool _triggered = false;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			if (_triggered)
				Visible = false;

			// Get the root node (Main)
			Node main = GetTree().Root.GetNode("Main");
			// Get Dungeon node from the scene
			_dungeon = main.GetNode<Dungeon>("GameWorld/Dungeon");
			// Get Screen Fader node from the scene
			_screenFader = main.GetNode<ScreenFader>("CanvasLayer/ScreenFader");
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			CheckForPlayer();
		}

		private void CheckForPlayer()
		{
			if (_raycast.IsColliding() && !_triggered)
			{
				var collider = _raycast.GetCollider();

				if (collider is Node colliderNode)
				{
					if (colliderNode.IsInGroup("player"))
					{
						Trigger();
					}
				}
			}
		}

		private void Trigger()
		{
			if (_triggerOnce)
			{
				_triggered = true;
			}

			_sfxPlayer.Play();
			_screenFader.Flash(new Color(0.545f, 0.545f, 0.796f, 1f));
			_dungeon.SetPlayerPos(_newPlayerPos, _newPlayerRot);
		}
	}
}
