using System;
using System.Numerics;

namespace RainEngine
{
	public class Input
	{
		// private IntPtr _WindowHandle;
		// internal Input(IntPtr windowHandle)
		// {
		// 	this._WindowHandle = windowHandle;
		// }

		public static bool GetKey(int keycode) =>
			RainNative.Interop.Window_IsKeyDown(Window.Active._Handle, keycode);

		// public static Input Active => Window.Active.Input;
	}
}
