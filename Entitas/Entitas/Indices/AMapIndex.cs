using Entitas;

namespace Entitas {

	public abstract class AMapIndex<T> : IEntityIndex where T : class
	{
		protected Group group;
		protected T[,] grid;
		protected bool initialised;

		protected AMapIndex(Group group)
		{
			this.group = group;
		}

#region Public

		public virtual void Init(int maxSizeX, int maxSizeY)
		{
			grid = new T[maxSizeX, maxSizeY];

			if(ShouldInitialiseGrid()){
				for(int i=grid.GetLength(0)-1; i>=0; i--){
					for(int j=grid.GetLength(1)-1; j>=0; j--){
						InitAt(i, j);
					}
				}
			}

			group.OnEntityAdded += OnEntityAdded;
			group.OnEntityRemoved += OnEntityRemoved;

			IndexEntities();

			initialised = true;
		}

		public virtual void Deactivate()
		{
			group.OnEntityAdded -= OnEntityAdded;
			group.OnEntityRemoved -= OnEntityRemoved;
			Clear();
		}

		public virtual bool IsEmptyAt(int x, int y)
		{
			if(!initialised){
				throw new EntityIndexException("EntityMapIndex hasn't been initialised!", "Please call Init() before accessing any index functions");
			}
			if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1))
			{
				return grid[x, y] == null;
			}
			return false;
		}

#endregion

#region EntityMapIndex

		protected virtual bool ShouldInitialiseGrid()
		{
			return false;
		}

		protected virtual void InitAt(int i, int j)
		{
		}

		protected virtual T GetAt(int x, int y)
		{
			if(!initialised){
				throw new EntityIndexException("EntityMapIndex hasn't been initialised!", "Please call Init() before accessing any index functions");
			}
			if(!IsEmptyAt(x, y)){
				return grid[x, y];
			}
			return null;
		}

		protected virtual void ClearAt(int i, int j)
		{
			grid[i,j] = null;
		}

		protected abstract void AddAt(int i, int j, Entity e);
		protected abstract void RemoveAt(int i, int j, Entity e);
		protected abstract int GetSizeX(Entity e, IComponent component);
		protected abstract int GetSizeY(Entity e, IComponent component);
		protected abstract int GetPositionX(Entity e, IComponent component);
		protected abstract int GetPositionY(Entity e, IComponent component);

#endregion

#region Internal

		protected void IndexEntities()
		{
			foreach(Entity e in group.GetEntities()){
				AddEntity(e, null);
			}
		}

		protected void Clear()
		{
			for(int i=grid.GetLength(0)-1; i>=0; i--){
				for(int j=grid.GetLength(1)-1; j>=0; j--){
					ClearAt(i, j);
				}
			}
			grid = null;
		}

		protected void OnEntityAdded(Group group, Entity entity, int index, IComponent component)
		{
			AddEntity(entity, component);
		}

		protected void OnEntityRemoved(Group group, Entity entity, int index, IComponent component)
		{
			RemoveEntity(entity, component);
		}

		protected void AddEntity(Entity entity, IComponent component)
		{	
			int sizeX = GetSizeX(entity, component);
			int sizeY = GetSizeY(entity, component);
			int x = GetPositionX(entity, component);
			int y = GetPositionY(entity, component);

			if(x >= 0 && x + sizeX < grid.GetLength(0) && y >= 0 && y + sizeY < grid.GetLength(1) && sizeX > 0 && sizeY > 0){
				for (int i = sizeX - 1; i >= 0; i--)
				{
					for (int j = sizeY - 1; j >= 0; j--)
					{
						AddAt(x + i,y + j, entity);
					}
				}
			}					
		}

		protected void RemoveEntity(Entity entity, IComponent component)
		{
			int sizeX = GetSizeX(entity, component);
			int sizeY = GetSizeY(entity, component);
			int x = GetPositionX(entity, component);
			int y = GetPositionY(entity, component);

			if(x >= 0 && x + sizeX < grid.GetLength(0) && y >= 0 && y + sizeY < grid.GetLength(1) && sizeX > 0 && sizeY > 0){
				for (int i = sizeX - 1; i >= 0; i--)
				{
					for (int j = sizeY - 1; j >= 0; j--)
					{
						RemoveAt(x + i,y + j, entity);
					}
				}
			}		
		}

#endregion
	}
}