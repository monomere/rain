using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace RainEngine
{
	public enum TextureFormat
	{
		Unknown = 0,
		Grey = 1,
		GreyAlpha = 2,
		RGB = 3,
		RGBA = 4
	}

	public class Texture
	{
		[JsonIgnore] public Extent2 Size { get; }
		[JsonIgnore] public TextureFormat Format { get; }

		public AssetID AssetID { get; }

		internal IntPtr _Handle { get; }

		[JsonConstructor]
		internal Texture(AssetID assetID, IntPtr handle, Extent2 size, TextureFormat format)
		{
			_Handle = handle;
			AssetID = assetID;
			Size = size;
			Format = (TextureFormat)format;
		}

		[JsonConstructor]
		public Texture(AssetID assetID, TextureFormat format)
		{
			AssetID = assetID;
		}

		~Texture()
		{
			RainNative.Interop.Texture_DestroyAndFree(_Handle);
		}

		public static Texture Create(
			Extent2 size,
			RainNative.SgPixelFormat pixelFormat,
			bool renderTarget = false
		)
		{
			var desc = new RainNative.Interop.TextureDesc
			{
				Dimensions = size,
				IsRenderTarget = renderTarget,
				PixelFormat = (uint)pixelFormat,
				SampleCount = 1
			};

			IntPtr handle = RainNative.Interop.Texture_Alloc();
			RainNative.Interop.Texture_Init(handle, ref desc);
			return new(AssetID.Empty, handle, size, TextureFormat.Unknown); // TODO: make texture format correct.
		}

		public static Texture FromFile(AssetID assetID, string path, TextureFormat format, bool dynamic = false)
		{
			IntPtr handle = RainNative.Interop.Texture_Alloc();

			RainNative.Interop.Texture_FromFile(
				handle, path,
				(int)format,
				(int)(dynamic ? RainNative.SgUsage.DYNAMIC : RainNative.SgUsage.IMMUTABLE)
			);

			Extent2 size;
			RainNative.Interop.Texture_GetSize(handle, out size);

			int actualFormat = RainNative.Interop.Texture_GetFormat(handle);

			return new(assetID, handle, size, (TextureFormat)actualFormat);
		}
	}
}
