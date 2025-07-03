using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Represents a stairway that leads to another level and sets the player's new position and rotation.
	/// </summary>
	public partial class Stairs : Node3D
	{
		#region Exported Settings

		[Export(PropertyHint.File, "*.tscn")]
		private string _targetScenePath = "";

		[Export] private Vector3 _newPlayerPos;
		[Export] private float _newPlayerRot = 0f;

		#endregion

		#region Public API

		/// <summary>
		/// Loads and returns the target scene to which the stairs lead.
		/// </summary>
		/// <returns>The target PackedScene if found, or null if not.</returns>
		public PackedScene ReturnTargetScene()
		{
			if (string.IsNullOrEmpty(_targetScenePath))
			{
				GD.PrintErr("Stairs: Target scene path is empty.");
				return null;
			}

			var scene = ResourceLoader.Load<PackedScene>(_targetScenePath);
			if (scene == null)
			{
				GD.PrintErr($"Stairs: Failed to load scene at path '{_targetScenePath}'.");
			}

			return scene;
		}

		/// <summary>
		/// Returns the position to place the player after scene transition.
		/// </summary>
		public Vector3 ReturnNewPlayerPos() => _newPlayerPos;

		/// <summary>
		/// Returns the rotation to apply to the player after scene transition.
		/// </summary>
		public float ReturnNewPlayerRot() => _newPlayerRot;

		#endregion
	}
}
