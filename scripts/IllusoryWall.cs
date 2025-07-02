using Godot;

namespace dungeonCrawl
{
	public partial class IllusoryWall : Node3D
	{
		[Export] private MeshInstance3D _wall;
		[Export] private RayCast3D _frontRay;
		[Export] private RayCast3D _backRay;
		[Export] private AudioStreamPlayer3D _sfxPlayer;

		private bool _open = false;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			if (_open)
			{
				Visible = false;
			}
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (!_open)
				CheckForPlayer();
		}

		private void CheckForPlayer()
		{
			Node collider = null;

			if (_frontRay.IsColliding())
			{
				collider = _frontRay.GetCollider() as Node;
			}

			if (_backRay.IsColliding())
			{
				collider = _backRay.GetCollider() as Node;
			}

			if (collider != null && collider.IsInGroup("player"))
			{
				Open();
			}
		}

		private async void Open()
		{
			_open = true;

			_sfxPlayer.Play();

			// Get the original shared material
			var sharedMaterial = _wall.GetActiveMaterial(0) as StandardMaterial3D;
			if (sharedMaterial == null) return;

			// Duplicate it so this wall has its own unique material
			var material = (StandardMaterial3D)sharedMaterial.Duplicate();
			_wall.SetSurfaceOverrideMaterial(0, material); // Apply the unique material

			// Set transparency mode and starting alpha
			material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
			material.AlbedoColor = new Color(material.AlbedoColor.R, material.AlbedoColor.G, material.AlbedoColor.B, 1.0f);

			// Fade out using Tween
			var tween = CreateTween();
			tween.TweenProperty(material, "albedo_color:a", 0.0f, 1f);
			await ToSignal(tween, Tween.SignalName.Finished);

			Visible = false;
		}
	}
}

