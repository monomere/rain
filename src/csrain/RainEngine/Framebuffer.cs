namespace RainEngine
{
	public class Framebuffer
	{
		public Texture ColorTexture { get; }
		public Texture DepthTexture { get; }
		public Extent2 Size { get; }

		public Framebuffer(Extent2 size)
		{
			Size = size;
			ColorTexture = Texture.Create(size, RainNative.SgPixelFormat.DEFAULT, true);
			DepthTexture = Texture.Create(size, RainNative.SgPixelFormat.DEPTH_STENCIL, true);
		}
	}
}
