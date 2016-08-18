namespace Entitas.CodeGenerator {
	public class IndexInfo {

		public string name { get { return _name; } }
		public string fullTypeName { get { return _fullTypeName; } }
		public string typeName { get { return _typeName; } }
		public string pool { get { return _pool; } }
		public string poolName { get { return _poolName; } }
		public int[] matchers { get { return _matchers; } }
		public IndexFunction[] functions { get { return _functions; } }

		readonly string _name;
		readonly string _fullTypeName;
		readonly string _typeName;
		readonly string _pool;
		readonly string _poolName;
		readonly int[] _matchers;
		readonly IndexFunction[] _functions;

		public IndexInfo(string name, string fullTypeName, string pool, int[] matchers, IndexFunction[] functions) {
			_name = name;
			_fullTypeName = fullTypeName;
			_pool = pool;
			_poolName = pool.LowercaseFirst();

			var nameSplit = fullTypeName.Split('.');
			_typeName = nameSplit[nameSplit.Length - 1];

			_matchers = matchers;
			_functions = functions;
		}
	}
	public class IndexFunction {
		public string returnFuncName { get { return _returnFuncName; } }
		public string returnType { get { return _returnType; } }
		public string parameters { get { return _parameters; } }
		public string callParameters { get { return _callParameters; } }
		public string indexFuncName { get { return _indexFuncName; } }

		readonly string _returnFuncName;
		readonly string _returnType;
		readonly string _parameters;
		readonly string _callParameters;
		readonly string _indexFuncName;

		public IndexFunction(string returnFuncName, string returnType, string parameters, string callParameters, string indexFuncName)
		{
			_returnFuncName = returnFuncName;
			_returnType = returnType;
			_parameters = parameters;
			_callParameters = callParameters;
			_indexFuncName = indexFuncName;
		}
	}
}