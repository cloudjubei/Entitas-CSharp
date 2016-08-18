using Entitas;

namespace Entitas {

	public abstract class AbstractEntityIndex : IEntityIndex {

		protected readonly Group _group;

		protected AbstractEntityIndex(Group group) {
			_group = group;

			group.OnEntityAdded += OnEntityAdded;
			group.OnEntityRemoved += OnEntityRemoved;
		}

		public virtual void Deactivate() {
			_group.OnEntityAdded -= OnEntityAdded;
			_group.OnEntityRemoved -= OnEntityRemoved;
			Clear();
		}

		protected void IndexEntities() {
			foreach(Entity e in _group.GetEntities()){
				AddEntity(e, null);
			}
		}

		protected virtual void OnEntityAdded(Group group, Entity entity, int index, IComponent component)
		{
			AddEntity(entity, component);
		}


		protected virtual void OnEntityRemoved(Group group, Entity entity, int index, IComponent component)
		{
			RemoveEntity(entity, component);
		}

		protected abstract void Clear();

		protected abstract void AddEntity(Entity entity, IComponent component);

		protected abstract void RemoveEntity(Entity entity, IComponent component);

		~AbstractEntityIndex () {
			Deactivate();
		}
	}
}