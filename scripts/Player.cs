using System;
using Godot;

namespace DungeonCrawler
{
	public partial class Player : Node3D
	{
		#region Player Attributes and Stats

		private int _str;
		private int _agi;
		private int _con;
		private int _wis;

		private int _hp = 20;

		#endregion

		#region Fields and Properties

		private bool _isInputBlocked = true;
		private PlayerController _playerController;
		private ScreenFlasher _screenFlasher;

		/// <summary>
		/// Whether player input is currently blocked.
		/// </summary>
		public bool IsInputBlocked => _isInputBlocked;

		#endregion

		#region Godot Lifecycle

		/// <summary>
		/// Called when the node enters the scene tree.
		/// Sets up references to child components.
		/// </summary>
		public override void _Ready()
		{
			_playerController = GetNodeOrNull<PlayerController>("PlayerController");
			if (_playerController == null)
			{
				GD.PrintErr("Player: Failed to find PlayerController as a child node.");
			}

			// Get Dungeon reference from scene root
			Node main = GetTree().Root.GetNodeOrNull("Main");
			if (main == null)
			{
				GD.PrintErr("PlayerController: 'Main' node not found in scene tree.");
				return;
			}

			_screenFlasher = main.GetNodeOrNull<ScreenFlasher>("CanvasLayer/ScreenFlasher");
			if (_screenFlasher == null)
				GD.PrintErr("PlayerController: ScreenFlasher node not found at 'CanvasLayer/ScreenFlasher'.");
		}

		#endregion

		#region Input Control

		/// <summary>
		/// Blocks player input. Used during animations or scripted events.
		/// </summary>
		public void BlockInput()
		{
			_isInputBlocked = true;
		}

		/// <summary>
		/// Unblocks player input. Call when player can move again.
		/// </summary>
		public void UnblockInput()
		{
			_isInputBlocked = false;
		}

		#endregion

		#region Control and Animation

		/// <summary>
		/// Immediately stops the player by killing the current tween animation.
		/// </summary>
		public void StopPlayer()
		{
			_playerController?.TweenInstance?.Kill();
		}

		#endregion

		public void TakeDamage(int diceCount, int diceType)
		{
			Random rnd = new();
			int damage = diceCount * rnd.Next(1, (diceType + 1));
			GD.Print($"You take {damage} damage!");
			_hp -= damage;
			GD.Print($"HP = {_hp}.");

			// Screen flash for damage effect
			_screenFlasher?.Flash(new Color(255f, 0f, 0f, 255f)); // Red
			_playerController.PlayHurt();

			if (_hp <= 0) Die();
		}

		private void Die()
		{
			GD.Print("You died!");
		}
	}
}
