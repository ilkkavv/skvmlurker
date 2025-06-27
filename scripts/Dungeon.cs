using Godot;

namespace dungeonCrawler
{
	public partial class Dungeon : Node3D
	{
		[Export(PropertyHint.File, "*.tscn")]
		private string _dlvl1Path;
		[Export] private PlayerController _playerCharacter;
		[Export] private ScreenFader _screenFader;
		[Export] private float _fadeTime = 0.5f;

		private Node3D _currentDlvl;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			if (string.IsNullOrEmpty(_dlvl1Path))
			{
				GD.PrintErr("Level path is empty!");
				return;
			}

			PackedScene startScene = ResourceLoader.Load<PackedScene>(_dlvl1Path);
			if (startScene == null)
			{
				GD.PrintErr($"Could not load scene from path: {_dlvl1Path}");
				return;
			}

			ChangeLevel(startScene, new Vector3(2, 0, 4), 0);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		private void SetPlayerPos(Vector3 position, float rotation)
		{
			_playerCharacter.GlobalPosition = position;
			_playerCharacter.GlobalRotationDegrees = new Vector3(0, rotation, 0);
		}

		public void ChangeLevel(PackedScene targetLevel, Vector3 newPlayerPos, float newPlayerRot)
		{
			_playerCharacter._blockInput = true;

			if (_screenFader == null)
			{
				GD.PrintErr("Screen Fader is null!");
				return;
			}

			// Fade to black and chain logic after completion
			_screenFader.FadeToBlack(_fadeTime);

			var tween = GetTree().CreateTween();
			tween.TweenInterval(_fadeTime); // Delay to match fade duration
			tween.TweenCallback(Callable.From(() =>
			{
				// Remove current dungeon level
				var currentDlvl = GetNodeOrNull<Node>("Level");
				if (currentDlvl != null)
				{
					RemoveChild(currentDlvl);
					currentDlvl.Free();
				}

				if (targetLevel == null)
				{
					GD.PrintErr("Target scene is null!");
					return;
				}

				// Instance and add new dungeon level
				var newLevelInstance = targetLevel.Instantiate<Node>();
				newLevelInstance.Name = "Level";
				AddChild(newLevelInstance);

				// Set player position and rotation
				SetPlayerPos(newPlayerPos, newPlayerRot);

				// Fade back in
				_screenFader.FadeBack(_fadeTime);

				_playerCharacter._blockInput = false;
			}
			));
		}
	}
}
