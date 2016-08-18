using System;

namespace Entitas.CodeGenerator {

	[AttributeUsage(AttributeTargets.Class)]
	public class CustomIndexAttribute : Attribute {
		public readonly string name;
		public readonly string poolName;
		public readonly int[] matchers;

		public CustomIndexAttribute(string name, string poolName, params int[] matchers) {
			this.name = name;
			this.poolName = poolName;
			this.matchers = matchers;
		}
	}
}