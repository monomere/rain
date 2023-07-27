using System;
using System.Collections.Generic;

namespace RainEngine
{
	public class Scene
	{
		internal List<Entity> Entities;
		private uint NextId = 0;

		public Scene()
		{
			Entities = new();
		}

		public Entity CreateEntity(params Component[] components)
		{
			Entity entity = new(NextId++, this);
			Entities.Add(entity);
			foreach (var component in components)
			{
				entity.AddComponent(component);
			}
			return entity;
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
