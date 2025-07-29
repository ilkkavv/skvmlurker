using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Visual representation of the player's health using an animated skull sprite.
	/// Updates animations based on damage levels and plays hurt effects dynamically.
	/// </summary>
	public partial class Skull : AnimatedSprite2D
	{
		#region Fields

		private string _defaultAnim;

		#endregion

		#region Lifecycle

		public override void _Ready()
		{
			// Connect animation finished signal
			AnimationFinished += OnAnimationFinished;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Updates the skull animation based on current health percentage.
		/// </summary>
		/// <param name="maxHp">Maximum health value.</param>
		/// <param name="hp">Current health value.</param>
		public void UpdateSkull(int maxHp, int hp)
		{
			float percent = (float)hp / maxHp;

			// Determine damage animation based on health %
			string anim = percent switch
			{
				> 0.8f => "damage0",
				> 0.6f => "damage1",
				> 0.4f => "damage2",
				> 0.2f => "damage3",
				> 0.0f => "damage4",
				_ => "dead"
			};

			_defaultAnim = anim;
			Animation = anim;
			Play();
		}

		/// <summary>
		/// Temporarily plays a hurt animation overlay, if available.
		/// </summary>
		/// <param name="hp">Current health to determine if hurt animation should play.</param>
		public void PlayHurt(int hp)
		{
			if (hp <= 0) return;

			string hurtAnim = $"{_defaultAnim}-hurt";

			if (SpriteFrames.HasAnimation(hurtAnim))
			{
				Animation = hurtAnim;
				Play();
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Resets to the default animation when a temporary animation finishes.
		/// </summary>
		private void OnAnimationFinished()
		{
			Animation = _defaultAnim;
			Play();
		}

		#endregion
	}
}
