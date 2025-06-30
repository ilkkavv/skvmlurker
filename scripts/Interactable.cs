using Godot;

namespace dungeonCrawler
{
	public abstract partial class Interactable : StaticBody3D
	{
		public abstract void OnInteract();
	}
}
