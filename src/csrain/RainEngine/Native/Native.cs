using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using RainEngine;

namespace RainNative
{
	internal static class Interop
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Debug_Log(string m);
	
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Renderer_RenderColoredQuad(IntPtr p, ref Vector4 c, ref Matrix4x4 tr);
		
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Renderer_RenderTexturedQuad(
			IntPtr o, // struct rain_renderer *restrict this,
			IntPtr tex, // const struct rain_texture *restrict texture,
			UInt32 samp, // const struct sg_sampler sampler,
			UInt64 offsetX,
			UInt64 offsetY,
			UInt64 width,
			UInt64 height,
			ref Matrix4x4 trans
		);

		internal enum BuiltinSamplerId {
			Nearest = 0,
			Bilinear = 1,
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static UInt32 Renderer_GetBuiltinSampler(
			IntPtr o,
			uint id
		);


		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static IntPtr Renderer_Alloc();
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Renderer_DestoryAndFree(IntPtr o);

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static IntPtr Renderer_BeginPass(
			IntPtr o,
			IntPtr pass,
			bool clear,
			ref Vector4 color
		);
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static IntPtr Renderer_BeginDefaultPass(
			IntPtr o,
			bool clear,
			ref Vector4 color
		);
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Renderer_EndPass(IntPtr o);

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static IntPtr RenderPass_Alloc(IntPtr color, IntPtr depthStencil);
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void RenderPass_DestroyAndFree(IntPtr o);
		
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static IntPtr Engine_GetWindow();

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Window_SetTitle(IntPtr o, string v);
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static string Window_GetTitle(IntPtr o);

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Window_SetFramebufferSize(IntPtr o, ref Vector2 v);
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static string Window_GetFramebufferSize(IntPtr o, out Vector2 v);

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static bool Window_IsKeyDown(IntPtr o, int keycode);

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static IntPtr Texture_Alloc();

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Texture_DestroyAndFree(IntPtr o);

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Texture_FromFile(
			IntPtr o,
			string path,
			int format,
			int usage
		);

		/// NB: DO NOT CHANGE THIS STRUCT!
		public struct TextureDesc {
			public bool IsRenderTarget;
			public Extent2 Dimensions;
			public uint SampleCount;
			public uint PixelFormat;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Texture_Init(IntPtr o, ref TextureDesc desc);

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void Texture_GetSize(IntPtr o, out Extent2 e);

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static int Texture_GetFormat(IntPtr o);
	}

	// copied from sokol jul 26 2023
	public enum SgUsage {
		DEFAULT = 0,
		IMMUTABLE = 1,
		DYNAMIC = 2,
		STREAM = 3,
	}

	// copied from sokol jul 26 2023
	public enum SgPixelFormat {
		DEFAULT = 0,
		NONE,

		R8,
		R8SN,
		R8UI,
		R8SI,

		R16,
		R16SN,
		R16UI,
		R16SI,
		R16F,
		RG8,
		RG8SN,
		RG8UI,
		RG8SI,

		R32UI,
		R32SI,
		R32F,
		RG16,
		RG16SN,
		RG16UI,
		RG16SI,
		RG16F,
		RGBA8,
		SRGB8A8,
		RGBA8SN,
		RGBA8UI,
		RGBA8SI,
		BGRA8,
		RGB10A2,
		RG11B10F,

		RG32UI,
		RG32SI,
		RG32F,
		RGBA16,
		RGBA16SN,
		RGBA16UI,
		RGBA16SI,
		RGBA16F,

		RGBA32UI,
		RGBA32SI,
		RGBA32F,

		DEPTH,
		DEPTH_STENCIL,

		BC1_RGBA,
		BC2_RGBA,
		BC3_RGBA,
		BC4_R,
		BC4_RSN,
		BC5_RG,
		BC5_RGSN,
		BC6H_RGBF,
		BC6H_RGBUF,
		BC7_RGBA,
		PVRTC_RGB_2BPP,
		PVRTC_RGB_4BPP,
		PVRTC_RGBA_2BPP,
		PVRTC_RGBA_4BPP,
		ETC2_RGB8,
		ETC2_RGB8A1,
		ETC2_RGBA8,
		ETC2_RG11,
		ETC2_RG11SN,
	}
}
