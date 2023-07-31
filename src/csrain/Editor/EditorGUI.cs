using System;
using System.Collections.Generic;
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
	public string TitleCaseName => GUIUtils.CamelToTitle(Name);

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
	private ImGUI _G;
	private Entity? _Selected;

	public EditorGUI(ImGUI gui) {
		_G = gui;

		AddMemberEditor<float>((ValueMember m, ref float value, object _) => _G.Drag(m.TitleCaseName, ref value));
		AddMemberEditor<Vector2>((ValueMember m, ref Vector2 value, object _) => _G.Drag(m.TitleCaseName, ref value));
		AddMemberEditor<Vector3>((ValueMember m, ref Vector3 value, object _) => _G.Drag(m.TitleCaseName, ref value));
		AddMemberEditor<Vector4>((ValueMember m, ref Vector4 value, object _) => _G.Drag(m.TitleCaseName, ref value));
		AddMemberEditor<Quaternion>((ValueMember m, ref Quaternion value, object _) => _G.Drag(m.TitleCaseName, ref value));
	}

	private void RenderWindow(string name, Action content) {
		if (_G.Begin(name)) content();
		_G.End();
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

			bool isOpen = _G.TreeNode(
				$"Entity #{entity.Id}",
				ref isSelected,
				entity
			);

			if (_G.IsItemClicked())
			{
				_Selected = entity;
				Debug.Log("Selected new entity");
				_SelectedComponent = null;
			}

			if (isOpen)
			{
				_G.TreePop();
			}
		}
	}

	private void AssetBrowser()
	{
		float padding = 16.0f;
		float thumbnailSize = 128.0f;
		float cellSize = thumbnailSize + padding;

		// float panelWidth = ImGui.GetContentRegionAvail().x;
		// int columnCount = (int)(panelWidth / cellSize);
		// if (columnCount < 1)
		// 	columnCount = 1;

		// foreach (var asset in AssetManager.Active.Assets)
		// {

		// }
	}

	private void _MemberEditor(ValueMember valueMember, object component)
	{
		if (!_MemberEditors.ContainsKey(valueMember.ValueType)) return;
		_MemberEditors[valueMember.ValueType](valueMember, component);
	}

	private Dictionary<Type, Action<ValueMember, object>> _MemberEditors = new();

	delegate void MemberEditor<T>(ValueMember member, ref T value, object component);

	void AddMemberEditor<T>(MemberEditor<T> editor) =>
		_MemberEditors.Add(typeof(T), (valueMember, component) =>
		{
			if (!valueMember.IsWriteable) _G.Label("Readonly");

			var value = (T)valueMember.GetValue(component);

			editor(valueMember, ref value, component);
			
			if (valueMember.IsWriteable) valueMember.SetValue(component, value!);
		});

	private int? _SelectedComponent;
	private void Inspector()
	{
		if (_Selected != null) {
			_G.Label($"Entity #{_Selected.Id}: {_Selected.Name}");

			for (int i = 0; i < _Selected.Components.Count; ++i) {
				var component = _Selected.Components[i];
				bool isSelected = (_SelectedComponent == i);
				var isOpen = _G.TreeNode(
					component.GetType().Name,
					ref isSelected,
					component
				);
				if (_G.IsItemClicked())
				{
					_SelectedComponent = i;
				}
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
					_G.TreePop();
				}
			}
		} else {
			_SelectedComponent = null;
			_G.Label("Nothing Selected.");
		}
	}
}
