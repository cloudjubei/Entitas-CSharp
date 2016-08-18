namespace Entitas.CodeGenerator {

	public class SetIndexAttribute : ANamedIndexAttribute {
		public SetIndexAttribute(string name, params int[] matchers) : base(name, matchers) {
		}
	}
}