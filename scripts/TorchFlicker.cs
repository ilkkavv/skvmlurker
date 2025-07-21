using Godot;

namespace DungeonCrawler
{
	/// <summary>
	/// Simulates a flickering torch light effect by modifying light energy over time.
	/// </summary>
	public partial class TorchFlicker : OmniLight3D
	{
		#region Exported Settings

		[Export] private float _baseEnergy = 3.0f;
		[Export] private float _flickerSpeed = 15.0f;
		[Export] private float _flickerAmount = 0.4f;

		#endregion

		#region Private Fields

		private float _time = 0.0f;

		#endregion

		#region Light Flicker

		/// <summary>
		/// Updates the light's energy every frame to simulate flickering.
		/// </summary>
		public override void _Process(double delta)
		{
			// Advance time
			_time += (float)delta * _flickerSpeed;

			// Generate pseudo-random noise using sine and cosine functions
			float noise = Mathf.Sin(_time * 3.0f) + Mathf.Cos(_time * 2.1f);

			// Add some randomness
			float random = (float)GD.RandRange(-1.0f, 1.0f);

			// Apply energy flickering effect
			LightEnergy = _baseEnergy + (noise + random) * _flickerAmount * 0.5f;
		}

		#endregion
	}
}
