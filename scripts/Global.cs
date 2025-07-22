using Godot;

namespace DungeonCrawler
{
	public partial class Global : Node
	{
		public static Global Instance;

		public static Dungeon Dungeon;
		public static MessageBox MessageBox;

		public override void _Ready()
		{
			Instance = this;

			Node main = GetTree().Root.GetNodeOrNull("Main");
			Dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");
			MessageBox = main?.GetNodeOrNull<MessageBox>("CanvasLayer/MessageBox");

			if (Dungeon == null) GD.PrintErr("Global: Dungeon not found.");
			if (MessageBox == null) GD.PrintErr("Global: MessageBox not found.");
		}
	}
}
