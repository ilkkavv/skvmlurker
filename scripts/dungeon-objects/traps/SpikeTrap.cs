using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// A trap that activates when the player steps on it.
	/// Spikes are raised, damage is applied once, and the spikes retract when the player leaves.
	/// </summary>
	public partial class SpikeTrap : Node3D
	{
		#region Exported Properties

		[Export] private string _triggerSfxPath;
		[Export] private string _resetSfxPath;

		#endregion

		#region Private Fields

		private Area3D _triggerArea;
		private Area3D _damageArea;
		private StaticBody3D _spikes;
		private AudioStreamPlayer3D _sfxPlayer;

		private Tween _tween;

		private bool _dealDamage = true;
		private float _damageTimer = 0.3f;

		private readonly Vector3 _spikesUpPos = new(0, 1.5f, 0); // Raised spike position

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the node enters the scene. Grabs child references and sets up signals.
		/// </summary>
		public override void _Ready()
		{
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");
			_damageArea = GetNodeOrNull<Area3D>("Spikes/DamageArea");
			_spikes = GetNodeOrNull<StaticBody3D>("Spikes");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			if (_triggerArea == null)
			{
				GD.PrintErr("SpikeTrap: Missing TriggerArea node.");
				return;
			}
			if (_damageArea == null)
			{
				GD.PrintErr("SpikeTrap: Missing DamageArea node.");
				return;
			}
			if (_spikes == null)
				GD.PrintErr("SpikeTrap: Spikes node not found.");
			if (_sfxPlayer == null)
				GD.PrintErr("SpikeTrap: SFXPlayer node not found.");

			_triggerArea.BodyEntered += OnBodyEnteredTrigger;
			_triggerArea.BodyExited += OnBodyExitedTrigger;
			_damageArea.BodyEntered += OnBodyEnteredDamage;
		}

		#endregion

		#region Trigger Logic

		/// <summary>
		/// Called when a body enters the trigger area. Raises the spikes.
		/// </summary>
		private void OnBodyEnteredTrigger(Node3D body)
		{
			if (body.IsInGroup("player"))
			{
				Trigger(_spikesUpPos);
			}
		}

		/// <summary>
		/// Called when a body exits the trigger area. Resets the spikes.
		/// </summary>
		private void OnBodyExitedTrigger(Node3D body)
		{
			if (body.IsInGroup("player"))
			{
				if (IsInsideTree())
				{
					Reset();
					_dealDamage = true;
				}
			}
		}

		/// <summary>
		/// Called when the player enters the damage area. Deals damage once per activation.
		/// </summary>
		private async void OnBodyEnteredDamage(Node3D body)
		{
			if (body.IsInGroup("player") && _dealDamage)
			{
				Global.Player = body.GetParentOrNull<Player>();

				Global.Player.BlockInput();

				_dealDamage = false;
				Global.Player.TakeDamage(1, 6);

				await ToSignal(GetTree().CreateTimer(_damageTimer), SceneTreeTimer.SignalName.Timeout);
				Global.Player.UnblockInput();
			}
		}

		#endregion

		#region Spike Animation

		/// <summary>
		/// Raises the spikes and plays a trigger sound.
		/// </summary>
		private void Trigger(Vector3 newSpikesPos)
		{
			_tween?.Kill();

			PlaySfx(_triggerSfxPath);
			_spikes.Position = newSpikesPos;
		}

		/// <summary>
		/// Retracts the spikes and plays a reset sound.
		/// </summary>
		private void Reset()
		{
			_tween = CreateTween();
			_tween.TweenProperty(_spikes, "position", Vector3.Zero, 0.5f);

			PlaySfx(_resetSfxPath);
		}

		#endregion

		#region Sound

		/// <summary>
		/// Loads and plays a sound effect from a given path.
		/// </summary>
		private void PlaySfx(string sfxPath)
		{
			if (_sfxPlayer == null)
				return;

			var stream = GD.Load<AudioStream>(sfxPath);
			if (stream != null)
			{
				_sfxPlayer.Stream = stream;
				CallDeferred(nameof(DeferredPlaySfx));
			}
			else
			{
				GD.PrintErr($"SpikeTrap: Could not load sound: {sfxPath}");
			}
		}

		private void DeferredPlaySfx()
		{
			if (_sfxPlayer.IsInsideTree())
				_sfxPlayer.Play();
		}

		#endregion
	}
}
