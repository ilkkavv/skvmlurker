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

		[Export] private Camera3D _playerCamera;
		[Export] private Node3D _player;
		[Export] private float _interactDistance = 1.0f;

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

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				TryClickObject();
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

		private void TryClickObject()
		{
			Vector2 mousePos = GetViewport().GetMousePosition();
			Vector3 from = _playerCamera.ProjectRayOrigin(mousePos);
			Vector3 to = from + _playerCamera.ProjectRayNormal(mousePos) * 100f;

			PhysicsRayQueryParameters3D rayParams = new()
			{
				From = from,
				To = to,
				CollideWithAreas = true,
				CollideWithBodies = true
			};

			var spaceState = GetWorld3D().DirectSpaceState;
			var result = spaceState.IntersectRay(rayParams);

			if (result.Count > 0)
			{
				var collider = result["collider"].As<Node3D>();
				if (collider != null)
				{
					float distance = _player.GlobalPosition.DistanceTo(collider.GlobalPosition);

					if (distance <= _interactDistance)
					{
						if (collider.HasMethod("OnInteract"))
						{
							collider.Call("OnInteract");
						}
					}
				}
			}
		}
	}
}
