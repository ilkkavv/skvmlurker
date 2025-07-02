using Godot;

namespace dungeonCrawler
{
	public partial class PlayerController : Node3D
	{
		[Export] private float _movementSpeed = 5f;
		[Export] private AudioStreamPlayer3D _sfxPlayer;
		[Export] private RayCast3D _frontRay;
		[Export] private RayCast3D _leftRay;
		[Export] private RayCast3D _rightRay;
		[Export] private RayCast3D _backRay;
		[Export] private RayCast3D _downRay;
		private Dungeon _dungeon;

		[Export] private Camera3D _playerCamera;
		[Export] private Player _player;
		[Export] private float _interactDistance = 1.0f;

		private Tween tween;
		private Stairs _stairs;
		public bool _blocksInput = false;

		private float _travelDistance = 2f;
		private float _travelTime;


		public override void _Ready()
		{
			// Get the root node (Main)
			Node main = GetTree().Root.GetNode("Main");
			// Get Dungeon node from the scene
			_dungeon = main.GetNode<Dungeon>("GameWorld/Dungeon");

			_travelTime = _travelDistance / _movementSpeed;

			if (_sfxPlayer == null)
			{
				GD.PrintErr("Sound effect player is not assigned!");
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			if (!_player._blockInput)
			{
				if (tween != null && tween.IsRunning())
					return;

				if (CheckForFall())
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
							MoveInDirection(-_player.Transform.Basis.Z.Normalized()); // ← Forward
						}
					}

				if (Input.IsActionPressed("MoveLeft") && !_leftRay.IsColliding())
				{
					MoveInDirection(-_player.Transform.Basis.X.Normalized()); // ← Left
				}

				if (Input.IsActionPressed("MoveRight") && !_rightRay.IsColliding())
				{
					MoveInDirection(_player.Transform.Basis.X.Normalized()); // ← Right
				}

				if (Input.IsActionPressed("MoveBackward") && !_backRay.IsColliding())
				{
					MoveInDirection(_player.Transform.Basis.Z.Normalized()); // ← Backward
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

			Transform3D targetTransform = _player.Transform.Translated(direction * _travelDistance);

			tween.TweenProperty(_player, "transform", targetTransform, _travelTime);

			tween.Parallel()
				.TweenCallback(Callable.From(() => PlayFootstep()))
				.SetDelay(_travelTime / 1.5f);
		}

		private void RotateByAngle(float angle)
		{
			tween = CreateTween();

			Basis rotated = _player.Transform.Basis.Rotated(Vector3.Up, angle);
			Transform3D newTransform = _player.Transform;
			newTransform.Basis = rotated;

			tween.TweenProperty(_player, "transform", newTransform, _travelTime);
		}

		private bool CheckForFall()
		{
			if (_downRay.IsColliding())
			{
				return false;
			}
			else
			{
				Fall();
				return true;
			}
		}

		private void Fall()
		{
			// Block input during fall (optional)
			_player._blockInput = true;

			// Create tween
			tween = CreateTween();

			// Fall settings
			float fallDistance = 10f;
			float fallDuration = 1f;

			// Target position
			Vector3 startPos = _player.GlobalPosition;
			Vector3 endPos = startPos + Vector3.Down * fallDistance;

			// Animate just the position
			tween.TweenProperty(_player, "global_position", endPos, fallDuration)
				 .SetTrans(Tween.TransitionType.Cubic)
				 .SetEase(Tween.EaseType.In);
		}


		private void PlayFootstep()
		{
			_sfxPlayer.VolumeDb = (float)GD.RandRange(-100f, -105f);
			_sfxPlayer.PitchScale = (float)GD.RandRange(0.5f, 0.75f);
			_sfxPlayer.Play();
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
					float distance = GlobalPosition.DistanceTo(collider.GlobalPosition);

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
