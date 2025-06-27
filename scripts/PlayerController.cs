using Godot;

namespace dungeonCrawler
{
	public partial class PlayerController : Node3D
	{
		[Export] private float _movementSpeed = 6f;
		[Export] private AudioStreamPlayer3D _footstepPlayer;
		[Export] private RayCast3D _frontRay;
		[Export] private RayCast3D _leftRay;
		[Export] private RayCast3D _rightRay;
		[Export] private RayCast3D _backRay;
		[Export] private Dungeon _dungeon;

		private Tween tween;
		private Stairs _stairs;
		public bool _blockInput = false;

		private float _travelDistance = 2f;
		private float _travelTime;


		public override void _Ready()
		{
			_travelTime = _travelDistance / _movementSpeed;

			if (_footstepPlayer == null)
			{
				GD.PrintErr("Sound effect player is not assigned!");
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			if (!_blockInput)
			{
				if (tween != null && tween.IsRunning())
					return;

				if (Input.IsActionPressed("MoveForward"))
				{
					if (CheckStairs(_frontRay))
					{
						_dungeon.ChangeLevel(_stairs.ReturnTargetScene(),
							_stairs.ReturnNewPlayerPos(),
							_stairs.ReturnNewPlayerRot());
					}
					else if (!_frontRay.IsColliding())
					{
						MoveInDirection(-Transform.Basis.Z.Normalized());
					}
				}

				if (Input.IsActionPressed("MoveLeft") && !_leftRay.IsColliding())
				{
					MoveInDirection(-Transform.Basis.X.Normalized());
				}

				if (Input.IsActionPressed("MoveRight") && !_rightRay.IsColliding())
				{
					MoveInDirection(Transform.Basis.X.Normalized());
				}

				if (Input.IsActionPressed("MoveBackward") && !_backRay.IsColliding())
				{
					MoveInDirection(Transform.Basis.Z.Normalized());
				}

				if (Input.IsActionPressed("TurnLeft"))
				{
					RotateByAngle(Mathf.Pi / 2);
				}

				if (Input.IsActionPressed("TurnRight"))
				{
					RotateByAngle(-Mathf.Pi / 2);
				}

				if (Input.IsActionPressed("Quit"))
				{
					GetTree().Quit();
				}
			}
		}

		private void MoveInDirection(Vector3 direction)
		{
			tween = CreateTween();
			tween.TweenProperty(this, "transform",
				Transform.Translated(direction * _travelDistance), _travelTime);

			tween.Parallel()
				.TweenCallback(Callable.From(() => PlayFootstep()))
				.SetDelay(_travelTime / 1.5f);
		}

		private void RotateByAngle(float angle)
		{
			tween = CreateTween();
			Basis rotated = Transform.Basis.Rotated(Vector3.Up, angle);
			Transform3D newTransform = Transform;
			newTransform.Basis = rotated;
			tween.TweenProperty(this, "transform", newTransform, _travelTime);

			//tween.Parallel()
			//	.TweenCallback(Callable.From(() => PlayFootstep()))
			//	.SetDelay(_travelTime / 1.5f);
		}

		private void PlayFootstep()
		{
			_footstepPlayer.VolumeDb = (float)GD.RandRange(0f, -5f);
			_footstepPlayer.PitchScale = (float)GD.RandRange(0.5f, 0.75f);
			_footstepPlayer.Play();
		}

		private bool CheckStairs(RayCast3D raycast)
		{
			if (raycast.IsColliding())
			{
				var collider = raycast.GetCollider();

				if (collider is Node colliderNode)
				{
					if (colliderNode.IsInGroup("stairs"))
					{
						if (colliderNode.GetParent() is Stairs stairsNode)
						{
							_stairs = stairsNode;
							return true;
						}
					}
				}
			}

			return false;
		}
	}
}
