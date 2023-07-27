using System.Numerics;
using RainEngine;

class Editor : IApp
{
	private Renderer _Renderer;

	private Framebuffer _GameFramebuffer;
	private RenderPass _GameRenderPass;
	private ImGUI _ImGUI;

	public bool IsPlaying;
	public Scene CurrentScene;

	public Editor()
	{
		Window.Active.Title = "Rain Engine Editor";

		_Renderer = new();
		_GameFramebuffer = new((Window.Active.Size / 2).ToExtent(), false);
		_GameRenderPass = new(_GameFramebuffer);
		CurrentScene = new Scene();
		_ImGUI = new();

		Engine.ActiveRenderer = _Renderer;
	}

	public void Entry()
	{
		// ImGui.Begin();

	}

	public void StartScene()
	{
	}

	public void Update(float deltaTime)
	{

	}

	public void Render()
	{
		if (IsPlaying)
		{
			_Renderer.BeginPass(_GameRenderPass, new(1.0f, 2.0f, 3.0f, 0.0f));
			CurrentScene.OnRender();
			_Renderer.EndPass();
		}

		_Renderer.BeginPass(new(0.5f, 0.4f, 0.3f, 0.1f));
		_ImGUI.BeginRender();
		
		if (_ImGUI.Begin("Viewport"))
		{
			_ImGUI.Image(
				_GameFramebuffer.ColorTexture,
				_GameFramebuffer.ColorTexture.Size
			);
		} _ImGUI.End();

		_ImGUI.EndRender();
		_Renderer.EndPass();
	}
}

