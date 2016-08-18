namespace Entitas {

	public abstract class AbstractEntityDictIndex<T> : AbstractDictIndex<T,Entity>
	{
		protected AbstractEntityDictIndex(Group group) : base(group)
		{
		}

#region AbstractDictIndex

		protected override Entity GetValue(Entity entity, IComponent component)
		{
			return entity;
		}

		protected override void Add(T key, Entity value)
		{
			base.Add(key, value);

			value.Retain(this);
		}

		protected override Entity Remove(T key)
		{
			Entity e = base.Remove(key);

			e.Release(this);

			return e;
		}

		protected override void Clear()
		{
			foreach(Entity e in index.Values){
				e.Release(this);
			}
			index.Clear();
		}

#endregion
	}
}