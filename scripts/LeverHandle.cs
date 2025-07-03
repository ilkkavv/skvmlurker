using Godot;

namespace dungeonCrawler
{
	/// <summary>
	/// Interactable component for a lever's handle.
	/// Relays interaction calls to its parent <see cref="Lever"/>.
	/// </summary>
	public partial class LeverHandle : Interactable
	{
		private Lever _lever;

		#region Lifecycle

		public override void _Ready()
		{
			_lever = GetParentOrNull<Lever>();

			if (_lever == null)
				GD.PrintErr("LeverHandle: Could not find parent Lever.");
		}

		#endregion

		#region Interaction

		/// <summary>
		/// Called when the player interacts with the handle.
		/// Toggles the parent lever if valid.
		/// </summary>
		public override void OnInteract()
		{
			_lever?.ToggleLever();
		}

		#endregion
	}
}
