using System;
using System.Numerics;

namespace RainEngine
{
	public static class Engine
	{
		public static Window ActiveWindow { get; }
		public static Renderer? ActiveRenderer;

		static Engine()
		{
			ActiveWindow = new(RainNative.Interop.Engine_GetWindow());
		}
	}
}
