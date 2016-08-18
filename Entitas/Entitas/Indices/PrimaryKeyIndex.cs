using System;
using System.Collections.Generic;
using Entitas;

namespace Entitas {
	
	public class PrimaryKeyIndex<T> : AbstractEntityIndex {

		readonly Dictionary<T, Entity> _index;
		readonly Func<Entity, IComponent, T> _getKey;

		public PrimaryKeyIndex(Group group, Func<Entity, IComponent, T> getKey) : base(group) {
			_getKey = getKey;
			_index = new Dictionary<T, Entity>();
			IndexEntities();
		}

		public bool HasEntity(T key) {
			return _index.ContainsKey(key);
		}

		public Entity GetEntity(T key) {
			var entity = TryGetEntity(key);
			if (entity == null) {
				throw new EntityIndexException("Entity for key '" + key + "' doesn't exist!",
					"You should check if an entity with that key exists before getting it.");
			}

			return entity;
		}

		public Entity TryGetEntity(T key) {
			Entity entity;
			_index.TryGetValue(key, out entity);
			return entity;
		}

		protected override void Clear() {
			foreach (var entity in _index.Values) {
				entity.Release(this);
			}

			_index.Clear();
		}

		protected override void AddEntity(Entity entity, IComponent component) {
			T key = _getKey(entity, component);

			if (_index.ContainsKey(key)) {
				throw new EntityIndexException("Entity for key '" + key + "' already exists!",
					"Only one entity for a primary key is allowed.");
			}

			_index.Add(key, entity);
			entity.Retain(this);
		}

		protected override void RemoveEntity(Entity entity, IComponent component) {
			T key = _getKey(entity, component);

			if(_index.ContainsKey(key)){
				_index.Remove(key);
				entity.Release(this);
			}
		}
	}
}