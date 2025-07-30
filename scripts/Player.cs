using System;
using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Represents the player character in the dungeon.
	/// Handles stats, damage, input control, and death logic.
	/// </summary>
	public partial class Player : Node3D
	{
		#region Player Stats

		private int _str;
		private int _agi;
		private int _con;
		private int _wis;

		public int Hp { private set; get; }

		#endregion

		#region Exported Properties

		[Export] public int MaxHp { private set; get; } = 20;

		[Export] private string _narrationLightWound = "You suffer a light graze.";
		[Export] private string _narrationSeriousWound = "You take a serious wound.";
		[Export] private string _narrationBrutalWound = "Pain sears your flesh — a brutal blow!";
		[Export] private string _narrationDrown = "You drown, your lungs filling with sorrow.";
		[Export] private string _narrationDeath = "Your tale ends here. You have perished in the dungeon.";

		#endregion

		#region Dependencies

		private PlayerController _playerController;

		#endregion

		#region Properties

		/// <summary>
		/// Indicates whether player input is currently blocked.
		/// </summary>
		public bool IsInputBlocked { get; private set; } = false;

		// This feature will be implemented in the future.
		public string KeyId { get; private set; } = "";

		private bool _isDead = false;
		private float _respawnTimer = 3f;

		#endregion

		#region Godot Lifecycle

		/// <summary>
		/// Called when the node enters the scene tree.
		/// Sets up component references from the scene.
		/// </summary>
		public override void _Ready()
		{
			_playerController = GetNodeOrNull<PlayerController>("PlayerController");
			if (_playerController == null)
				GD.PrintErr("Player: PlayerController not found.");
		}

		#endregion

		#region Input Control

		/// <summary>
		/// Disables all player input (used during events or death).
		/// </summary>
		public void BlockInput()
		{
			IsInputBlocked = true;
		}

		/// <summary>
		/// Enables player input, unless the player is dead.
		/// </summary>
		public void UnblockInput()
		{
			if (_isDead)
				return;

			IsInputBlocked = false;
		}

		#endregion

		#region Movement Control

		/// <summary>
		/// Immediately stops any active movement tween.
		/// </summary>
		public void StopPlayer()
		{
			_playerController?.TweenInstance?.Kill();
		}

		#endregion

		#region Combat and Health

		/// <summary>
		/// Deals randomized damage to the player and handles death logic.
		/// </summary>
		/// <param name="diceCount">Number of dice to roll.</param>
		/// <param name="diceType">Type of dice (e.g., 6-sided).</param>
		public void TakeDamage(int diceCount, int diceType)
		{
			Random rnd = new();
			int damage = diceCount * rnd.Next(1, diceType + 1);

			string message = "";
			switch (damage)
			{
				case < 3:
					message = _narrationLightWound;
					break;
				case < 5:
					message = _narrationSeriousWound;
					break;
				case < 7:
					message = _narrationBrutalWound;
					break;
				default:
					break;
			}

			Global.MessageBox.Message($"{message}", Global.Red);

			Hp -= damage;

			Global.ScreenFlasher?.Flash(new Color(1f, 0f, 0f, 1f));
			_playerController?.PlayHurt();

			Global.Skull.UpdateSkull();
			Global.Skull.PlayHurt(Hp);

			if (Hp <= 0)
				Die();
		}

		/// <summary>
		/// Triggers the player's death sequence.
		/// </summary>
		public async void Die(bool drown = false)
		{
			float fadeTime = 0.5f;
			if (drown)
			{
				Global.Skull.PlayDrown();
				fadeTime = 0.25f;
				Global.MessageBox.Message(_narrationDrown, Global.Red);
			}
			else
			{
				Global.MessageBox.Message(_narrationDeath, Global.Red);
			}

			BlockInput();
			_isDead = true;
			Global.ScreenFader?.FadeToBlack(fadeTime);

			await ToSignal(GetTree().CreateTimer(_respawnTimer), SceneTreeTimer.SignalName.Timeout);

			Global.Dungeon.StartNewGame();
		}

		/// <summary>
		/// Restores the player to an active state by resetting health and death status.
		/// </summary>
		public void Spawn()
		{
			Hp = MaxHp;
			KeyId = "";
			_isDead = false;
		}

		#endregion

		#region Inventory Management

		/// <summary>
		/// This feature will be properly implemented in the future.
		/// </summary>
		/// <param name="keyId"></param>
		public void SetKeyId(string keyId)
		{
			KeyId = keyId;
		}

		#endregion
	}
}
