using Godot;

namespace dungeonCrawler
{
	public partial class ScreenFader : ColorRect
	{
		private Tween _currentTween;

		public void FadeToBlack(float duration = 0.5f)
		{
			Visible = true;

			// Set the color to black before fading
			Color = new Color(0, 0, 0); // RGB black

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

		public void Flash(Color flashColor, float flashDuration = 0.2f)
		{
			Visible = true;
			_currentTween?.Kill();

			// Set the actual ColorRect color
			Color = flashColor;

			// Start fully transparent (but with color)
			Modulate = new Color(1, 1, 1, 0f); // No tinting, just use ColorRect's color

			_currentTween = GetTree().CreateTween();

			_currentTween
				.TweenProperty(this, "modulate:a", 1f, flashDuration * 0.5f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.Out);

			_currentTween
				.TweenProperty(this, "modulate:a", 0f, flashDuration * 0.5f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.In);

			_currentTween.Finished += () => Visible = false;
		}
	}
}
