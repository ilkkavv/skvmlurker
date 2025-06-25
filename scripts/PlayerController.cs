using Godot;
using System;

namespace dungeonCrawler
{
	public partial class PlayerController : Node3D
	{
		[Export] private RayCast3D _frontRay;
		[Export] private RayCast3D _leftRay;
		[Export] private RayCast3D _rightRay;
		[Export] private RayCast3D _backRay;

		private Tween tween;

		[Export] private float _movementSpeed = 6f;
		private float _travelDistance = 2f;
		private float _travelTime;

		public override void _Ready()
		{
			_travelTime = _travelDistance / _movementSpeed;
		}

		public override void _PhysicsProcess(double delta)
		{
			if (tween != null && tween.IsRunning())
				return;

			// Move forward in the direction the player is facing
			if (Input.IsActionPressed("MoveForward") && !_frontRay.IsColliding())
			{
				tween = CreateTween();
				Vector3 moveDir = -Transform.Basis.Z.Normalized();
				tween.TweenProperty(this, "transform",
					Transform.Translated(moveDir * _travelDistance), _travelTime);
			}

			// Move left in relation to the direction the player is facing
			if (Input.IsActionPressed("MoveLeft") && !_leftRay.IsColliding())
			{
				tween = CreateTween();
				Vector3 moveDir = -Transform.Basis.X.Normalized();
				tween.TweenProperty(this, "transform",
					Transform.Translated(moveDir * _travelDistance), _travelTime);
			}

			// Move right in relation to the direction the player is facing
			if (Input.IsActionPressed("MoveRight") && !_rightRay.IsColliding())
			{
				tween = CreateTween();
				Vector3 moveDir = Transform.Basis.X.Normalized();
				tween.TweenProperty(this, "transform",
					Transform.Translated(moveDir * _travelDistance), _travelTime);
			}

			// Move backward (opposite of current facing)
			if (Input.IsActionPressed("MoveBackward") && !_backRay.IsColliding())
			{
				tween = CreateTween();
				Vector3 moveDir = Transform.Basis.Z.Normalized();
				tween.TweenProperty(this, "transform",
					Transform.Translated(moveDir * _travelDistance), _travelTime);
			}

			// Turn left (90° counter-clockwise)
			if (Input.IsActionPressed("TurnLeft"))
			{
				tween = CreateTween();
				Basis rotated = Transform.Basis.Rotated(Vector3.Up, Mathf.Pi / 2);
				Transform3D newTransform = Transform;
				newTransform.Basis = rotated;
				tween.TweenProperty(this, "transform", newTransform, _travelTime);
			}

			// Turn right (90° clockwise)
			if (Input.IsActionPressed("TurnRight"))
			{
				tween = CreateTween();
				Basis rotated = Transform.Basis.Rotated(Vector3.Up, -Mathf.Pi / 2);
				Transform3D newTransform = Transform;
				newTransform.Basis = rotated;
				tween.TweenProperty(this, "transform", newTransform, _travelTime);
			}

			if (Input.IsActionPressed("Quit"))
			{
				GetTree().Quit();
			}
		}
	}
}
