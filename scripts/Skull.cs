using Godot;

namespace DungeonCrawler
{
	public partial class Skull : AnimatedSprite2D
	{
		private string _defaultAnim;

		public override void _Ready()
		{
			AnimationFinished += OnAnimationFinished;
		}

		public void UpdateSkull(int maxHp, int hp)
		{
			float percent = (float)hp / maxHp;

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

		private void OnAnimationFinished()
		{
			Animation = _defaultAnim;
			Play();
		}
	}
}
