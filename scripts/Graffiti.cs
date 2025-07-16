using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Interactable component representing graffiti on a wall.
	/// Triggers the parent wall's read action when interacted with.
	/// </summary>
	public partial class Graffiti : Interactable
	{
		#region Fields

		private GraffitiWall _wall;

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the node enters the scene tree.
		/// Locates the parent GraffitiWall.
		/// </summary>
		public override void _Ready()
		{
			_wall = GetParentOrNull<GraffitiWall>();

			if (_wall == null)
				GD.PrintErr("Graffiti: Could not find parent Wall.");
		}

		#endregion

		#region Interaction

		/// <summary>
		/// Called when the player interacts with the graffiti.
		/// Delegates the action to the wall.
		/// </summary>
		public override void OnInteract()
		{
			_wall?.Read();
		}

		#endregion
	}
}
