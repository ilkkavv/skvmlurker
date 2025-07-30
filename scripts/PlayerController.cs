using Godot;

namespace DungeonCrawler
{
	public partial class PlayerController : Node3D
	{
		#region Fields and Properties

		// === Exported Config Values ===
		[Export] private float _movementSpeed = 5f;
		[Export] private float _interactDistance = 2.0f;
		[Export] private float _fallDistance = 10f;
		[Export] private float _fallDuration = 1f;
		[Export] private float _raycastDistance = 100f;
		[Export] private string _pathToFootstepSfx = "res://assets/audio/sfx/footstep.wav";
		[Export] private string _pathToGruntSfx = "res://assets/audio/sfx/grunt.wav";
		[Export] private string _pathToHurtSfx = "res://assets/audio/sfx/hurt.wav";
		[Export] private string _pathToDeathSfx = "res://assets/audio/sfx/death.wav";
		[Export] private string _narrationStairs = "You enter the stairwell, where shadow and stone entwine.";
		[Export] private string _narrationFall = "A misstep! Down you plummet.";

		// === Internal Node References ===
		private AudioStreamPlayer3D _sfxPlayer;
		private RayCast3D _frontRaycast;
		private RayCast3D _leftRaycast;
		private RayCast3D _rightRaycast;
		private RayCast3D _backRaycast;
		private RayCast3D _downRaycast;
		private Camera3D _playerCamera;
		private AudioStream _footstepSound;
		private AudioStream _gruntSound;
		private AudioStream _hurtSound;
		private AudioStream _deathSound;

		// === Constants ===
		private const float TravelDistance = 2f;

		private const int FootstepCount = 3;
		private const float BaseVolumeDb = -110f;
		private const float VolumeStepDb = 5f;
		private const float MinPitch = 0.5f;
		private const float MaxPitch = 0.75f;
		private const float StepDelay = 0.24f;
		private const float GruntVolumeDb = 0f;
		private const float GruntPitchScale = 1f;
		private const float HurtVolumeDb = 0f;
		private const float HurtPitchScale = 1f;
		private const float RandomFootstepVolumeMin = -110f;
		private const float RandomFootstepVolumeMax = -105f;

		// === Private State ===
		private float _travelTime;
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
			// Internal child nodes
			_frontRaycast = GetNodeOrNull<RayCast3D>("CollisionShape3D/FrontRaycast");
			_leftRaycast = GetNodeOrNull<RayCast3D>("CollisionShape3D/LeftRaycast");
			_rightRaycast = GetNodeOrNull<RayCast3D>("CollisionShape3D/RightRaycast");
			_backRaycast = GetNodeOrNull<RayCast3D>("CollisionShape3D/BackRaycast");
			_downRaycast = GetNodeOrNull<RayCast3D>("CollisionShape3D/DownRaycast");
			_playerCamera = GetNodeOrNull<Camera3D>("PlayerCamera");
			_sfxPlayer = GetNodeOrNull<AudioStreamPlayer3D>("SFXPlayer");
			_footstepSound = GD.Load<AudioStream>(_pathToFootstepSfx);
			_gruntSound = GD.Load<AudioStream>(_pathToGruntSfx);
			_hurtSound = GD.Load<AudioStream>(_pathToHurtSfx);
			_deathSound = GD.Load<AudioStream>(_pathToDeathSfx);

			// Null checks with clear messages
			if (_frontRaycast == null) GD.PrintErr("PlayerController: FrontRaycast not found.");
			if (_leftRaycast == null) GD.PrintErr("PlayerController: LeftRaycast not found.");
			if (_rightRaycast == null) GD.PrintErr("PlayerController: RightRaycast not found.");
			if (_backRaycast == null) GD.PrintErr("PlayerController: BackRaycast not found.");
			if (_downRaycast == null) GD.PrintErr("PlayerController: DownRaycast not found.");
			if (_playerCamera == null) GD.PrintErr("PlayerController: PlayerCamera not found.");
			if (_sfxPlayer == null) GD.PrintErr("PlayerController: SFXPlayer not found.");
			if (_footstepSound == null) GD.PrintErr("PlayerController: Footstep sound effect not found.");
			if (_gruntSound == null) GD.PrintErr("PlayerController: Grunt sound effect not found.");
			if (_hurtSound == null) GD.PrintErr("PlayerController: Hurt sound effect not found.");
			if (_deathSound == null) GD.PrintErr("PlayerController: Death sound effect not found.");

			_travelTime = TravelDistance / _movementSpeed;
		}

		/// <summary>
		/// Called every physics frame. Handles movement, fall detection, and input logic.
		/// </summary>
		public override void _PhysicsProcess(double delta)
		{
			HandleSystemInput();

			if (Global.Player.IsInputBlocked || IsTweenRunning() || CheckForFall())
				return;

			HandleMovementInput();
			HandleRotationInput();
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
				direction = -Global.Player.Transform.Basis.Z.Normalized();
				raycast = _frontRaycast;
			}
			else if (Input.IsActionPressed("MoveLeft"))
			{
				direction = -Global.Player.Transform.Basis.X.Normalized();
				raycast = _leftRaycast;
			}
			else if (Input.IsActionPressed("MoveRight"))
			{
				direction = Global.Player.Transform.Basis.X.Normalized();
				raycast = _rightRaycast;
			}
			else if (Input.IsActionPressed("MoveBackward"))
			{
				direction = Global.Player.Transform.Basis.Z.Normalized();
				raycast = _backRaycast;
			}

			// Placeholder brightness adjustment
			else if (Input.IsActionJustPressed("IncreaseBrightness"))
			{
				if (Global.WorldEnvironment.Environment.TonemapExposure < 2.0f)
					Global.WorldEnvironment.Environment.TonemapExposure += 0.1f;
			}
			else if (Input.IsActionJustPressed("DecreaseBrightness"))
			{
				if (Global.WorldEnvironment.Environment.TonemapExposure > 0.5f)
					Global.WorldEnvironment.Environment.TonemapExposure -= 0.1f;
			}

			if (raycast == null || direction == Vector3.Zero)
				return;

			// 1. Try stairs
			if (CheckStairs(raycast))
			{
				Global.MessageBox.Message(_narrationStairs, Global.Grey);
				PlayStairFootsteps();

				Global.Dungeon?.ChangeLevel(
					_stairs.ReturnTargetScene(),
					_stairs.ReturnNewPlayerPos(),
					_stairs.ReturnNewPlayerRot()
				);
				return;
			}

			// 2. Try illusory wall
			if (CheckIllusoryWall(raycast))
			{
				// Don't move immediately; wait for reveal
				return;
			}

			// 3. Check if clear
			if (!raycast.IsColliding())
			{
				MoveInDirection(direction);
			}
			else
			{
				if (!_sfxPlayer.Playing)
					PlayGrunt();
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
			Transform3D targetTransform = Global.Player.Transform.Translated(direction * TravelDistance);

			_tween.TweenProperty(Global.Player, "transform", targetTransform, _travelTime);
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

			Basis rotated = Global.Player.Transform.Basis.Rotated(Vector3.Up, angle);
			Transform3D newTransform = Global.Player.Transform;
			newTransform.Basis = rotated;

			_tween.TweenProperty(Global.Player, "transform", newTransform, _travelTime);
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
			Global.MessageBox.Message(_narrationFall, Global.Red);

			Global.Player.BlockInput();
			_tween = CreateTween();

			Vector3 startPos = Global.Player.GlobalPosition;
			Vector3 endPos = startPos + Vector3.Down * _fallDistance;

			_tween.TweenProperty(Global.Player, "global_position", endPos, _fallDuration)
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
		/// Plays a randomized footstep sound with pitch and volume variation.
		/// </summary>
		private void PlayFootstep()
		{
			_sfxPlayer.Stream = _footstepSound;
			_sfxPlayer.VolumeDb = (float)GD.RandRange(RandomFootstepVolumeMin, RandomFootstepVolumeMax);
			_sfxPlayer.PitchScale = (float)GD.RandRange(MinPitch, MaxPitch);
			_sfxPlayer.Play();
		}

		/// <summary>
		/// Plays 3 randomized stair footstep sounds with decreasing volume and varied pitch, spaced evenly.
		/// </summary>
		private async void PlayStairFootsteps()
		{
			int counter = 0;

			while (counter < FootstepCount)
			{
				_sfxPlayer.Stream = _footstepSound;
				_sfxPlayer.VolumeDb = BaseVolumeDb - (VolumeStepDb * counter);
				_sfxPlayer.PitchScale = (float)GD.RandRange(MinPitch, MaxPitch);
				_sfxPlayer.Play();
				counter += 1;
				await ToSignal(GetTree().CreateTimer(StepDelay), SceneTreeTimer.SignalName.Timeout);
			}
		}

		/// <summary>
		/// Plays a grunt sound effect with default pitch and volume.
		/// </summary>
		private void PlayGrunt()
		{
			_sfxPlayer.Stream = _gruntSound;
			_sfxPlayer.VolumeDb = GruntVolumeDb;
			_sfxPlayer.PitchScale = GruntPitchScale;
			_sfxPlayer.Play();
		}

		/// <summary>
		/// Plays a hurt or death sound effect based on player HP, using default pitch and volume.
		/// </summary>
		public void PlayHurt()
		{
			_sfxPlayer.Stream = Global.Player.Hp > 0 ? _hurtSound : _deathSound;
			_sfxPlayer.VolumeDb = HurtVolumeDb;
			_sfxPlayer.PitchScale = HurtPitchScale;
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
