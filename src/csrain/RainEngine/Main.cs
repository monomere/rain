
namespace RainEngine
{
	interface IApp
	{
		void Entry();
		void Update(float deltaTime);
		void Render();
	}

	static class Main
	{
		static private IApp? _App;

		static void Entry()
		{
			var cam = Renderer.Active!.ActiveCamera!;
			cam.Position = new(5.0f, 0.0f, -3.0f);
			cam.VerticalSize = 10.0f;

			_App = new Editor();
		}

		static void Update(float deltaTime)
		{
			_App!.Update(deltaTime);
		}

		static void Render()
		{
			_App!.Render();
		}
	}
}
