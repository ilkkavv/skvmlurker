/*
TODO: null checks, isActive check
*/

namespace dungeonCrawler
{
	public partial class LeverHandle : Interactable
	{
		private Lever _lever;

		public override void _Ready()
		{
			_lever = GetParent<Lever>();
		}

		public override void _Process(double delta)
		{
		}

		public override void OnInteract()
		{
			_lever.ToggleLever();
		}
	}
}
