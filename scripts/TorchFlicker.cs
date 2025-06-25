using Godot;
using System;

public partial class TorchFlicker : OmniLight3D
{
	private float baseEnergy = 3.0f;
	private float flickerSpeed = 15.0f;
	private float flickerAmount = 0.4f;
	private float time = 0.0f;

	public override void _Process(double delta)
	{
		time += (float)delta * flickerSpeed;
		float noise = Mathf.Sin(time * 3.0f) + Mathf.Cos(time * 2.1f);
		float random = (float)GD.RandRange(-1.0f, 1.0f);

		this.LightEnergy = baseEnergy + noise * flickerAmount * 0.5f + random * flickerAmount * 0.5f;
	}
}
