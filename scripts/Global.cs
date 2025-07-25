using Godot;

namespace DungeonCrawler
{
	public partial class Global : Node
	{
		public static Global Instance;

		public static Dungeon Dungeon;
		public static MessageBox MessageBox;
		public static Skull Skull;

		public override void _Ready()
		{
			Instance = this;

			Node main = GetTree().Root.GetNodeOrNull("Main");
			Dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");
			MessageBox = main?.GetNodeOrNull<MessageBox>("CanvasLayer/MessageBox");
			Skull = main?.GetNodeOrNull<Skull>("CanvasLayer/Skull");

			if (Dungeon == null) GD.PrintErr("Global: Dungeon not found.");
			if (MessageBox == null) GD.PrintErr("Global: MessageBox not found.");
			if (Skull == null) GD.PrintErr("Global: Skull not found.");
		}
	}
}
