namespace RainEngine
{
	public class Framebuffer
	{
		public Texture ColorTexture { get; }
		public Texture? DepthTexture { get; }
		public Extent2 Size { get; }

		public Framebuffer(Extent2 size, bool depth = false)
		{
			Size = size;
			ColorTexture = Texture.Create(size, RainNative.SgPixelFormat.DEFAULT, true);
			if (depth)
			{
				DepthTexture = Texture.Create(size, RainNative.SgPixelFormat.DEPTH, true);
			}
		}
	}
}
