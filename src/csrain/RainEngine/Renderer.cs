using System;
using System.Numerics;

namespace RainEngine
{

	public static class Renderer
	{
		public static void RenderColoredQuad(Vector4 color, Matrix4x4 transform) =>
			RainNative.Interop.Renderer_RenderColoredQuad(ref color, ref transform);

		public static void BeginPass(RenderPass renderPass, Vector4? clearColor)
		{
			Vector4 color = clearColor ?? new();
			RainNative.Interop.Renderer_BeginPass(renderPass._Handle, clearColor != null, ref color);
		}

		public static void BeginPass(Vector4? clearColor)
		{
			Vector4 color = clearColor ?? new();
			RainNative.Interop.Renderer_BeginDefaultPass(clearColor != null, ref color);
		}

		public static void EndPass()
		{
			RainNative.Interop.Renderer_EndPass();
		}

		public static void RenderTexturedQuad(
			Texture texture,
			Rect2 rect,
			Vector4 tint,
			Matrix4x4 transform
		)
		{
			var rectNative = new RainNative.Interop.Renderer_Rect
			{
				OffsetX = rect.X,
				OffsetY = rect.Y,
				Width = rect.Width,
				Height = rect.Height
			};

			RainNative.Interop.Renderer_RenderTexturedQuad(
				texture._Handle,
				RainNative.Interop.Renderer_GetBuiltinSampler(
					(uint)RainNative.Interop.BuiltinSamplerId.Nearest
				),
				ref rectNative,
				ref tint,
				ref transform
			);
		}

		// public static Renderer? Active => Engine.ActiveRenderer;
	}

}
