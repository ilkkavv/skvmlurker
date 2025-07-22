using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Handles water interaction logic in the game world, including player drowning,
	/// visual feedback, and sound effects when the player enters the water area.
	/// </summary>
	public partial class Water : Node3D
	{
		#region Private Fields

		private Area3D _triggerArea;
		private AudioStreamPlayer3D _sfxPlayer;
		private Dungeon _dungeon;
		private ScreenFlasher _screenFlasher;
		private Player _player;

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the node is added to the scene. Initializes references to child nodes and other game systems.
		/// </summary>
		public override void _Ready()
		{
			// Local node references
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			if (_triggerArea == null)
				GD.PrintErr("Water: Missing TriggerArea node.");
			if (_sfxPlayer == null)
				GD.PrintErr("Water: Missing SFXPlayer node.");
			else
				_triggerArea.BodyEntered += OnBodyEntered;

			// External references
			Node main = GetTree().Root.GetNodeOrNull("Main");
			_dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");
			_screenFlasher = main?.GetNodeOrNull<ScreenFlasher>("CanvasLayer/ScreenFlasher");

			if (_dungeon == null)
				GD.PrintErr("Water: Dungeon not found.");
			if (_screenFlasher == null)
				GD.PrintErr("Water: ScreenFlasher not found.");
		}

		#endregion

		#region Trigger Logic

		/// <summary>
		/// Called when a body enters the water trigger area. Handles drowning effects if the player enters.
		/// </summary>
		/// <param name="body">The node that entered the trigger area.</param>
		private void OnBodyEntered(Node3D body)
		{
			if (body == null || !body.IsInGroup("player"))
				return;

			GD.Print("Drown!");

			_player = body.GetParentOrNull<Player>();
			if (_player == null)
			{
				GD.PrintErr("Water: Could not resolve Player node.");
				return;
			}

			// Flash screen with dark green tone to simulate drowning
			_screenFlasher?.Flash(new Color(0.0588f, 0.1059f, 0.0745f, 1.0f));

			_player.Die(drown: true);
			_sfxPlayer.Play();
		}

		#endregion
	}
}
