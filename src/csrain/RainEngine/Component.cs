
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace RainEngine
{
	public class Entity
	{
		internal List<Component> Components = new();

		public string Name;
		public uint Id { get; }
		public Scene Scene { get; }
		public Entity(uint id, Scene scene, string name) { Id = id; Scene = scene; Name = name; }

		public TransformComponent? Transform;

		public void AddComponent<T>(T component) where T : Component
		{
			component.Bound = this;
			Components.Add(component);
			if (component is TransformComponent)
			{
				Transform = component as TransformComponent;
			}
		}

		public void RemoveComponent<T>(T component) where T : Component
		{
			component.Bound = null;
			Components.Remove(component);
			if (component is TransformComponent)
			{
				Transform = null;
			}
		}

		public T? GetComponent<T>() where T : Component
		{
			if (typeof(T) == typeof(TransformComponent))
				return Transform as T;

			foreach (var comp in Components)
			{
				if (comp is T) return (T)comp;
			}
			return null;
		}
	}

	public class Component
	{
		[JsonIgnore]
		public Entity? Bound { get; internal set; }

		public void AddComponent<T>(T component) where T : Component
		{
			Bound?.AddComponent<T>(component);
		}

		public T? GetComponent<T>() where T : Component
		{
			return Bound?.GetComponent<T>();
		}

		public virtual void OnCreate() { }
		public virtual void OnUpdate(float deltaTime) { }
		public virtual void OnRender() { }
		public virtual void OnDestroy() { }

		[JsonIgnore]
		public TransformComponent? Transform => Bound?.Transform;
	}

	public class TransformComponent : Component
	{
		public TransformComponent? Parent;

		[JsonIgnore]
		private bool _Clean = false;
		[JsonIgnore]
		private Vector3 _Position;
		[JsonIgnore]
		private Quaternion _Rotation = Quaternion.Identity;
		[JsonIgnore]
		private Vector3 _Scale = new(1, 1, 1);

		[JsonIgnore]
		private Matrix4x4 _Matrix;

		[JsonConstructor]
		public TransformComponent(Vector3 position, Quaternion rotation, Vector3 scale) =>
			(_Position, _Rotation, _Scale, Parent, _Clean) = (position, rotation, scale, null, false);
		[JsonIgnore]
		public Matrix4x4 LocalTransform
		{
			get
			{
				if (!_Clean)
				{
					_Matrix
						= Matrix4x4.CreateTranslation(_Position)
						* Matrix4x4.CreateFromQuaternion(_Rotation)
						* Matrix4x4.CreateScale(_Scale)
						;
					_Clean = true;
				}
				return _Matrix;
			}
		}

		[JsonIgnore]
		public Matrix4x4 GlobalTransform =>
			Parent == null ? LocalTransform : LocalTransform * Parent.GlobalTransform;

		public Vector3 Position
		{
			get => _Position;
			set { _Position = value; _Clean = false; }
		}

		public Quaternion Rotation
		{
			get => _Rotation;
			set { _Rotation = value; _Clean = false; }
		}

		public Vector3 Scale
		{
			get => _Scale;
			set { _Scale = value; _Clean = false; }
		}
	}

	public class SpriteComponent : Component {
		public Vector4 Color { get; set; }
		public Asset<Texture> Sprite { get; set; }

		public SpriteComponent() {
			Sprite = new();
		}

		public SpriteComponent(Vector4 color) {
			Color = color;
			Sprite = new();
		}

		public SpriteComponent(Asset<Texture> sprite) {
			Sprite = sprite;
		}

		public override void OnCreate()
		{
		}

		public override void OnRender()
		{
			if (Sprite.Get() != null)
			{
				Renderer.RenderTexturedQuad(
					Sprite.Get()!,
					new(),
					Camera.Active.ComputeTransformMatrix(Transform!.GlobalTransform)
				);
			}
			else
			{
				Renderer.RenderColoredQuad(
					Color, Camera.Active.ComputeTransformMatrix(Transform!.GlobalTransform)
				);
			}
		}
	}
}
