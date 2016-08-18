namespace Entitas.CodeGenerator {
    public interface ICodeGeneratorDataProvider {
        string[] poolNames { get; }
		ComponentInfo[] componentInfos { get; }
		IndexInfo[] indexInfos { get; }
        string[] blueprintNames { get; }
    }
}

