using System.Numerics;

namespace RainEngine
{
	public static class VectorExtensions
	{
		public static Vector2 Normalized(this Vector2 vec) => vec / vec.Length();
		public static Vector3 Normalized(this Vector3 vec) => vec / vec.Length();
		public static Vector4 Normalized(this Vector4 vec) => vec / vec.Length();
		public static Matrix4x4 Transposed(this Matrix4x4 mat) =>
			Matrix4x4.Transpose(mat);
		
		public static Extent2 ToExtent(this Vector2 vec) => new((ulong)vec.X, (ulong)vec.Y);
	}

	public struct Extent2
	{
		public ulong Width, Height;

		public Extent2(ulong width, ulong height) => (Width, Height) = (width, height);
		public Vector2 ToVector() => new(Width, Height);
		public override string ToString() => $"Extent2({Width}x{Height})";
	}

	public struct Rect2
	{
		public ulong X, Y, Width, Height;

		public static Rect2 Zero => new(0, 0, 0, 0);

		public Rect2(ulong x, ulong y, ulong width, ulong height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
	}
}
