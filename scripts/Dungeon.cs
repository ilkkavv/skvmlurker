using Godot;
using System.Threading.Tasks;

namespace dungeonCrawler
{
	public partial class Dungeon : Node3D
	{
		#region Fields and Configuration

		[Export(PropertyHint.File, "*.tscn")]
		private string _startLevelPath;

		[Export] private float _fadeTime = 0.5f;

		private Player _player;
		private ScreenFader _screenFader;
		private Node3D _currentLevel;

		#endregion

		#region Godot Lifecycle

		/// <summary>
		/// Called when the node enters the scene tree. Initializes references and loads the starting level.
		/// </summary>
		public override void _Ready()
		{
			// Resolve required references from the scene
			var main = GetTree().Root.GetNodeOrNull("Main");
			if (main == null)
			{
				GD.PrintErr("Dungeon: 'Main' node not found.");
				return;
			}

			_player = main.GetNodeOrNull<Player>("GameWorld/Player");
			if (_player == null)
			{
				GD.PrintErr("Dungeon: Failed to find Player node at 'GameWorld/Player'.");
				return;
			}

			_screenFader = main.GetNodeOrNull<ScreenFader>("UI/ScreenFader");
			if (_screenFader == null)
			{
				GD.PrintErr("Dungeon: Failed to find ScreenFader node at 'UI/ScreenFader'.");
				return;
			}

			// Load initial level
			if (string.IsNullOrEmpty(_startLevelPath))
			{
				GD.PrintErr("Dungeon: No start level path specified.");
				return;
			}

			PackedScene startScene = ResourceLoader.Load<PackedScene>(_startLevelPath);
			if (startScene == null)
			{
				GD.PrintErr($"Dungeon: Failed to load scene from path: {_startLevelPath}");
				return;
			}

			_ = ChangeLevel(startScene, Vector3.Zero, 0f); // Fire and forget
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Sets the player's global position and Y rotation in degrees.
		/// </summary>
		public void SetPlayerPos(Vector3 position, float rotation)
		{
			_player.GlobalPosition = position;
			_player.GlobalRotationDegrees = new Vector3(0, rotation, 0);
		}

		/// <summary>
		/// Changes the current level to the given scene and repositions the player.
		/// Handles fade out/in transitions and optional fall damage.
		/// </summary>
		/// <param name="targetScene">Scene to load as the new level.</param>
		/// <param name="newPlayerPos">Where to place the player.</param>
		/// <param name="newPlayerRot">Optional new Y rotation.</param>
		/// <param name="fallDamage">Whether fall damage should be triggered.</param>
		public async Task ChangeLevel(PackedScene targetScene, Vector3 newPlayerPos, float? newPlayerRot = null, bool fallDamage = false)
		{
			if (targetScene == null)
			{
				GD.PrintErr("Dungeon: Target scene is null.");
				return;
			}

			_player.BlockInput();

			// Fade to black and wait
			_screenFader.FadeToBlack(_fadeTime);
			await ToSignal(GetTree().CreateTimer(_fadeTime), SceneTreeTimer.SignalName.Timeout);

			// Remove existing level if any
			Node oldLevel = GetNodeOrNull<Node>("Level");
			if (oldLevel != null)
			{
				RemoveChild(oldLevel);
				oldLevel.QueueFree();
			}

			// Instantiate and add new level
			Node newLevelInstance = targetScene.Instantiate<Node>();
			newLevelInstance.Name = "Level";
			AddChild(newLevelInstance);
			_currentLevel = newLevelInstance as Node3D;

			// Position and rotate player
			float finalRot = newPlayerRot ?? _player.GlobalRotationDegrees.Y;
			SetPlayerPos(newPlayerPos, finalRot);

			// Fade back in and wait
			_screenFader.FadeBack(_fadeTime);
			await ToSignal(GetTree().CreateTimer(_fadeTime), SceneTreeTimer.SignalName.Timeout);

			// Re-enable player input
			_player.UnblockInput();

			if (fallDamage)
			{
				GD.Print("Player takes fall damage! (not yet implemented)");
				// TODO: Apply actual fall damage here
			}
		}

		#endregion
	}
}
