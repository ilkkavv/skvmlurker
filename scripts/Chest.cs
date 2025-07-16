using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Represents a lootable chest in the dungeon. Manages lid movement, sound effects,
	/// and persistence of opened state across game sessions.
	/// </summary>
	public partial class Chest : Node3D
	{
		#region Exported Properties

		/// <summary>
		/// Unique ID used for saving/loading the chest's state.
		/// </summary>
		[Export] public string ChestId { private set; get; }

		/// <summary>
		/// The loot inside the chest. Displayed to the player when looted.
		/// </summary>
		[Export] private string _loot = "Nothing";

		/// <summary>
		/// Path to the sound effect played when the lid starts to move.
		/// </summary>
		[Export] private string _pathToMoveSfx = "res://assets/audio/sfx/chest-lid-move.ogg";

		/// <summary>
		/// Path to the sound effect played when the lid finishes opening.
		/// </summary>
		[Export] private string _pathToOpenSfx = "res://assets/audio/sfx/chest-lid-open.wav";

		#endregion

		#region Private Fields

		public bool _chestOpen = false;

		private StaticBody3D _chestLid;
		private Dungeon _dungeon;
		private AudioStreamPlayer3D _lidSfxPlayer;
		private AudioStream _moveSound;
		private AudioStream _openSound;

		private float _moveDelay = 1.5f;
		private float _openDelay = 1.0f;

		private Vector3 _lidMovedPos = new Vector3(-0.1f, 0.6f, -0.3f);
		private Vector3 _lidMovedRot = new Vector3(0f, -15f, 0f);
		private Vector3 _lidOpenPos = new Vector3(-0.2f, 0.295f, -0.085f);
		private Vector3 _lidOpenRot = new Vector3(75f, -15f, 0f);

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the node is added to the scene tree. Initializes references and state.
		/// </summary>
		public override void _Ready()
		{
			Node main = GetTree().Root.GetNodeOrNull("Main");
			_dungeon = main?.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");

			_chestLid = GetNodeOrNull<StaticBody3D>("ChestLid");
			_lidSfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("ChestLid/SFXPlayer");
			_moveSound = GD.Load<AudioStream>(_pathToMoveSfx);
			_openSound = GD.Load<AudioStream>(_pathToOpenSfx);

			if (string.IsNullOrEmpty(ChestId)) GD.PrintErr("Chest: ID not set.");
			if (_dungeon == null) GD.PrintErr("Chest: Dungeon not found.");
			if (_chestLid == null) GD.PrintErr("Chest: Lid node not found.");
			if (_moveSound == null) GD.PrintErr("Chest: Move sound effect not found.");
			if (_openSound == null) GD.PrintErr("Chest: Open sound effect not found.");

			_dungeon?.AddObject(this);
			InitializeState();
		}

		#endregion

		#region Interaction

		/// <summary>
		/// Opens the chest, plays lid animations and sounds, and prints the loot.
		/// Blocks and unblocks player input during the sequence.
		/// </summary>
		/// <param name="player">The player interacting with the chest.</param>
		public async void LootChest(Player player)
		{
			if (!_chestOpen)
			{
				player.BlockInput();

				_chestOpen = true;

				_lidSfxPlayer.Stream = _moveSound;
				_lidSfxPlayer.Play();

				_chestLid.Position = _lidMovedPos;
				_chestLid.RotationDegrees = _lidMovedRot;

				await ToSignal(GetTree().CreateTimer(_moveDelay), SceneTreeTimer.SignalName.Timeout);

				_lidSfxPlayer.Stream = _openSound;
				_lidSfxPlayer.Play();

				_chestLid.Position = _lidOpenPos;
				_chestLid.RotationDegrees = _lidOpenRot;

				await ToSignal(GetTree().CreateTimer(_openDelay), SceneTreeTimer.SignalName.Timeout);

				GD.Print($"You loot the chest and get: {_loot}!");

				player.UnblockInput();
			}
		}

		#endregion

		#region State Management

		/// <summary>
		/// Loads the chest's saved state and applies the lid's final transform if opened.
		/// </summary>
		private void InitializeState()
		{
			if (_dungeon == null || string.IsNullOrEmpty(ChestId)) return;

			_chestOpen = _dungeon.LoadObjectState("Chest", ChestId, "Open");

			if (_chestOpen && _chestLid != null)
			{
				_chestLid.Position = _lidOpenPos;
				_chestLid.RotationDegrees = _lidOpenRot;
			}
		}

		#endregion
	}
}
