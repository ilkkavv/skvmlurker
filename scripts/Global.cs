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
		public static MessageBox MessageBox;
		public static Skull Skull;

		#endregion

		#region Lifecycle

		public override void _Ready()
		{
			Instance = this;

			// Attempt to locate key nodes in the scene tree
			Node main = GetTree().Root.GetNodeOrNull("Main");
			Dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");
			MessageBox = main?.GetNodeOrNull<MessageBox>("CanvasLayer/MessageBox");
			Skull = main?.GetNodeOrNull<Skull>("CanvasLayer/Skull");

			// Error handling for missing references
			if (Dungeon == null) GD.PrintErr("Global: Dungeon not found.");
			if (MessageBox == null) GD.PrintErr("Global: MessageBox not found.");
			if (Skull == null) GD.PrintErr("Global: Skull not found.");
		}

		#endregion
	}
}
