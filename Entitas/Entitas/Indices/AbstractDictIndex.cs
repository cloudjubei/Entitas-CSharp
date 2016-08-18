using Entitas;
using System.Collections.Generic;

namespace Entitas {
	
	public abstract class AbstractDictIndex<TKey,TValue> : AbstractEntityIndex where TValue : class
	{
		protected readonly Dictionary<TKey, TValue> index;

		public AbstractDictIndex(Group group) : base(group)
		{
			index = new Dictionary<TKey, TValue>();
		}

		protected abstract TKey GetKey(Entity entity, IComponent component);

		protected abstract TValue GetValue(Entity entity, IComponent component);

		#region Public

		public virtual bool Has(TKey key)
		{
			return index.ContainsKey(key);
		}

		public virtual TValue Get(TKey key)
		{
			if(Has(key)){
				return index[key];
			}
			return null;
		}

		#endregion

		#region AbstractEntityIndex

		protected override void AddEntity(Entity entity, IComponent component)
		{
			Add(GetKey(entity, component), GetValue(entity, component));
		}

		protected override void RemoveEntity(Entity entity, IComponent component)
		{
			Remove(GetKey(entity, component));
		}

		protected override void Clear()
		{
			index.Clear();
		}

		#endregion

		#region Internal

		protected virtual void Add(TKey key, TValue value)
		{
			if (index.ContainsKey(key))
			{
				throw new EntityIndexException("Already has index " + this + " for key: " + key, "FIX!");
			}
			index.Add(key, value);
		}

		protected virtual TValue Remove(TKey key)
		{
			if (!index.ContainsKey(key))
			{
				throw new EntityIndexException("Trying to remove non-existent index on " + this + " for key: " + key, "FIX!");
			}
			TValue value = index[key];
			index.Remove(key);

			return value;
		}

		#endregion
	}
}