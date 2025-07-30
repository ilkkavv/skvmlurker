using System.Collections.Generic;
using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Displays color-coded messages in a RichTextLabel.
	/// Maintains a capped log of recent messages.
	/// </summary>
	public partial class MessageBox : RichTextLabel
	{
		#region Constants & Fields

		private const int MaxMessages = 100;
		private readonly List<string> _messageLog = new();

		#endregion

		#region Lifecycle

		public override void _Ready()
		{
			ScrollActive = false;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds a formatted message to the message log with optional color tag.
		/// </summary>
		/// <param name="message">The message text to display.</param>
		/// <param name="color">Color tag for the message ("grey", "red", "green", or default).</param>
		public void Message(string message, Color color)
		{
			string formatted = $"[color={color.ToHtml()}]• {message}[/color]";

			_messageLog.Add(formatted);

			if (_messageLog.Count > MaxMessages)
				_messageLog.RemoveAt(0);

			RebuildLog();
		}

		/// <summary>
		/// Clears the entire message log and UI text.
		/// </summary>
		public void ClearMessages()
		{
			_messageLog.Clear();
			Clear();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Rebuilds the RichTextLabel content from the message log.
		/// </summary>
		private void RebuildLog()
		{
			Text = string.Join("\n", _messageLog);
			ScrollToLine(GetLineCount());
		}

		#endregion
	}
}
