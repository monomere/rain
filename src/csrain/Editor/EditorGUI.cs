using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using ImGuiNET;
using RainEngine;

static class GUIUtils
{
	public static string CamelToTitle(string text, bool preserveAcronyms = true)
	{
		if (string.IsNullOrWhiteSpace(text)) return "";

		StringBuilder newText = new(text.Length * 2);

		newText.Append(text[0]);

		for (int i = 1; i < text.Length; i++)
		{
			if (char.IsUpper(text[i]))
				if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
						(preserveAcronyms && char.IsUpper(text[i - 1]) && 
						i < text.Length - 1 && !char.IsUpper(text[i + 1])))
						newText.Append(' ');
			newText.Append(text[i]);
		}

		return newText.ToString();
	}

}

class ValueMember
{
	public readonly MemberInfo Info;
	public ValueMember(MemberInfo info) => Info = info;

	public string Name => Info.Name;

	public object GetValue(object obj) => Info switch
	{
		FieldInfo fieldInfo => fieldInfo.GetValue(obj),
		PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
		_ => throw new InvalidOperationException()
	};

	public void SetValue(object obj, object value)
	{
		switch (Info)
		{
			case FieldInfo fieldInfo: fieldInfo.SetValue(obj, value); break;
			case PropertyInfo propertyInfo: propertyInfo.SetValue(obj, value); break;
			default: throw new InvalidOperationException();
		};
	}

	public bool IsReadable => Info switch
	{
		FieldInfo fieldInfo => fieldInfo.IsPublic,
		PropertyInfo propertyInfo => propertyInfo.CanRead && propertyInfo.GetGetMethod(true).IsPublic,
		_ => throw new InvalidOperationException()
	};

	public bool IsWriteable => Info switch
	{
		FieldInfo fieldInfo => !fieldInfo.IsInitOnly,
		PropertyInfo propertyInfo => propertyInfo.CanWrite && propertyInfo.GetSetMethod(true).IsPublic,
		_ => throw new InvalidOperationException()
	};

	public Type ValueType => Info switch
	{
		FieldInfo fieldInfo => fieldInfo.FieldType,
		PropertyInfo propertyInfo => propertyInfo.PropertyType,
		_ => throw new InvalidOperationException()
	};
}

public class EditorGUI {
	private Entity? _Selected;

	public static float DragSpeed = 0.3f;
	public static string NumericFormat = "%.3f";

	private static void DragFloat(ValueMember m, ref float value)
	{
		ImGui.DragFloat(GUIUtils.CamelToTitle(m.Name), ref value, DragSpeed, 0.0f, 0.0f, NumericFormat);
	}

	private static void DragFloat(ValueMember m, ref Vector2 value)
	{
		ImGui.DragFloat2(GUIUtils.CamelToTitle(m.Name), ref value, DragSpeed, 0.0f, 0.0f, NumericFormat);
	}

	private static void DragFloat(ValueMember m, ref Vector3 value)
	{
		ImGui.DragFloat3(GUIUtils.CamelToTitle(m.Name), ref value, DragSpeed, 0.0f, 0.0f, NumericFormat);
	}

	private static void DragFloat(ValueMember m, ref Vector4 value)
	{
		ImGui.DragFloat4(GUIUtils.CamelToTitle(m.Name), ref value, DragSpeed, 0.0f, 0.0f, NumericFormat);
	}

	private static void DragFloat(ValueMember m, ref Quaternion value)
	{
		ImGuiUtil.DragFloatQ(GUIUtils.CamelToTitle(m.Name), ref value, DragSpeed, 0.0f, 0.0f, NumericFormat);
	}

	private Asset<Texture>? _OpenTextureSelector()
	{
		Asset<Texture>? ret = null;
		if (ImGui.BeginPopupModal("Texture Selector"))
		{
			if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();
			if (ImGui.Button("Empty"))
			{
				ret = new(new(0));
				ImGui.CloseCurrentPopup();
			}
			
			foreach (var asset in AssetManager.Active.Assets)
			{
				if (ImGui.Button(asset.Value.Name))
				{
					ret = new(new(asset.Key));
					ImGui.CloseCurrentPopup();
				}
			}
			ImGui.EndPopup();
		}
		return ret;
	}

	private void _TextureAssetMemberEditor(ValueMember m, ref Asset<Texture> value, object _)
	{
		bool clicked;
		if (value.Get() != null)
		{
			clicked = ImGuiUtil.ImageButton($"{m.Name}_ImageButton", value.Get()!, 64.0f);
		}
		else
		{
			clicked = ImGui.Button("No Texture");
		}

		if (clicked) ImGui.OpenPopup("Texture Selector");

		{
			var selected = _OpenTextureSelector();
			if (selected != null) value = selected;
		}
		
		if (ImGui.BeginDragDropTarget())
		{
			var payload = ImGui.AcceptDragDropPayload("AssetBrowserItem");
			unsafe
			{
				if (payload.NativePtr != null)
				{
					var newId = *(ulong*)payload.Data;
					var newAssetType = AssetManager.Active.Assets[newId].Data.GetType();
					if (value.GetType().GenericTypeArguments[0].IsAssignableFrom(newAssetType))
						value = new(new(newId));
				}
			}
			ImGui.EndDragDropTarget();
		}
		int id = (int)value.ID.Raw;
		if (id != 0) ImGui.InputInt(GUIUtils.CamelToTitle(m.Name), ref id);
		if (AssetManager.Active.Assets.ContainsKey((ulong)id))
			value = new(new((ulong)id));
	}

	public EditorGUI()
	{
		AddMemberEditor<float>((ValueMember m, ref float value, object _) => DragFloat(m, ref value));
		AddMemberEditor<Vector2>((ValueMember m, ref Vector2 value, object _) => DragFloat(m, ref value));
		AddMemberEditor<Vector3>((ValueMember m, ref Vector3 value, object _) => DragFloat(m, ref value));
		AddMemberEditor<Vector4>((ValueMember m, ref Vector4 value, object _) => DragFloat(m, ref value));
		AddMemberEditor<Quaternion>((ValueMember m, ref Quaternion value, object _) => DragFloat(m, ref value));
		AddMemberEditor<Asset<Texture>>(_TextureAssetMemberEditor);
	}

	private void RenderWindow(string name, Action content) {
		if (ImGui.Begin(name)) content();
		ImGui.End();
	}

	public void Render() {
		RenderWindow("Inspector", Inspector);
		RenderWindow("Scene", EntityList);
		RenderWindow("Asset Browser", AssetBrowser);
	}

	private void EntityList()
	{
		foreach (var entity in Scene.Active.Entities) {
			bool isSelected = (_Selected == entity);

			bool isOpen = ImGui.TreeNodeEx(
				new IntPtr(entity.Id),
				isSelected ? ImGuiTreeNodeFlags.Selected : 0,
				entity.Name
			);

			if (ImGui.IsItemClicked())
			{
				_Selected = entity;
				Debug.Log("Selected new entity");
				// _SelectedComponent = null;
			}

			if (isOpen)
			{
				ImGui.TreePop();
			}
		}
	}


	private bool _FileLikeIconButton(
		Texture thumbnail,
		string name,
		bool dragDrop = false,
		string payloadType = "",
		IntPtr payloadData = default(IntPtr),
		uint payloadSize = 0
	)
	{
		ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));

		bool res = false;
		ImGuiUtil.ImageButton($"{name}_ImageButton",
			thumbnail, AssetBrowserThumbnailSize);

		if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
		{
			res = true;
		}

		ImGui.PopStyleColor();

		if (dragDrop && ImGui.BeginDragDropSource())
		{
			ImGui.SetDragDropPayload(payloadType, payloadData, payloadSize);
			ImGui.EndDragDropSource();
		}

		ImGui.TextWrapped($"{name}");
		ImGui.NextColumn();

		return res;
	}

	private int AssetBrowserPadding = 16;
	private int AssetBrowserThumbnailSize = 72;

	private string AssetBrowserCurrentPath = "";

	private void AssetBrowser()
	{
		float cellSize = AssetBrowserThumbnailSize + AssetBrowserPadding;

		float panelWidth = ImGui.GetContentRegionAvail().X;
		int columnCount = (int)(panelWidth / cellSize);
		if (columnCount < 1) columnCount = 1;


		HashSet<string> directories = new();

		foreach (var assetName in AssetManager.Active.AssetNames)
		{
			string dirname = Path.GetDirectoryName(assetName.Key);
			if (directories.Contains(dirname) || dirname == "") continue;
			string parentDirname = Path.GetDirectoryName(dirname);
			if (parentDirname != AssetBrowserCurrentPath) continue;
			directories.Add(dirname);
		}

		ImGui.Columns(columnCount, "#AssetBrowser", false);

		if (AssetBrowserCurrentPath != "")
			if (_FileLikeIconButton(
				AssetManager.Active.Get<Texture>("icons/folder.png")!,
				".."
			))
			{
				AssetBrowserCurrentPath = Path.GetDirectoryName(AssetBrowserCurrentPath);
				return;
			}

		foreach (var directory in directories)
		{
			if (_FileLikeIconButton(
				AssetManager.Active.Get<Texture>("icons/folder.png")!,
				directory
			))
			{
				AssetBrowserCurrentPath = directory;
				return;
			}
		}

		foreach (var asset in AssetManager.Active.Assets)
		{
			string dirname = Path.GetDirectoryName(asset.Value.Name);
			string filename = Path.GetFileName(asset.Value.Name);
			if (dirname != AssetBrowserCurrentPath) continue;

			Texture thumbnail;
			if (asset.Value.Data.GetType() == typeof(Texture))
			{
				thumbnail = (Texture)asset.Value.Data;
			}
			else
			{
				thumbnail = AssetManager.Active.Get<Texture>("icons/image.png")!;
			}

			IntPtr data;
			unsafe
			{
				ulong id = asset.Key;
				data = new(&id);
			}

			_FileLikeIconButton(
				thumbnail,
				filename,
				true,
				"AssetBrowserItem",
				data,
				sizeof(ulong)
			);
		}

		// ImGui.SliderInt("Thumbnail Size", ref assetBrowserThumbnailSize, 16, 512);
		// ImGui.SliderInt("Padding", ref assetBrowserPadding, 0, 32);
	}

	private void _MemberEditor(ValueMember valueMember, object component)
	{
		if (!_MemberEditors.ContainsKey(valueMember.ValueType)) return;
		ImGui.BeginDisabled(!valueMember.IsWriteable);
		_MemberEditors[valueMember.ValueType](valueMember, component);
		ImGui.EndDisabled();
	}

	private Dictionary<Type, Action<ValueMember, object>> _MemberEditors = new();

	delegate void MemberEditor<T>(ValueMember member, ref T value, object component);

	void AddMemberEditor<T>(MemberEditor<T> editor) =>
		_MemberEditors.Add(typeof(T), (valueMember, component) =>
		{
			var value = (T)valueMember.GetValue(component);

			editor(valueMember, ref value, component);
			
			if (valueMember.IsWriteable) valueMember.SetValue(component, value!);
		});

	private void Inspector()
	{
		if (_Selected?.Scene != Scene.Active) _Selected = null;
		if (_Selected != null) {
			ImGui.Text($"Entity #{_Selected.Id}: {_Selected.Name}");

			for (int i = 0; i < _Selected.Components.Count; ++i) {
				var flags = ImGuiTreeNodeFlags.DefaultOpen;

				var component = _Selected.Components[i];
				// bool isSelected = (_SelectedComponent == i);

				// if (isSelected) flags |= ImGuiTreeNodeFlags.Selected;
				
				var isOpen = ImGui.TreeNodeEx(
					new IntPtr(i),
					flags,
					component.GetType().Name
				);

				// if (ImGui.IsItemClicked()) _SelectedComponent = i;

				if (isOpen)
				{
					var members = component.GetType().GetMembers();
					foreach (var member in members) {
						if (member.MemberType != System.Reflection.MemberTypes.Field
						&& member.MemberType != System.Reflection.MemberTypes.Property) continue;
						ValueMember valueMember = new(member);
						if (!valueMember.IsReadable) continue;

						_MemberEditor(valueMember, component);
					}
					ImGui.TreePop();
				}
			}
		} else {
			// _SelectedComponent = null;
			ImGui.TextDisabled("Nothing Selected.");
		}
	}
}
