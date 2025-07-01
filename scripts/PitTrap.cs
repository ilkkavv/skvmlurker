using Godot;
using System;

namespace dungeonCrawler
{
	public partial class PitTrap : Node3D
	{
		[Export(PropertyHint.File, "*.tscn")]
		private string _targetScenePath = "";

		[Export] private Vector3 _newPlayerPos;

		[Export] private StaticBody3D _fakeFloor;
		[Export] private RayCast3D _raycast;
		[Export] private AudioStreamPlayer3D _sfxPlayer;

		private bool _triggered = false;
		private Dungeon _dungeon;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			if (_triggered)
			{
				_fakeFloor.Visible = false;
			}

			// Get the root node (Main)
			Node main = GetTree().Root.GetNode("Main");
			// Get Dungeon node from the scene
			_dungeon = main.GetNode<Dungeon>("GameWorld/Dungeon");
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			CheckForPlayer();
		}

		private void CheckForPlayer()
		{
			if (_raycast.IsColliding() && !_triggered)
			{
				var collider = _raycast.GetCollider();

				if (collider is Node colliderNode)
				{
					if (colliderNode.IsInGroup("player"))
					{
						TriggerTrap();
					}
				}
			}
		}

		private async void TriggerTrap()
		{
			_triggered = true;
			_sfxPlayer.Play();

			_fakeFloor.Visible = false;

			var collider = _fakeFloor.GetNode<CollisionShape3D>("CollisionShape3D");

			// Safely disable in next physics frame
			collider.SetDeferred("disabled", true);

			await ToSignal(GetTree().CreateTimer(1f), "timeout");

			_dungeon.ChangeLevel(ResourceLoader.Load<PackedScene>(_targetScenePath), _newPlayerPos);
		}
	}
}
