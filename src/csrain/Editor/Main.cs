using System.Numerics;
using RainEngine;
using ImGuiNET;

class Editor : IApp
{
	private Framebuffer _GameFramebuffer;
	private RenderPass _GameRenderPass;
	private ImGUI _ImGUI;
	private EditorGUI _GUI;

	public bool IsPlaying = false;

	public Editor()
	{
		Window.Active.Title = "Rain Engine Editor";

		_GameFramebuffer = new((Window.Active.Size / 2).ToExtent());
		_GameRenderPass = new(_GameFramebuffer);

		AssetManager.Active.LoadAllFromManifestFile("data/manifest.json");
		SceneManager.ActiveScene = SceneAsset.BuildFromFile("data/scene1.json");
		// Scene.Active.CreateEntity("Player",
		// 	new TransformComponent(),
		// 	new Player(),
		// 	new SpriteComponent(AssetManager.Active.Get<Texture>(new(1)))
		// );
		var transform = Scene.Active.Entities[1].GetComponent<TransformComponent>()!;
		// Debug.Log($"{transform._Clean}");
		Camera.Active.Position = new(0.0f, 0.0f, -3.0f);
		_ImGUI = new();
		_GUI = new(_ImGUI);
	}

	public void Entry()
	{
	}

	private void StartPlaying()
	{
		Scene.Active.OnCreate();
	}

	private void StopPlaying()
	{
		// Scene.Active.OnDestroy();
	}

	public void Update(float deltaTime)
	{
		if (IsPlaying)
		{
			Scene.Active.OnUpdate(deltaTime);
		}
	}

	private bool _ShouldRender = true;
	public void Render()
	{
		Renderer.BeginPass(_GameRenderPass, new(0.1f, 0.2f, 0.3f, 1.0f));
		Scene.Active.OnRender();
		Renderer.EndPass();

		Renderer.BeginPass(new(0.5f, 0.4f, 0.3f, 1.0f));
		_ImGUI.BeginRender();
		
		if (_ImGUI.Begin("Viewport"))
		{
			_ImGUI.Image(
				_GameFramebuffer.ColorTexture,
				_GameFramebuffer.ColorTexture.Size.ToVector()
			);
		}
		_ImGUI.End();

		_GUI.Render();
		_ImGUI.Demo();

		if (ImGui.Begin("Hello"))
		{
			ImGui.Text("what");
		}
		ImGui.End();

		_ImGUI.EndRender();

		Renderer.EndPass();
	}

	public void Destroy()
	{
		SceneAsset.SaveToFile(SceneManager.ActiveScene, "data/scene1.json", true);
	}
}

