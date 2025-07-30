using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Represents a wall with graffiti that can be read by the player.
	/// </summary>
	public partial class GraffitiWall : StaticBody3D
	{
		#region Exported Properties

		[Export] private string _narration = "Inscribed on the wall are old words that read: ";
		[Export] private string _message = "";

		#endregion

		#region Public API

		/// <summary>
		/// Displays the graffiti text to the player.
		/// </summary>
		public void Read()
		{
			Global.MessageBox.Message((_narration + _message), Global.Blue);
		}

		#endregion
	}
}
