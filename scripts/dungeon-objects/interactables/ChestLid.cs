using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Interactable component for a chest's lid.
	/// Relays interaction calls to its parent <see cref="Chest"/>.
	/// </summary>
	public partial class ChestLid : Interactable
	{
		private Chest _chest;

		#region Lifecycle

		public override void _Ready()
		{
			_chest = GetParentOrNull<Chest>();
			if (_chest == null) GD.PrintErr("ChestLid: Could not find parent Chest.");
		}

		#endregion

		#region Interaction

		/// <summary>
		/// Called when the player interacts with the chest lid.
		/// Loots the chest.
		/// </summary>
		public override void OnInteract()
		{
			_chest?.LootChest();
		}

		#endregion
	}
}
