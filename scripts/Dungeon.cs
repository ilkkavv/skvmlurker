using System;
using Godot;

namespace dungeonCrawler
{
	public partial class Dungeon : Node3D
	{
		[Export(PropertyHint.File, "*.tscn")]
		private string _dlvl1Path;
		[Export] private Player _player;
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

			ChangeLevel(startScene, new Vector3(0, 0, 0), 0);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		public void SetPlayerPos(Vector3 position, float rotation)
		{
			_player.GlobalPosition = position;
			_player.GlobalRotationDegrees = new Vector3(0, rotation, 0);
		}

		public void ChangeLevel(PackedScene targetLevel, Vector3 newPlayerPos,
						float? newPlayerRot = null, bool fallDamage = false)
		{
			_player._blockInput = true;

			if (_screenFader == null)
			{
				GD.PrintErr("Screen Fader is null!");
				return;
			}

			// Start fade to black
			_screenFader.FadeToBlack(_fadeTime);

			// Create tween for timing
			var tween = GetTree().CreateTween();

			// Step 1: Wait for fade out
			tween.TweenInterval(_fadeTime);

			// Step 2: Load new level
			tween.TweenCallback(Callable.From(() =>
			{
				// Remove current level
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

				// Add new level
				var newLevelInstance = targetLevel.Instantiate<Node>();
				newLevelInstance.Name = "Level";
				AddChild(newLevelInstance);

				// Move player (with or without new rotation)
				float finalRot = newPlayerRot ?? _player.GlobalRotationDegrees.Y;
				SetPlayerPos(newPlayerPos, finalRot);

				// Start fade back in
				_screenFader.FadeBack(_fadeTime);
			}));

			// Step 3: Wait for fade back in
			tween.TweenInterval(_fadeTime);

			// Step 4: Re-enable player input
			tween.TweenCallback(Callable.From(() =>
			{
				_player._blockInput = false;

				if (fallDamage)
				{
					GD.Print("Player takes fall damage!");
					// TODO: Apply fall damage here
				}
			}));
		}

		internal void ChangeLevel(string targetScenePath, Vector3 newPlayerPos, float v)
		{
			throw new NotImplementedException();
		}
	}
}
