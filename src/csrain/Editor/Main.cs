using System.Numerics;
using RainEngine;
using ImGuiNET;

class Editor : IApp
{
	private Framebuffer _GameFramebuffer;
	private RenderPass _GameRenderPass;
	private EditorGUI _GUI;

	public bool IsPlaying = false;

	public Editor()
	{
		Window.Active.Title = "Rain Engine Editor";

		_GameFramebuffer = new((Window.Active.Size / 2).ToExtent());
		_GameRenderPass = new(_GameFramebuffer);

		AssetManager.Active.LoadAllFromManifestFile("data/manifest.json");
		ReloadScene();
		_GUI = new();
	}

	public void ReloadScene()
	{
		SceneManager.ActiveScene = SceneAsset.BuildFromFile("data/scene1.json");
		// Scene.Active.CreateEntity("Player",
		// 	new TransformComponent(),
		// 	new Player(),
		// 	new SpriteComponent(AssetManager.Active.Get<Texture>(new(1)))
		// );
		Camera.Active.Position = new(0.0f, 0.0f, -3.0f);
	}

	public void Entry()
	{
	}

	private void StartPlaying()
	{
		IsPlaying = true;
		Scene.Active.OnCreate();
	}

	private void StopPlaying()
	{
		IsPlaying = false;
		// Scene.Active.OnDestroy();
		ReloadScene();
	}

	private bool _ViewportWasFocused = false;

	public void Update(float deltaTime)
	{
		if (IsPlaying)
		{
			Input.KeyboardCaptured = !_ViewportWasFocused;
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
		RainImGui.BeginRender();

		if (ImGui.BeginMainMenuBar())
		{
			ImGui.MenuItem("File");
			ImGui.MenuItem("Edit");
			ImGui.EndMainMenuBar();
		}
		
		if (ImGui.Begin("Viewport"))
		{
			_ViewportWasFocused = ImGui.IsWindowFocused();
			var icon = AssetManager.Active.Get<Texture>(
				IsPlaying ? "icons/pause.png" : "icons/play.png"
			)!;
			if (ImGuiUtil.ImageButton("PlayButton", icon, new Vector2(16, 16)))
			{
				if (IsPlaying) StopPlaying();
				else StartPlaying();
			}
			ImGuiUtil.Image(_GameFramebuffer.ColorTexture);
		}
		ImGui.End();

		ImGui.ShowDemoWindow();
		_GUI.Render();

		RainImGui.EndRender();

		Renderer.EndPass();
	}

	public void Destroy()
	{
		SceneAsset.SaveToFile(SceneManager.ActiveScene, "data/scene1.json", true);
	}
}

