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

		[Export] public string ChestId { private set; get; }
		[Export] private string _loot = "Nothing";
		[Export] private string _pathToMoveSfx = "res://assets/audio/sfx/chest-lid-move.ogg";
		[Export] private string _pathToOpenSfx = "res://assets/audio/sfx/chest-lid-open.wav";
		[Export] private string _openNarration = "With effort and a creak, you manage to open the old chest.";
		[Export] private string _lootNarration = "You find a key of ancient design.";

		#endregion

		#region Private Fields

		public bool _chestOpen = false;

		private StaticBody3D _chestLid;
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
			_chestLid = GetNodeOrNull<StaticBody3D>("ChestLid");
			_lidSfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("ChestLid/SFXPlayer");
			_moveSound = GD.Load<AudioStream>(_pathToMoveSfx);
			_openSound = GD.Load<AudioStream>(_pathToOpenSfx);

			if (string.IsNullOrEmpty(ChestId)) GD.PrintErr("Chest: ID not set.");
			if (_chestLid == null) GD.PrintErr("Chest: Lid node not found.");
			if (_moveSound == null) GD.PrintErr("Chest: Move sound effect not found.");
			if (_openSound == null) GD.PrintErr("Chest: Open sound effect not found.");

			Global.Dungeon?.AddObject(this);
			InitializeState();
		}

		#endregion

		#region Interaction

		/// <summary>
		/// Opens the chest, plays lid animations and sounds, and prints the loot.
		/// Blocks and unblocks player input during the sequence.
		/// </summary>
		public async void LootChest()
		{
			if (!_chestOpen)
			{
				Global.Player.BlockInput();

				_chestOpen = true;

				Global.MessageBox.Message(_openNarration, Global.Grey);

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

				Global.MessageBox.Message(_lootNarration, Global.Green);

				Global.Player.SetKeyId(_loot);

				Global.Player.UnblockInput();
			}
		}

		#endregion

		#region State Management

		/// <summary>
		/// Loads the chest's saved state and applies the lid's final transform if opened.
		/// </summary>
		private void InitializeState()
		{
			if (string.IsNullOrEmpty(ChestId)) return;

			_chestOpen = Global.Dungeon.LoadObjectState("Chest", ChestId, "Open");

			if (_chestOpen && _chestLid != null)
			{
				_chestLid.Position = _lidOpenPos;
				_chestLid.RotationDegrees = _lidOpenRot;
			}
		}

		#endregion
	}
}
