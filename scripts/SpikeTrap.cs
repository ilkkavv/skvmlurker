using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// SpikeTrap detects when the player steps on it using Area3D.
	/// It plays sound effects, raises and lowers the spikes visually,
	/// and causes the player damage.
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
		private Player _player;
		private Vector3 _spikesUpPos = new(0, 1, 0);
		private bool _dealDamage = true;
		private Tween _tween;
		private float _damageTimer = 0.3f;

		#endregion

		#region Lifecycle

		/// <summary>
		/// Initializes node references and connects area signals.
		/// </summary>
		public override void _Ready()
		{
			// Find child nodes
			_triggerArea = GetNodeOrNull<Area3D>("TriggerArea");
			_damageArea = GetNodeOrNull<Area3D>("Spikes/DamageArea");
			_spikes = GetNodeOrNull<StaticBody3D>("Spikes");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			// Error checks for critical nodes
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

			// Connect signals
			_triggerArea.BodyEntered += OnBodyEnteredTrigger;
			_triggerArea.BodyExited += OnBodyExitedTrigger;
			_damageArea.BodyEntered += OnBodyEnteredDamage;
		}

		#endregion

		#region Signal Handlers

		/// <summary>
		/// Called when a body enters the trigger area. If it's the player, trigger the spike trap.
		/// </summary>
		private void OnBodyEnteredTrigger(Node3D body)
		{
			if (body.IsInGroup("player"))
				Trigger(_spikesUpPos);
		}

		/// <summary>
		/// Called when a body exits the trigger area. If it's the player, retract the spikes.
		/// </summary>
		private void OnBodyExitedTrigger(Node3D body)
		{
			if (body.IsInGroup("player"))
			{
				Reset();
				_dealDamage = true;
			}
		}

		private async void OnBodyEnteredDamage(Node3D body)
		{
			if (body.IsInGroup("player") && _dealDamage)
			{
				// Resolve the Player script (assumes trigger is child of PlayerController)
				_player = body.GetParentOrNull<Player>();

				if (_player == null)
				{
					GD.PrintErr("SpikeTrap: Could not resolve Player script.");
					return;
				}

				_player.BlockInput();

				_dealDamage = false;
				_player.TakeDamage(1, 6);

				await ToSignal(GetTree().CreateTimer(_damageTimer), SceneTreeTimer.SignalName.Timeout);
				_player.UnblockInput();
			}
		}

		#endregion

		#region Spikes Logic

		private void Trigger(Vector3 newSpikesPos)
		{
			_tween?.Kill();

			PlaySfx(_triggerSfxPath);
			_spikes.Position = newSpikesPos;
		}

		private void Reset()
		{
			_tween = CreateTween();
			_tween.TweenProperty(_spikes, "position", Vector3.Zero, 0.5);

			PlaySfx(_resetSfxPath);
		}

		#endregion

		private void PlaySfx(string sfxPath)
		{
			// Load and play appropriate sound effect
			if (_sfxPlayer != null)
			{
				AudioStream stream = GD.Load<AudioStream>(sfxPath);
				if (stream != null)
				{
					_sfxPlayer.Stream = stream;
					_sfxPlayer.Play();
				}
				else
				{
					GD.PrintErr($"SpikeTrap: Could not load sound: {sfxPath}");
				}
			}
		}
	}
}
