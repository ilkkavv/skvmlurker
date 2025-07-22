using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Represents a wall with graffiti that can be read by the player.
	/// </summary>
	public partial class GraffitiWall : StaticBody3D
	{
		#region Exported Properties

		[Export] private string _text = "";

		#endregion

		#region Public API

		/// <summary>
		/// Displays the graffiti text to the player.
		/// </summary>
		public void Read()
		{
			Global.MessageBox.Message($"The stone beareth words of old, saying thus: {_text}");
		}

		#endregion
	}
}
