using Godot;

namespace DungeonCrawler
{
	public partial class PlayerController : Node3D
	{
		#region Fields and Properties

		// === Exported Config Values ===
		[Export] private float _movementSpeed = 5f;
		[Export] private float _interactDistance = 1.0f;
		[Export] private float _fallDistance = 10f;
		[Export] private float _fallDuration = 1f;
		[Export] private float _raycastDistance = 100f;

		// === Internal Node References ===
		private AudioStreamPlayer3D _sfxPlayer;
		private RayCast3D _frontRaycast;
		private RayCast3D _leftRaycast;
		private RayCast3D _rightRaycast;
		private RayCast3D _backRaycast;
		private RayCast3D _downRaycast;
		private Camera3D _playerCamera;
		private Player _player;

		// === Constants ===
		private const float TravelDistance = 2f;

		// === Private State ===
		private float _travelTime;
		private Dungeon _dungeon;
		private Stairs _stairs;
		private Tween _tween;
		private bool _blocksInput = false;

		// === Public Properties ===
		public bool BlocksInput => _blocksInput;
		public Tween TweenInstance => _tween;

		#endregion

		#region Godot Lifecycle

		/// <summary>
		/// Called when the node enters the scene tree. Initializes node references and setup logic.
		/// </summary>
		public override void _Ready()
		{
			// Reference player node from parent
			_player = GetParent<Player>();
			if (_player == null)
				GD.PrintErr("PlayerController: Failed to find Player node as parent.");

			// Get Dungeon reference from scene root
			Node main = GetTree().Root.GetNodeOrNull("Main");
			if (main == null)
			{
				GD.PrintErr("PlayerController: 'Main' node not found in scene tree.");
				return;
			}

			_dungeon = main.GetNodeOrNull<Dungeon>("GameWorld/Dungeon");
			if (_dungeon == null)
				GD.PrintErr("PlayerController: Dungeon node not found at 'GameWorld/Dungeon'.");

			// Internal child nodes
			_frontRaycast = GetNodeOrNull<RayCast3D>("CollisionShape3D/FrontRaycast");
			_leftRaycast = GetNodeOrNull<RayCast3D>("CollisionShape3D/LeftRaycast");
			_rightRaycast = GetNodeOrNull<RayCast3D>("CollisionShape3D/RightRaycast");
			_backRaycast = GetNodeOrNull<RayCast3D>("CollisionShape3D/BackRaycast");
			_downRaycast = GetNodeOrNull<RayCast3D>("CollisionShape3D/DownRaycast");
			_playerCamera = GetNodeOrNull<Camera3D>("PlayerCamera");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");

			// Null checks with clear messages
			if (_frontRaycast == null) GD.PrintErr("PlayerController: FrontRaycast not found.");
			if (_leftRaycast == null) GD.PrintErr("PlayerController: LeftRaycast not found.");
			if (_rightRaycast == null) GD.PrintErr("PlayerController: RightRaycast not found.");
			if (_backRaycast == null) GD.PrintErr("PlayerController: BackRaycast not found.");
			if (_downRaycast == null) GD.PrintErr("PlayerController: DownRaycast not found.");
			if (_playerCamera == null) GD.PrintErr("PlayerController: PlayerCamera not found.");
			if (_sfxPlayer == null) GD.PrintErr("PlayerController: SFXPlayer not found.");

			_travelTime = TravelDistance / _movementSpeed;
		}

		/// <summary>
		/// Called every physics frame. Handles movement, fall detection, and input logic.
		/// </summary>
		public override void _PhysicsProcess(double delta)
		{
			if (_player.IsInputBlocked || IsTweenRunning() || CheckForFall())
				return;

			HandleMovementInput();
			HandleRotationInput();
			HandleSystemInput();
		}

		#endregion

		#region Input Handling

		/// <summary>
		/// Called when any input event occurs. Used here to detect mouse click interactions.
		/// </summary>
		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseEvent &&
				mouseEvent.Pressed &&
				mouseEvent.ButtonIndex == MouseButton.Left)
			{
				TryClickObject();
			}
		}

		/// <summary>
		/// Handles directional movement input (WASD or equivalent actions).
		/// </summary>
		private void HandleMovementInput()
		{
			Vector3 direction = Vector3.Zero;
			RayCast3D raycast = null;

			if (Input.IsActionPressed("MoveForward"))
			{
				direction = -_player.Transform.Basis.Z.Normalized();
				raycast = _frontRaycast;
			}
			else if (Input.IsActionPressed("MoveLeft"))
			{
				direction = -_player.Transform.Basis.X.Normalized();
				raycast = _leftRaycast;
			}
			else if (Input.IsActionPressed("MoveRight"))
			{
				direction = _player.Transform.Basis.X.Normalized();
				raycast = _rightRaycast;
			}
			else if (Input.IsActionPressed("MoveBackward"))
			{
				direction = _player.Transform.Basis.Z.Normalized();
				raycast = _backRaycast;
			}

			if (raycast == null || direction == Vector3.Zero)
				return;

			// Priority 1: check for stairs
			if (CheckStairs(raycast))
			{
				_dungeon?.ChangeLevel(
					_stairs.ReturnTargetScene(),
					_stairs.ReturnNewPlayerPos(),
					_stairs.ReturnNewPlayerRot()
				);
				return;
			}

			// Priority 2: check for illusory wall
			if (!CheckIllusoryWall(raycast))
			{
				MoveInDirection(direction);
				return;
			}

			// Default movement if no collision
			if (!raycast.IsColliding())
			{
				MoveInDirection(direction);
			}
		}

		/// <summary>
		/// Handles player rotation input (e.g., turning left/right).
		/// </summary>
		private void HandleRotationInput()
		{
			if (Input.IsActionPressed("TurnLeft"))
				RotateByAngle(Mathf.Pi / 2f);

			if (Input.IsActionPressed("TurnRight"))
				RotateByAngle(-Mathf.Pi / 2f);
		}

		/// <summary>
		/// Handles non-gameplay system-level inputs like quitting the game.
		/// </summary>
		private void HandleSystemInput()
		{
			if (Input.IsActionPressed("Quit"))
				GetTree().Quit();
		}

		#endregion

		#region Movement and Rotation

		/// <summary>
		/// Moves the player in a given world direction using tween animation.
		/// </summary>
		private void MoveInDirection(Vector3 direction)
		{
			_tween = CreateTween();
			Transform3D targetTransform = _player.Transform.Translated(direction * TravelDistance);

			_tween.TweenProperty(_player, "transform", targetTransform, _travelTime);
			_tween.Parallel()
				  .TweenCallback(Callable.From(() => PlayFootstep()))
				  .SetDelay(_travelTime / 1.5f);
		}

		/// <summary>
		/// Rotates the player around the Y-axis by the specified angle (in radians).
		/// </summary>
		private void RotateByAngle(float angle)
		{
			_tween = CreateTween();

			Basis rotated = _player.Transform.Basis.Rotated(Vector3.Up, angle);
			Transform3D newTransform = _player.Transform;
			newTransform.Basis = rotated;

			_tween.TweenProperty(_player, "transform", newTransform, _travelTime);
		}

		#endregion

		#region Fall and Gravity

		/// <summary>
		/// Checks if the player should fall (i.e., not standing on anything).
		/// </summary>
		private bool CheckForFall()
		{
			if (_downRaycast == null || _downRaycast.IsColliding())
				return false;

			Fall();
			return true;
		}

		/// <summary>
		/// Animates the player falling and blocks input during the process.
		/// </summary>
		private void Fall()
		{
			_player.BlockInput();
			_tween = CreateTween();

			Vector3 startPos = _player.GlobalPosition;
			Vector3 endPos = startPos + Vector3.Down * _fallDistance;

			_tween.TweenProperty(_player, "global_position", endPos, _fallDuration)
				  .SetTrans(Tween.TransitionType.Cubic)
				  .SetEase(Tween.EaseType.In);
		}

		#endregion

		#region Interaction and Raycasting

		/// <summary>
		/// Attempts to interact with an object the player clicks on within range.
		/// </summary>
		private void TryClickObject()
		{
			if (_playerCamera == null)
				return;

			Vector2 mousePos = GetViewport().GetMousePosition();
			Vector3 from = _playerCamera.ProjectRayOrigin(mousePos);
			Vector3 to = from + _playerCamera.ProjectRayNormal(mousePos) * _raycastDistance;

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
					if (distance <= _interactDistance && collider.HasMethod("OnInteract"))
					{
						collider.Call("OnInteract");
					}
				}
			}
		}

		/// <summary>
		/// Checks whether a given ray is hitting a stair object, and caches a reference if so.
		/// </summary>
		private bool CheckStairs(RayCast3D raycast)
		{
			if (raycast == null || !raycast.IsColliding())
				return false;

			var collider = raycast.GetCollider();
			if (collider is Node colliderNode && colliderNode.IsInGroup("stairs"))
			{
				if (colliderNode.GetParent() is Stairs stairsNode)
				{
					_stairs = stairsNode;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if the given raycast is colliding with an illusory wall.
		/// If so, triggers the wall to reveal itself and returns true.
		/// </summary>
		/// <param name="raycast">The RayCast3D used for detection.</param>
		/// <returns>True if an illusory wall was detected and triggered; otherwise, false.</returns>
		private static bool CheckIllusoryWall(RayCast3D raycast)
		{
			if (raycast == null || !raycast.IsColliding())
				return false;

			var collider = raycast.GetCollider();
			if (collider is Node colliderNode && colliderNode.IsInGroup("illusory-wall"))
			{
				if (colliderNode.GetParent() is IllusoryWall wall)
				{
					wall.TryReveal();
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Utility

		/// <summary>
		/// Plays a randomized footstep sound effect.
		/// </summary>
		private void PlayFootstep()
		{
			if (_sfxPlayer == null)
				return;

			_sfxPlayer.VolumeDb = (float)GD.RandRange(-100f, -105f);
			_sfxPlayer.PitchScale = (float)GD.RandRange(0.5f, 0.75f);
			_sfxPlayer.Play();
		}

		/// <summary>
		/// Checks if a tween is currently active and running.
		/// </summary>
		private bool IsTweenRunning() =>
			_tween != null && _tween.IsRunning();

		#endregion
	}
}
