using Godot;

namespace dungeonCrawler
{
	/// <summary>
	/// A clickable tile that triggers its parent SecretButton when interacted with.
	/// </summary>
	public partial class SecretButtonTile : Interactable
	{
		private SecretButton _secretButton;

		#region Lifecycle

		public override void _Ready()
		{
			_secretButton = GetParentOrNull<SecretButton>();

			if (_secretButton == null)
				GD.PrintErr("SecretButtonTile: Parent SecretButton not found.");
		}

		#endregion

		#region Interaction

		/// <summary>
		/// Called when the player interacts with the tile.
		/// Triggers the parent SecretButton if valid.
		/// </summary>
		public override void OnInteract()
		{
			_secretButton?.Activate();
		}

		#endregion
	}
}
