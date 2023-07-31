using System;
using System.Numerics;
using ImGuiNET;

namespace RainEngine {
	public class ImGUI {
		public ImGUI() {
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

		~ImGUI() {
			RainNative.Interop.ImGUI_DeInit();
		}

		public bool Begin(string name) {
			bool ignore = true;
			return RainNative.Interop.ImGUI_Begin(name, ref ignore);
		}

		public void End() {
			RainNative.Interop.ImGUI_End();
		}

		public bool Button(string label) {
			return RainNative.Interop.ImGUI_Button(label);
		}

		public void Label(string label) {
			RainNative.Interop.ImGUI_Label(label);
		}

		public void Image(Texture texture, Vector2 size) {
			RainNative.Interop.ImGUI_Image(
				texture._Handle,
				size.X, size.Y
			);
		}

		public void Demo() {
			RainNative.Interop.ImGUI_Demo();
		}

		public void BeginRender() {
			RainNative.Interop.ImGUI_BeginRender();
		}

		public void EndRender() {
			RainNative.Interop.ImGUI_EndRender();
		}

		public void Drag(string label, ref float value)
		{ RainNative.Interop.ImGUI_DragFloat(label, ref value); }
		public void Drag(string label, ref Vector2 value)
		{ RainNative.Interop.ImGUI_DragFloat2(label, ref value); }
		public void Drag(string label, ref Vector3 value)
		{ RainNative.Interop.ImGUI_DragFloat3(label, ref value); }
		public void Drag(string label, ref Vector4 value)
		{ RainNative.Interop.ImGUI_DragFloat4(label, ref value); }
		public void Drag(string label, ref Quaternion value)
		{ RainNative.Interop.ImGUI_DragFloat4q(label, ref value); }
		public void Input(string label, ref float value)
		{ RainNative.Interop.ImGUI_InputFloat(label, ref value); }
		public void Input(string label, ref Vector2 value)
		{ RainNative.Interop.ImGUI_InputFloat2(label, ref value); }
		public void Input(string label, ref Vector3 value)
		{ RainNative.Interop.ImGUI_InputFloat3(label, ref value); }
		public void Input(string label, ref Vector4 value)
		{ RainNative.Interop.ImGUI_InputFloat4(label, ref value); }
		public void Slider(string label, ref float value, float min, float max)
		{ RainNative.Interop.ImGUI_SliderFloat(label, ref value, min, max); }
		public void Slider(string label, ref Vector2 value, float min, float max)
		{ RainNative.Interop.ImGUI_SliderFloat2(label, ref value, min, max); }
		public void Slider(string label, ref Vector3 value, float min, float max)
		{ RainNative.Interop.ImGUI_SliderFloat3(label, ref value, min, max); }
		public void Slider(string label, ref Vector4 value, float min, float max)
		{ RainNative.Interop.ImGUI_SliderFloat4(label, ref value, min, max); }

		public bool IsItemClicked()
		{ return RainNative.Interop.ImGUI_IsItemClicked(); }

		public bool TreeNode(
			string label,
			ref bool selected,
			object id
		) {
			return RainNative.Interop.ImGUI_TreeNode(id, ref selected, label);
		}

		public void TreePop() {
			RainNative.Interop.ImGUI_TreePop();
		}
	}
}
