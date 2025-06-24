using Godot;

namespace dungeon
{
	public partial class Player : Node3D
	{
		[Export] private float _movementSpeed = 4f;
		[Export] private float _turnSpeed = 10f;
		[Export] private int _playerStartPosX = 0;
		[Export] private int _playerStartPosY = 0;
		[Export] private Test _dungeonMap;
		[Export] private AudioStreamPlayer2D _sfxPlayer;

		private Node3D _leftNode;
		private Node3D _frontNode;
		private Node3D _rightNode;
		private Node3D _backNode;

		private bool _playSound = false;
		private bool _isMoving = false;
		private bool _isRotating = false;
		private float _targetRot = 0f;
		private Vector3 _targetPos = Vector3.Zero;
		private Vector3 _movementDir = Vector3.Zero;

		private bool CheckCollision(float x, float y)
		{
			int mapX = Mathf.FloorToInt(x / 2);
			int mapY = Mathf.FloorToInt(y / 2);

			if (_dungeonMap.ReturnDungeonTile(mapY, mapX) == "#")
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private void MovePlayer(double delta)
		{
			if (_isMoving)
			{
				float distance = GlobalTransform.Origin.DistanceTo(_targetPos);
				float moveStep = _movementSpeed * (float)delta;

				if (distance > 0.01f)
				{
					if (moveStep >= distance)
					{
						// Snap to final position to avoid overshoot
						GlobalTransform = new Transform3D(GlobalTransform.Basis, _targetPos);
						_isMoving = false;

						if (_playSound)
						{
							_sfxPlayer.Play();
							_playSound = false;
						}
					}
					else
					{
						Vector3 newPosition = GlobalTransform.Origin + _movementDir * moveStep;
						GlobalTransform = new Transform3D(GlobalTransform.Basis, newPosition);
						_playSound = true;
					}
				}
				else
				{
					// Fallback safety
					_isMoving = false;
					GlobalTransform = new Transform3D(GlobalTransform.Basis, _targetPos);

					if (_playSound)
					{
						_sfxPlayer.Play();
						_playSound = false;
					}
				}
			}
		}

		private void RotatePlayer(double delta)
		{
			float currentRotY = Rotation.Y;
			float newRotY = Mathf.LerpAngle(currentRotY, _targetRot, _turnSpeed * (float)delta);
			Rotation = new Vector3(0, newRotY, 0);

			// Stop rotating when close enough
			if (Mathf.Abs(Mathf.AngleDifference(newRotY, _targetRot)) < 0.01f)
			{
				Rotation = new Vector3(0, _targetRot, 0);
				_isRotating = false;
				//_sfxPlayer.Play();
			}
		}

		private void HandleInput()
		{
			if (!_isMoving && !_isRotating)
			{
				if (Input.IsActionPressed("TurnLeft"))
				{
					_isRotating = true;
					_targetRot = Rotation.Y + Mathf.Pi / 2;
				}
				if (Input.IsActionPressed("TurnRight"))
				{
					_isRotating = true;
					_targetRot = Rotation.Y - Mathf.Pi / 2;
				}
				if (Input.IsActionPressed("MoveLeft"))
				{
					if (!CheckCollision(_leftNode.GlobalPosition.X, _leftNode.GlobalPosition.Z))
					{
						_isMoving = true;
						_targetPos = _leftNode.GlobalTransform.Origin;
						_movementDir = (_targetPos - GlobalTransform.Origin).Normalized();
					}
				}
				if (Input.IsActionPressed("MoveForward"))
				{
					if (!CheckCollision(_frontNode.GlobalPosition.X, _frontNode.GlobalPosition.Z))
					{
						_isMoving = true;
						_targetPos = _frontNode.GlobalTransform.Origin;
						_movementDir = (_targetPos - GlobalTransform.Origin).Normalized();
					}
				}
				if (Input.IsActionPressed("MoveRight"))
				{
					if (!CheckCollision(_rightNode.GlobalPosition.X, _rightNode.GlobalPosition.Z))
					{
						_isMoving = true;
						_targetPos = _rightNode.GlobalTransform.Origin;
						_movementDir = (_targetPos - GlobalTransform.Origin).Normalized();
					}
				}
				if (Input.IsActionPressed("MoveBackward"))
				{
					if (!CheckCollision(_backNode.GlobalPosition.X, _backNode.GlobalPosition.Z))
					{
						_isMoving = true;
						_targetPos = _backNode.GlobalTransform.Origin;
						_movementDir = (_targetPos - GlobalTransform.Origin).Normalized();
					}
				}
				if (Input.IsActionJustPressed("Quit"))
				{
					GetTree().Quit();
				}
			}
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			_leftNode = GetNode<Node3D>("LeftNode");
			_frontNode = GetNode<Node3D>("FrontNode");
			_rightNode = GetNode<Node3D>("RightNode");
			_backNode = GetNode<Node3D>("BackNode");

			Position = new Vector3(_playerStartPosX * 2, 0, _playerStartPosY * 2);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			HandleInput();
			MovePlayer(delta);
			RotatePlayer(delta);
		}
	}
}
