using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Flashes a colored overlay on the screen for a brief duration.
	/// Useful for effects like damage feedback or teleportation.
	/// </summary>
	public partial class ScreenFlasher : ColorRect
	{
		private Tween _currentTween;

		/// <summary>
		/// Flashes a color over the screen and fades it out automatically.
		/// </summary>
		/// <param name="flashColor">The color to flash.</param>
		/// <param name="flashDuration">Total duration of the flash effect.</param>
		public void Flash(Color flashColor, float flashDuration = 0.2f)
		{
			Visible = true;
			_currentTween?.Kill();

			Color = flashColor;
			Modulate = new Color(1, 1, 1, 0f); // No tint, just color transparency

			_currentTween = GetTree().CreateTween();

			// Fade in
			_currentTween
				.TweenProperty(this, "modulate:a", 1f, flashDuration * 0.5f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.Out);

			// Fade out
			_currentTween
				.TweenProperty(this, "modulate:a", 0f, flashDuration * 0.2f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.In);

			_currentTween.Finished += () => Visible = false;
		}
	}
}
