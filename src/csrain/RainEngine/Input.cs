using System;
using System.Numerics;

namespace RainEngine
{
	public class Input
	{
		private IntPtr _WindowHandle;
		internal Input(IntPtr windowHandle)
		{
			this._WindowHandle = windowHandle;
		}

		public bool IsKeyDown(int keycode) =>
			RainNative.Interop.Window_IsKeyDown(_WindowHandle, keycode);

		public static Input Active => Window.Active.Input;
	}
}
