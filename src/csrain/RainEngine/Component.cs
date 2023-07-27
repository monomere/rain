
using System.Collections.Generic;
using System.Numerics;

namespace RainEngine
{
	public class Entity
	{
		internal List<Component> Components = new();

		public uint Id { get; }
		public Scene Scene { get; }
		public Entity(uint id, Scene scene) { Id = id; Scene = scene; }

		public void AddComponent<T>(T component) where T : Component
		{
			component.Bound = this;
			Components.Add(component);
		}

		public T? GetComponent<T>() where T : Component
		{
			foreach (var comp in Components)
			{
				if (comp is T) return (T)comp;
			}
			return null;
		}
	}

	public class Component
	{
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
	}

	public class TransformComponent : Component
	{
		public TransformComponent? Parent;

		private bool _Dirty;
		private Vector3 _Position;
		private Quaternion _Rotation;
		private Vector3 _Scale;

		private Matrix4x4 _Matrix;

		public Matrix4x4 LocalTransform
		{
			get
			{
				if (_Dirty)
				{
					_Matrix
						= Matrix4x4.CreateTranslation(_Position)
						* Matrix4x4.CreateFromQuaternion(_Rotation)
						* Matrix4x4.CreateScale(_Scale)
						;
					_Dirty = false;
				}
				return _Matrix;
			}
		}

		public Matrix4x4 GlobalTransform =>
			Parent == null ? LocalTransform : LocalTransform * Parent.GlobalTransform;

		public Vector3 Position
		{
			get => _Position;
			set { _Position = value; _Dirty = true; }
		}

		public Quaternion Rotation
		{
			get => _Rotation;
			set { _Rotation = value; _Dirty = true; }
		}

		public Vector3 Scale
		{
			get => _Scale;
			set { _Scale = value; _Dirty = true; }
		}
	}
}
