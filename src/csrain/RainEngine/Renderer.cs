using System;
using System.Numerics;

namespace RainEngine
{

	public class Renderer
	{
		private IntPtr _Handle;

		internal Renderer()
		{
			_Handle = RainNative.Interop.Renderer_Alloc();
		}

		~Renderer()
		{
			RainNative.Interop.Renderer_DestoryAndFree(_Handle);
		}

		public Camera? ActiveCamera;

		public void RenderColoredQuad(Vector4 color, Matrix4x4 transform) =>
			RainNative.Interop.Renderer_RenderColoredQuad(_Handle, ref color, ref transform);

		public void BeginPass(RenderPass renderPass, Vector4? clearColor)
		{
			Vector4 color = clearColor ?? new();
			RainNative.Interop.Renderer_BeginPass(_Handle, renderPass._Handle, clearColor != null, ref color);
		}

		public void BeginPass(Vector4? clearColor)
		{
			Vector4 color = clearColor ?? new();
			RainNative.Interop.Renderer_BeginDefaultPass(_Handle, clearColor != null, ref color);
		}

		public void EndPass()
		{
			RainNative.Interop.Renderer_EndPass(_Handle);
		}

		public void RenderTexturedQuad(
			Texture texture,
			Rect2 rect,
			Matrix4x4 transform
		)
		{
			RainNative.Interop.Renderer_RenderTexturedQuad(
				_Handle,
				texture._Handle,
				RainNative.Interop.Renderer_GetBuiltinSampler(
					_Handle,
					(uint)RainNative.Interop.BuiltinSamplerId.Nearest
				),
				rect.X, rect.Y, rect.Width, rect.Height,
				ref transform
			);
		}

		public static Renderer? Active => Engine.ActiveRenderer;
	}

}
