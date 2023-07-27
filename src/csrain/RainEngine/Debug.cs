
namespace RainEngine
{
	public static class Debug
	{
		public static void Log(string message) =>
			RainNative.Interop.Debug_Log(message);
	}
}
