
using System;
using System.Numerics;

namespace RainEngine
{
	public class Window
	{
		internal IntPtr _Handle { get; }
		public Input Input => new(_Handle);

		internal Window(IntPtr handle) { this._Handle = handle; }

		public static Window Active => Engine.ActiveWindow;

		public string Title
		{
			get => RainNative.Interop.Window_GetTitle(_Handle);
			set => RainNative.Interop.Window_SetTitle(_Handle, value);
		}

		public Vector2 Size
		{
			get
			{
				Vector2 r;
				RainNative.Interop.Window_GetFramebufferSize(_Handle, out r);
				return r;
			}
			set => RainNative.Interop.Window_SetFramebufferSize(_Handle, ref value);
		}

		public float AspectRatio => Size.X / Size.Y;
	}
}
