using Godot;

namespace dungeonCrawler
{
	public partial class ScreenFader : ColorRect
	{
		private Tween _currentTween;

		public void FadeToBlack(float duration = 0.5f)
		{
			Visible = true;

			// Cancel any existing tween
			_currentTween?.Kill();

			_currentTween = GetTree().CreateTween();
			_currentTween.TweenProperty(this, "modulate:a", 1f, duration);
		}

		public void FadeBack(float duration = 0.5f)
		{
			// Cancel any existing tween
			_currentTween?.Kill();

			_currentTween = GetTree().CreateTween();
			_currentTween.TweenProperty(this, "modulate:a", 0f, duration);
			_currentTween.Finished += () => Visible = false;
		}
	}
}
