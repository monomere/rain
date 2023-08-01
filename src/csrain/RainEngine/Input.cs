using System;
using System.Numerics;

namespace RainEngine
{
	public class Input
	{
		internal static bool KeyboardCaptured { get; set; }
		// private IntPtr _WindowHandle;
		// internal Input(IntPtr windowHandle)
		// {
		// 	this._WindowHandle = windowHandle;
		// }

		public static bool GetKey(int keycode) =>
			KeyboardCaptured ? false :
			RainNative.Interop.Window_IsKeyDown(Window.Active._Handle, keycode);

		// public static Input Active => Window.Active.Input;
	}
}
