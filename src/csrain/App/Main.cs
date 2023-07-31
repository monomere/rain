using System.Numerics;
using RainEngine;

class Game : IApp
{
	public Game()
	{
		Window.Active.Title = "Mokosh";
		// Camera.Active!.Position = new(0.0f, 0.0f, -3.0f);
	}

	public void Entry()
	{

	}

	public void Update(float deltaTime)
	{
	}

	public void Render()
	{
		// Renderer.Active.RenderTexturedQuad(
		// 	_Texture,
		// 	new(),
		// 	Camera.Active.ComputeTransformMatrix(
		// 		Matrix4x4.CreateTranslation(_Position)
		// 	)
		// );
	}

	public void Destroy()
	{
		throw new System.NotImplementedException();
	}
}

