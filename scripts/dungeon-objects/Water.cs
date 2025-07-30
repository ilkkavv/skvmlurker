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

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the node is added to the scene. Initializes references to child nodes and other game systems.
		/// </summary>
		public override void _Ready()
		{
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			if (_triggerArea == null)
				GD.PrintErr("Water: Missing TriggerArea node.");
			if (_sfxPlayer == null)
				GD.PrintErr("Water: Missing SFXPlayer node.");
			else
				_triggerArea.BodyEntered += OnBodyEntered;
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

			Global.Player = body.GetParentOrNull<Player>();
			if (Global.Player == null)
			{
				GD.PrintErr("Water: Could not resolve Player node.");
				return;
			}

			Global.ScreenFlasher?.Flash(Global.DarkGreen);

			Global.Player.Die(drown: true);
			_sfxPlayer.Play();
		}

		#endregion
	}
}
