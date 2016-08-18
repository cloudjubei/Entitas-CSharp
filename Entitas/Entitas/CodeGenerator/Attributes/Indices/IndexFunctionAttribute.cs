using System;

namespace Entitas.CodeGenerator {

	[AttributeUsage(AttributeTargets.Method)]
	public class IndexFunctionAttribute : Attribute {
		public readonly string name;

		public IndexFunctionAttribute(string name = "") {
			this.name = name;
		}
	}
}