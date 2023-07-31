using System.Numerics;
using RainEngine;

class Player : Component
{
	public float Speed = 1.0f;

	public override void OnCreate()
	{
	}

	public override void OnUpdate(float deltaTime)
	{
		Vector3 move = new(0.0f);
		if (Input.GetKey('A'))
		{
			move.X += -1.0f;
		}
		if (Input.GetKey('D'))
		{
			move.X += +1.0f;
		}

		if (Input.GetKey('S'))
		{
			move.Y += -1.0f;
		}
		if (Input.GetKey('W'))
		{
			move.Y += +1.0f;
		}

		if (move.X != 0 || move.Y != 0)
		{
			Transform!.Position += move.Normalized() * Speed * deltaTime;
		}
	}
}
