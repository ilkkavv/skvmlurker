using System.Collections.Generic;
using Godot;

namespace DungeonCrawler
{
	public partial class MessageBox : RichTextLabel
	{
		[Export] private Color _grey = new Color("#8b8b8b");
		[Export] private Color _red = new Color("#7f0000");
		[Export] private Color _green = new Color("#6b6b0f");

		private const int MaxMessages = 100;
		private readonly List<string> _messageLog = new();

		public override void _Ready()
		{
			ScrollActive = false;
		}

		public void Message(string message, string color = "white")
		{
			string formatted = $"[color={GetColor(color).ToHtml()}]• {message}[/color]";

			_messageLog.Add(formatted);

			if (_messageLog.Count > MaxMessages)
				_messageLog.RemoveAt(0);

			RebuildLog();
		}

		public void ClearMessages()
		{
			_messageLog.Clear();
			Clear();
		}

		private void RebuildLog()
		{
			Text = string.Join("\n", _messageLog);
			ScrollToLine(GetLineCount());
		}

		private Color GetColor(string colorName)
		{
			switch (colorName.ToLower())
			{
				case "grey":
					return _grey;
				case "red":
					return _red;
				case "green":
					return _green;
				default:
					return _grey;
			}
		}
	}
}
