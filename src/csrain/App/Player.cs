using System.Numerics;
using RainEngine;

class Player : Component
{
	private TransformComponent _Transform = new();
	private Vector4 _Color = new(0.7f, 0.8f, 0.4f, 1.0f);
	private float _Speed = 1.0f;
	private Texture? _Texture;

	public override void OnCreate()
	{
		AddComponent<TransformComponent>(_Transform);
		_Texture = Texture.FromFile("data/texture.png", TextureFormat.RGBA);
	}

	public override void OnUpdate(float deltaTime)
	{
		Vector3 move = new(0.0f);
		if (Input.Active.IsKeyDown('A'))
		{
			move.X += -1.0f;
		}
		if (Input.Active.IsKeyDown('D'))
		{
			move.X += +1.0f;
		}

		if (Input.Active.IsKeyDown('S'))
		{
			move.Y += -1.0f;
		}
		if (Input.Active.IsKeyDown('W'))
		{
			move.Y += +1.0f;
		}

		if (move.X != 0 || move.Y != 0)
		{
			_Transform.Position += move.Normalized() * _Speed * deltaTime;
		}
	}
}
