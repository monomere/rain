using System;
using System.Numerics;

namespace RainEngine
{
	public class RenderPass
	{
		internal IntPtr _Handle { get; }
		public Framebuffer Framebuffer { get; }

		public RenderPass(Framebuffer framebuffer)
		{
			Framebuffer = framebuffer;
			_Handle = RainNative.Interop.RenderPass_Alloc(
				Framebuffer.ColorTexture._Handle,
				Framebuffer.DepthTexture._Handle
			);
		}

		~RenderPass()
		{
			RainNative.Interop.RenderPass_DestroyAndFree(_Handle);
		}
	}
}
