using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Base class for interactable objects in the dungeon.
	/// Inherit from this and implement <see cref="OnInteract"/> to define custom interaction logic.
	/// </summary>
	public abstract partial class Interactable : StaticBody3D
	{
		#region Interaction

		/// <summary>
		/// Called when the player interacts with this object.
		/// Must be implemented by derived classes to define custom behavior.
		/// </summary>
		public abstract void OnInteract();

		#endregion
	}
}
