using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// An illusory wall that fades out and disables its collider when revealed.
	/// Often used for secret passages. State persists through saves.
	/// </summary>
	public partial class IllusoryWall : Node3D
	{
		#region Exported Properties

		[Export] public string IllusoryWallId { private set; get; }
		[Export] private string _narration = "A false wall reveals its secret — you've discovered a hidden path.";

		#endregion

		#region Private Fields

		private Node3D _wallRoot;
		private MeshInstance3D _mesh;
		private CollisionShape3D _collider;
		private AudioStreamPlayer3D _sfxPlayer;

		public bool _isRevealed = false;

		#endregion

		#region Lifecycle

		/// <summary>
		/// Called when the wall enters the scene. Caches references, initializes state, and registers with the dungeon.
		/// </summary>
		public override void _Ready()
		{
			_wallRoot = GetNodeOrNull<Node3D>("FakeWall");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			if (_wallRoot == null)
			{
				GD.PrintErr("IllusoryWall: 'FakeWall' node not found.");
				return;
			}

			_mesh = _wallRoot.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
			_collider = _wallRoot.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");

			if (_mesh == null) GD.PrintErr("IllusoryWall: MeshInstance3D not found.");
			if (_collider == null) GD.PrintErr("IllusoryWall: CollisionShape3D not found.");
			if (_sfxPlayer == null) GD.PrintErr("IllusoryWall: SFXPlayer node not found.");

			Global.Dungeon.AddObject(this);
			InitializeState();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Reveals the wall by fading out its material and disabling its collider.
		/// Safe to call multiple times; only triggers once.
		/// </summary>
		public async void TryReveal()
		{
			if (_isRevealed || _mesh == null || _collider == null)
				return;

			_isRevealed = true;
			Global.MessageBox.Message(_narration, Global.Green);
			_sfxPlayer?.Play();

			if (_mesh.GetActiveMaterial(0) is not StandardMaterial3D sharedMaterial)
			{
				GD.PrintErr("IllusoryWall: Active material is not a StandardMaterial3D.");
				return;
			}

			var uniqueMaterial = (StandardMaterial3D)sharedMaterial.Duplicate();
			_mesh.SetSurfaceOverrideMaterial(0, uniqueMaterial);

			uniqueMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
			uniqueMaterial.AlbedoColor = new Color(
				uniqueMaterial.AlbedoColor.R,
				uniqueMaterial.AlbedoColor.G,
				uniqueMaterial.AlbedoColor.B,
				1f
			);

			var tween = CreateTween();
			tween.TweenProperty(uniqueMaterial, "albedo_color:a", 0f, 1f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);

			await ToSignal(tween, Tween.SignalName.Finished);

			_collider.SetDeferred("disabled", true);
			_wallRoot.Visible = false;
		}

		#endregion

		#region Initialization

		/// <summary>
		/// Restores the revealed state from the save system, disabling visuals and collision if already triggered.
		/// </summary>
		private void InitializeState()
		{
			if (string.IsNullOrEmpty(IllusoryWallId))
				return;

			_isRevealed = Global.Dungeon.LoadObjectState("IllusoryWall", IllusoryWallId, "Revealed");

			if (_isRevealed && _mesh != null && _collider != null)
			{
				_collider.SetDeferred("disabled", true);
				_wallRoot.Visible = false;
			}
		}

		#endregion
	}
}
