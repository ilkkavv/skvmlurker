using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Interactable component for a gate lock.
	/// Checks if the player has the correct key and opens the lock if valid.
	/// Otherwise, plays a locked sound effect.
	/// </summary>
	public partial class GateLock : Interactable
	{
		#region Exported Properties

		[Export] private string _pathToLockedSfx = "res://assets/audio/sfx/locked.wav";
		[Export] private string _pathToOpenLockSfx = "res://assets/audio/sfx/open-lock.wav";
		[Export] private string _pathToRemoveLockSfx = "res://assets/audio/sfx/remove-lock.wav";
		[Export] private string _openMessage = "With a cunning hand, you unlock the gate.";
		[Export] private string _lockedMessage = "The gate resists you — it is sealed tight.";

		#endregion

		#region Private Fields

		private Gate _gate;
		private AudioStreamPlayer3D _sfxPlayer;
		private float _delayTime = 1.0f;

		private AudioStream _lockedSound;
		private AudioStream _openLockSound;
		private AudioStream _removeLockSound;

		#endregion

		#region Godot Lifecycle

		public override void _Ready()
		{
			_gate = GetParentOrNull<Gate>();
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");
			_lockedSound = GD.Load<AudioStream>(_pathToLockedSfx);
			_openLockSound = GD.Load<AudioStream>(_pathToOpenLockSfx);
			_removeLockSound = GD.Load<AudioStream>(_pathToRemoveLockSfx);

			if (_gate == null) GD.PrintErr("GateLock: Could not find parent Gate.");
			if (_sfxPlayer == null) GD.PrintErr("GateLock: SFXPlayer not found.");
			if (_lockedSound == null) GD.PrintErr("GateLock: Locked sound effect not found.");
			if (_openLockSound == null) GD.PrintErr("GateLock: Open Lock sound effect not found.");
			if (_removeLockSound == null) GD.PrintErr("GateLock: Remove Lock sound effect not found.");
		}

		#endregion

		#region Interactions

		/// <summary>
		/// Called when the player interacts with the gate lock.
		/// Opens the gate if the player's key matches the gate's key ID.
		/// Otherwise, plays a locked sound.
		/// </summary>
		public override void OnInteract()
		{
			if (Global.Player.KeyId == _gate.KeyId)
			{
				Global.MessageBox.Message(_openMessage, Global.Green);
				Open();
			}
			else
			{
				Global.MessageBox.Message(_lockedMessage, Global.Blue);
				_sfxPlayer.Stream = _lockedSound;
				_sfxPlayer.Play();
			}
		}

		/// <summary>
		/// Plays unlock and remove lock sound effects, then signals the gate to unlock.
		/// </summary>
		private async void Open()
		{
			Global.Player.BlockInput();
			_sfxPlayer.Stream = _openLockSound;
			_sfxPlayer.Play();
			await ToSignal(GetTree().CreateTimer(_delayTime), SceneTreeTimer.SignalName.Timeout);
			_sfxPlayer.Stream = _removeLockSound;
			_sfxPlayer.Play();
			_gate?.OpenLock(Global.Player);
		}

		#endregion
	}
}
