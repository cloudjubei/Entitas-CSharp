namespace Entitas.CodeGenerator {
    
	public class PrimaryKeyIndexAttribute : ANamedIndexAttribute {
		public PrimaryKeyIndexAttribute(string name, params int[] matchers) : base(name, matchers) {
        }
    }
}