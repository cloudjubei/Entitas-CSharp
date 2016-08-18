using System;
using System.Collections.Generic;
using Entitas;

namespace Entitas {
	
	public class SetIndex<T> : AbstractEntityIndex {

		readonly Dictionary<T, HashSet<Entity>> _index;
		readonly Func<Entity, IComponent, T> _getKey;

		public SetIndex(Group group, Func<Entity, IComponent, T> getKey) : base(group) {
			_getKey = getKey;
			_index = new Dictionary<T, HashSet<Entity>>();
			IndexEntities();
		}

		public HashSet<Entity> GetEntities(T key) {
			HashSet<Entity> entities;
			if (_index.TryGetValue(key, out entities)) {
				return new HashSet<Entity>(entities);
			}
			return null;		
		}

		public bool HasEntitities(T key)
		{
			return _index.ContainsKey(key);
		}

		public int GetCount(T key)
		{
			HashSet<Entity> entities;
			if (_index.TryGetValue(key, out entities)) {
				return entities.Count;
			}
			return 0;
		}

		protected override void Clear() {
			foreach (var entities in _index.Values) {
				foreach (var entity in entities) {
					entity.Release(this);
				}
			}

			_index.Clear();
		}

		protected override void AddEntity (Entity entity, IComponent component)
		{
			T key = _getKey(entity, component);

			HashSet<Entity> entities;
			if(!_index.TryGetValue(key, out entities)){
				entities = new HashSet<Entity>(EntityEqualityComparer.comparer);
				_index.Add(key, entities);
			}
			if(!entities.Contains(entity)){
				entities.Add(entity);
				entity.Retain(this);
			}
		}

		protected override void RemoveEntity (Entity entity, IComponent component)
		{
			T key = _getKey(entity, component);

			HashSet<Entity> entities;
			if(_index.TryGetValue(key, out entities)){
				if(entities.Remove(entity)){
					entity.Release(this);
				}
			}
		}
	}
}