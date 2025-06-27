using Godot;

namespace dungeonCrawler
{
	public partial class Stairs : Node3D
	{
		[Export(PropertyHint.File, "*.tscn")]
		private string _targetScenePath = "";

		[Export] private Vector3 _newPlayerPos;
		[Export] private float _newPlayerRot = 0f;

		public PackedScene ReturnTargetScene()
		{
			if (string.IsNullOrEmpty(_targetScenePath))
			{
				GD.PrintErr("Stairs target scene path is empty!");
				return null;
			}

			var scene = ResourceLoader.Load<PackedScene>(_targetScenePath);
			if (scene == null)
			{
				GD.PrintErr($"Failed to load scene at {_targetScenePath}");
			}
			return scene;
		}

		public Vector3 ReturnNewPlayerPos() => _newPlayerPos;

		public float ReturnNewPlayerRot() => _newPlayerRot;
	}
}