using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Singleton for globally accessible references to key game nodes.
	/// Automatically assigns references at runtime and provides static access across the project.
	/// </summary>
	public partial class Global : Node
	{
		#region Static Instance & References

		public static Global Instance;

		public static Dungeon Dungeon;
		public static Player Player;
		public static MessageBox MessageBox;
		public static Skull Skull;
		public static ScreenFader ScreenFader;
		public static ScreenFader SkullFader;
		public static ScreenFlasher ScreenFlasher;
		public static SaveManager SaveManager;
		public static WorldEnvironment WorldEnvironment;

		#endregion

		#region Exported Colors

		public static Color Grey = new Color("#8b8b8b");
		public static Color Red = new Color("#7f0000");
		public static Color Green = new Color("#6b6b0f");
		public static Color DarkGreen = new Color("#0f1b13");
		public static Color Blue = new Color("#8b8bcb");
		public static Color Yellow = new Color("#fff31b");

		#endregion

		#region Lifecycle

		public override void _Ready()
		{
			Instance = this;

			Node main = GetTree().Root.GetNodeOrNull("Main");
			Dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");
			Player = main?.GetNodeOrNull<Player>("CanvasLayer/SubViewportContainer/SubViewport/Player");
			MessageBox = main?.GetNodeOrNull<MessageBox>("CanvasLayer/Gui/MessageBox");
			Skull = main?.GetNodeOrNull<Skull>("CanvasLayer/Gui/Skull");
			ScreenFader = main?.GetNodeOrNull<ScreenFader>("CanvasLayer/Gui/ScreenFader");
			SkullFader = main?.GetNodeOrNull<ScreenFader>("CanvasLayer/Gui/SkullFader");
			ScreenFlasher = main?.GetNodeOrNull<ScreenFlasher>("CanvasLayer/Gui/ScreenFlasher");
			SaveManager = main?.GetNodeOrNull<SaveManager>("SaveManager");
			WorldEnvironment = main?.GetNodeOrNull<WorldEnvironment>("GameWorld/WorldEnvironment");

			if (Dungeon == null) GD.PrintErr("Global: Dungeon not found.");
			if (Player == null) GD.PrintErr("Global: Player not found.");
			if (MessageBox == null) GD.PrintErr("Global: MessageBox not found.");
			if (Skull == null) GD.PrintErr("Global: Skull not found.");
			if (ScreenFader == null) GD.PrintErr("Global: ScreenFader not found.");
			if (SkullFader == null) GD.PrintErr("Global: SkullFader not found.");
			if (ScreenFlasher == null) GD.PrintErr("Global: ScreenFlasher not found.");
			if (SaveManager == null) GD.PrintErr("Global: SaveManager not found.");
			if (WorldEnvironment == null) GD.PrintErr("Global: WorldEnvironment not found.");
		}

		#endregion
	}
}
