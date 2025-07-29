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

		[Export] public int MaxHp { private set; get; } = 20;
		public int Hp { private set; get; }

		#endregion

		#region Dependencies

		private PlayerController _playerController;
		private ScreenFlasher _screenFlasher;
		private ScreenFader _screenFader;

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

			Node main = GetTree().Root.GetNodeOrNull("Main");
			if (main == null)
			{
				GD.PrintErr("Player: 'Main' node not found.");
				return;
			}

			_screenFlasher = main.GetNodeOrNull<ScreenFlasher>("CanvasLayer/ScreenFlasher");
			if (_screenFlasher == null)
				GD.PrintErr("Player: ScreenFlasher not found at 'CanvasLayer/ScreenFlasher'.");

			_screenFader = main.GetNodeOrNull<ScreenFader>("CanvasLayer/ScreenFader");
			if (_screenFader == null)
				GD.PrintErr("Player: ScreenFader not found at 'CanvasLayer/ScreenFader'.");
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
					message = "Thou art grazed by harm most slight.";
					break;
				case < 5:
					message = "A grievous wound dost thou suffer.";
					break;
				case < 7:
					message = "Pain sears thy flesh — a dire blow indeed!";
					break;
				default:
					break;
			}

			Global.MessageBox.Message($"{message}", color: "red");

			Hp -= damage;

			// Visual/audio feedback
			_screenFlasher?.Flash(new Color(1f, 0f, 0f, 1f)); // Red flash
			_playerController?.PlayHurt();

			Global.Skull.UpdateSkull(MaxHp, Hp);
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
				fadeTime = 0.25f;
				Global.MessageBox.Message($"Thou drowneth, lungs aflood with sorrow.", color: "red");
			}
			else
			{
				Global.MessageBox.Message($"Thy tale endeth here. Thou hast perished in the dungeon.", color: "red");
			}

			BlockInput();
			_isDead = true;
			_screenFader?.FadeToBlack(fadeTime);

			await ToSignal(GetTree().CreateTimer(_respawnTimer), SceneTreeTimer.SignalName.Timeout);

			Global.Dungeon.StartNewGame();
		}

		/// <summary>
		/// Restores the player to an active state by resetting health and death status.
		/// </summary>
		public void Spawn()
		{
			Hp = MaxHp;
			_isDead = false;
			Global.Skull.UpdateSkull(MaxHp, Hp);
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
