namespace Entitas
{
	public abstract class EntityMapIndex : AMapIndex<Entity>
	{
		protected EntityMapIndex(Group group) : base(group)
		{
		}

		public virtual Entity GetEntityAt(int i, int j)
		{
			return GetAt(i, j);
		}

		protected override void AddAt(int i, int j, Entity e)
		{
			grid[i, j] = e;
			e.Retain(this);
		}

		protected override void RemoveAt(int i, int j, Entity e)
		{
			ClearAt(i, j);
		}

		protected override void ClearAt(int i, int j)
		{
			if(grid[i,j] != null){
				grid[i,j].Release(this);
				grid[i, j] = null;
			}
		}
	}
}