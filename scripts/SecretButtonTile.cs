/*
TODO: null checks, isActive check
*/

namespace dungeonCrawler
{
	public partial class SecretButtonTile : Interactable
	{
		private SecretButton _secretButton;

		public override void _Ready()
		{
			_secretButton = GetParent<SecretButton>();
		}

		public override void _Process(double delta)
		{
		}

		public override void OnInteract()
		{
			_secretButton.Activate();
		}
	}
}
