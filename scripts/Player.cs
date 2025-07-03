using Godot;

namespace dungeonCrawler
{
	public partial class Player : Node3D
	{
		#region Fields and Properties

		private bool _isInputBlocked = true;
		private PlayerController _playerController;

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
	}
}
