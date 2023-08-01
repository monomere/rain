
using System;

namespace RainEngine
{
	interface IApp
	{
		void Entry();
		void Update(float deltaTime);
		void Render();
		void Destroy();
	}

	static class Main
	{
		static private IApp? _App;

		static void Entry()
		{
			RainImGui.Init();
			try
			{
				_App = new Editor();
				_App.Entry();
			}
			catch (Exception e)
			{
				Debug.Log($"EXCEPTION: {e}");
				throw e;
			}
		}

		static void Update(float deltaTime)
		{
			try
			{
				_App!.Update(deltaTime);
			}
			catch (Exception e)
			{
				Debug.Log($"EXCEPTION: {e}");
				throw e;
			}
		}

		static void Render()
		{
			try
			{
				_App!.Render();
			}
			catch (Exception e)
			{
				Debug.Log($"EXCEPTION: {e}");
				throw e;
			}
		}

		static void Destroy()
		{
			try
			{
				_App!.Destroy();
			}
			catch (Exception e)
			{
				Debug.Log($"EXCEPTION: {e}");
				throw e;
			}
			RainImGui.DeInit();
		}
	}
}
