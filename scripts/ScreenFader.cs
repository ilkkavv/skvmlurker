using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Handles screen fading transitions (e.g., fade to black and back).
	/// </summary>
	public partial class ScreenFader : ColorRect
	{
		private Tween _currentTween;

		/// <summary>
		/// Fades the screen to black over the given duration.
		/// </summary>
		/// <param name="duration">Duration of fade in seconds.</param>
		public void FadeToBlack(float duration = 0.5f)
		{
			Visible = true;
			Color = new Color(0, 0, 0); // Set background color to black

			_currentTween?.Kill();

			_currentTween = GetTree().CreateTween();
			_currentTween.TweenProperty(this, "modulate:a", 1f, duration);
		}

		/// <summary>
		/// Fades the screen back to transparent over the given duration.
		/// </summary>
		/// <param name="duration">Duration of fade out in seconds.</param>
		public void FadeBack(float duration = 0.5f)
		{
			_currentTween?.Kill();

			_currentTween = GetTree().CreateTween();
			_currentTween.TweenProperty(this, "modulate:a", 0f, duration);
			_currentTween.Finished += () => Visible = false;
		}
	}
}
