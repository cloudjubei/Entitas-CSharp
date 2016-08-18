using System;

namespace Entitas.CodeGenerator {

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
	public abstract class ANamedIndexAttribute : Attribute {
		public readonly string name;
		public readonly int[] matchers;

		protected ANamedIndexAttribute(string name, params int[] matchers) {
			this.name = name;
			this.matchers = matchers;
		}
	}
}