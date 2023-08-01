using System;
using System.Numerics;
using ImGuiNET;

namespace RainEngine {
	public static class RainImGui
	{
		internal static void Init()
		{
			unsafe
			{
				RainNative.Interop.ImGUI_Data data;
				RainNative.Interop.ImGUI_Init(1 << 16, &data);
				ImGui.SetCurrentContext(data.Context);
				ImGui.SetAllocatorFunctions(data.AllocFunc, data.FreeFunc);
				ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
				Debug.Log($"ImGui.NET version: {ImGui.GetVersion()}");
			}
		}

		internal static void DeInit() => RainNative.Interop.ImGUI_DeInit();

		public static void BeginRender() => RainNative.Interop.ImGUI_BeginRender();
		public static void EndRender() => RainNative.Interop.ImGUI_EndRender();
	}

	public static class ImGuiUtil
	{
		// NB: Defaults from ImGui v1.89.7-docking
		public static bool DragFloatQ(
			string label,
			ref Quaternion v,
			float v_speed = 1.0f,
			float v_min = 0.0f,
			float v_max = 0.0f,
			string format = "%.3f",
			ImGuiSliderFlags flags = 0
		)
		{
			Vector4 t = v.ToVector4();
			var res = ImGui.DragFloat4(label, ref t,
				v_speed, v_min, v_max, format, flags);
			v = t.ToQuaternion();
			return res;
		}
		public static bool SliderFloatQ(
			string label,
			ref Quaternion v,
			float v_min,
			float v_max,
			string format = "%.3f",
			ImGuiSliderFlags flags = 0
		)
		{
			Vector4 t = v.ToVector4();
			var res = ImGui.SliderFloat4(label, ref t,
				v_min, v_max, format, flags);
			v = t.ToQuaternion();
			return res;
		}
		public static bool InputFloatQ(
			string label,
			ref Quaternion v,
			string format = "%.3f",
			ImGuiInputTextFlags flags = 0
		)
		{
			Vector4 t = v.ToVector4();
			var res = ImGui.InputFloat4(label, ref t, format, flags);
			v = t.ToQuaternion();
			return res;
		}
		
		public static void Image(Texture texture) =>
			Image(texture, texture.Size.ToVector());

		public static void Image(Texture texture, Vector2 size) =>
			Image(texture, size, new(0, 1), new(1, 0));

		public static void Image(
			Texture texture,
			Vector2 size,
			Vector2 uv0,
			Vector2 uv1
		) => Image(texture, size, uv0, uv1, new(1, 1, 1, 1));

		public static void Image(
			Texture texture,
			Vector2 size,
			Vector2 uv0,
			Vector2 uv1,
			Vector4 tint_col
		) => Image(texture, size, uv0, uv1, tint_col, new(0, 0, 0, 0));
		
		public static void Image(
			Texture texture,
			Vector2 size,
			Vector2 uv0,
			Vector2 uv1,
			Vector4 tint_col,
			Vector4 border_col
		) => ImGui.Image(texture._Handle, size, uv0, uv1, tint_col, border_col);
		
		public static bool ImageButton(
			string str_id,
			Texture texture
		) => ImageButton(str_id, texture, texture.Size.ToVector());

		public static bool ImageButton(
			string str_id,
			Texture texture,
			Vector2 size
		) => ImageButton(str_id, texture, size, new(0, 1), new(1, 0));
		
		public static bool ImageButton(
			string str_id,
			Texture texture,
			Vector2 size,
			Vector2 uv0,
			Vector2 uv1
		) => ImageButton(str_id, texture, size, uv0, uv1, new(0, 0, 0, 0));

		public static bool ImageButton(
			string str_id,
			Texture texture,
			Vector2 size,
			Vector2 uv0,
			Vector2 uv1,
			Vector4 bg_col
		) => ImageButton(str_id, texture, size, uv0, uv1, bg_col, new(1, 1, 1, 1));

		public static bool ImageButton(
			string str_id,
			Texture texture,
			Vector2 size,
			Vector2 uv0,
			Vector2 uv1,
			Vector4 bg_col,
			Vector4 tint_col
		) => ImGui.ImageButton(str_id, texture._Handle, size, uv0, uv1, bg_col, tint_col);

		public static bool ImageButton(string str_id, Texture texture, float thumbnailSize) =>
			ImageButton(
				str_id,
				texture,
				new Vector2(
					thumbnailSize * (texture.Size.Width /(float) texture.Size.Height),
					thumbnailSize
				)
			);
	}
}
