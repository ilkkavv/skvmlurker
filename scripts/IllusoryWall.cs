using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// An illusory wall that visually fades and disables collision when the player attempts to walk into it.
	/// Commonly used for hidden or secret passageways.
	/// </summary>
	public partial class IllusoryWall : Node3D
	{
		#region Private Fields

		private Node3D _wallRoot;                   // The "Wall" scene instance
		private MeshInstance3D _mesh;               // The mesh inside Wall
		private CollisionShape3D _collider;         // Collider inside Wall
		private AudioStreamPlayer3D _sfxPlayer;     // Reveal SFX
		private bool _isRevealed = false;           // State flag

		#endregion

		#region Lifecycle

		public override void _Ready()
		{
			_wallRoot = GetNodeOrNull<Node3D>("FakeWall");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			if (_wallRoot == null)
			{
				GD.PrintErr("IllusoryWall: 'Wall' node not found.");
				return;
			}

			_mesh = _wallRoot.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
			_collider = _wallRoot.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");

			if (_mesh == null)
				GD.PrintErr("IllusoryWall: MeshInstance3D not found under Wall.");
			if (_collider == null)
				GD.PrintErr("IllusoryWall: CollisionShape3D not found under Wall.");
			if (_sfxPlayer == null)
				GD.PrintErr("IllusoryWall: SFXPlayer node not found.");
		}

		#endregion

		#region Public API

		/// <summary>
		/// Reveals the illusory wall by disabling its collider, fading its mesh,
		/// and hiding it after the fade completes.
		/// </summary>
		public async void TryReveal()
		{
			if (_isRevealed || _mesh == null || _collider == null)
				return;

			_isRevealed = true;

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
	}
}
