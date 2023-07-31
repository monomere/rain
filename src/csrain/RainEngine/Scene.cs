using System;
using System.Collections.Generic;

namespace RainEngine
{
	public static class SceneManager
	{
		public static Scene ActiveScene {
			get;
			set;
		} = new("Default Scene");
	}

	public class Scene
	{
		internal List<Entity> Entities;
		private uint NextId = 0;

		public static Scene Active =>
			SceneManager.ActiveScene;
		
		public Camera ActiveCamera;
		public string Name;

		public Scene(string name)
		{
			Name = name;
			Entities = new();
			ActiveCamera = Camera.Orthographic(6.0f);
		}

		public Entity CreateEntity(string name, params Component[] components)
		{
			Entity entity = new(NextId++, this, name);
			Entities.Add(entity);
			foreach (var component in components)
			{
				entity.AddComponent(component);
			}
			return entity;
		}

		public void OnCreate()
		{
			foreach (var entity in Entities)
			{
				foreach (var component in entity.Components)
				{
					component.OnCreate();
				}
			}
		}

		public void OnUpdate(float deltaTime)
		{
			foreach (var entity in Entities)
			{
				foreach (var component in entity.Components)
				{
					component.OnUpdate(deltaTime);
				}
			}
		}

		public void OnRender()
		{
			foreach (var entity in Entities)
			{
				foreach (var component in entity.Components)
				{
					component.OnRender();
				}
			}
		}
	}
}
