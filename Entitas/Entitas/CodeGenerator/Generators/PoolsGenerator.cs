using System.Collections.Generic;
using System.Linq;
using Entitas.CodeGenerator;
using Entitas.Serialization;

namespace Entitas.CodeGenerator {
    public class PoolsGenerator : IPoolCodeGenerator {

        const string CLASS_TEMPLATE = @"using Entitas;

public class Pools {{
{0}
{1}
}}
{2}";

        const string CLASS_EXTENSIONS_TEMPLATE = @"
public static class PoolExtensions {{{0}
}}
";

        const string POOL_FIELD = @"
        public Pool {0};";

        const string POOL_CONSTRUCTOR = @"
        $poolName = new Pool($Ids.TotalComponents, 0, new PoolMetaData(""$PoolNameWithSpace"", $Ids.componentNames, $Ids.componentTypes));

#if (!ENTITAS_DISABLE_VISUAL_DEBUGGING && UNITY_EDITOR)
        if (UnityEngine.Application.isPlaying) {
            var poolObserver = new Entitas.Unity.VisualDebugging.PoolObserver($poolName);
            UnityEngine.Object.DontDestroyOnLoad(poolObserver.entitiesContainer);
        }
#endif
";

        const string CONSTRUCTOR = @"{0}

    public Pools() {{
       {1}
       {2}
    }}";

        const string ALL_POOLS_GETTER = @"
    public Pool[] allPools {{
        get {{
            return new [] {{ {0} }};
        }}
    }}";

        const string PRIMARY_INDEX_ADD = @"
        $poolName.AddEntityIndex(""$indexName"", new PrimaryKeyIndex<$memberType>($group, (e, c) => c is $componentType ? (($componentType)c).$memberName : e.$componentName.$memberName));
        ";

        const string PRIMARY_INDEX_POOL_EXTENSION_FUNCTIONS = @"    

    public static Entity GetEntityWith$indexName(this Pool pool, $memberType $memberName)
    {
        return ((PrimaryKeyIndex<$memberType>)(pool.GetEntityIndex(""$indexName""))).GetEntity($memberName);
    }

    public static bool HasEntityWith$indexName(this Pool pool, $memberType $memberName)
    {
        return ((PrimaryKeyIndex<$memberType>)(pool.GetEntityIndex(""$indexName""))).HasEntity($memberName);
    }"
    ;

        const string SET_INDEX_ADD = @"
        $poolName.AddEntityIndex(""$indexName"", new SetIndex<$memberType>($group, (e, c) => c is $componentType ? (($componentType)c).$memberName : e.$componentName.$memberName));
        ";

        const string SET_INDEX_POOL_EXTENSION_FUNCTIONS = @"    

    public static System.Collections.Generic.HashSet<Entity> GetEntitiesWith$indexName(this Pool pool, $memberType $memberName)
    {
        return ((SetIndex<$memberType>)(pool.GetEntityIndex(""$indexName""))).GetEntities($memberName);
    }

    public static bool HasEntitiesWith$indexName(this Pool pool, $memberType $memberName)
    {
        return ((SetIndex<$memberType>)(pool.GetEntityIndex(""$indexName""))).HasEntitities($memberName);
    }

    public static int GetCountForEntitiesWith$indexName(this Pool pool, $memberType $memberName)
    {
        return ((SetIndex<$memberType>)(pool.GetEntityIndex(""$indexName""))).GetCount($memberName);
    }"
    ;

        const string CUSTOM_INDEX_ADD = @"
        $poolName.AddEntityIndex(""$indexName"", new $indexType($constructorParams));
        ";

        const string CUSTOM_INDEX_POOL_EXTENSION_FUNCTION = @"
    public static $returnType $returnFuncName(this Pool pool, $params)
    {
        return (($indexType)(pool.GetEntityIndex(""$indexName""))).$indexFuncName($callParams);
    }
    ";

        public CodeGenFile[] Generate(string[] poolNames, ComponentInfo[] componentInfos, IndexInfo[] indexInfos) {

            string constructor = string.Format(CONSTRUCTOR, GetFields(poolNames), GetPoolConstructors(poolNames, componentInfos), GetIndices(componentInfos, indexInfos));

            string allPools = string.Format(ALL_POOLS_GETTER,
                string.Join(", ", poolNames.Select(poolName => poolName.LowercaseFirst()).ToArray()));

            string extensions = string.Format(CLASS_EXTENSIONS_TEMPLATE, GetIndicesExtensions(componentInfos, indexInfos));

            return new[] { new CodeGenFile("Pools", string.Format(CLASS_TEMPLATE, constructor, allPools, extensions), GetType().FullName) };
        }

        string GetFields(string[] poolNames) {
            return poolNames.Aggregate(string.Empty, (acc, poolName) =>
                                       acc + GetField(poolName));
        }

        string GetField(string poolName) {
            return string.Format(POOL_FIELD, poolName.LowercaseFirst());
        }

        string GetPoolConstructors(string[] poolNames, ComponentInfo[] infos) {
            return poolNames.Aggregate(string.Empty, (acc, poolName) =>
                                       acc + GetPoolConstructor(poolName, infos));
        }

        string GetPoolConstructor(string poolName, ComponentInfo[] infos) {
            string format = CreateFormatString(POOL_CONSTRUCTOR);

            string poolNameLowercase = poolName.LowercaseFirst();
            string poolPrefix = poolName.IsDefaultPoolName() ? string.Empty : poolName.PoolPrefix().UppercaseFirst();
            string poolNameWithSpace = poolPrefix + (poolName.IsDefaultPoolName() ? string.Empty : " ") + CodeGenerator.DEFAULT_POOL_NAME;
            string ids = poolName.PoolPrefix() + CodeGenerator.DEFAULT_COMPONENT_LOOKUP_TAG;

            return string.Format(format, poolNameLowercase, poolPrefix, poolNameWithSpace, ids);
        }

        internal class Tuple<T1, T2> {
            public T1 first;
            public T2 second;

            public Tuple(T1 first, T2 second) {
                this.first = first;
                this.second = second;
            }
        }

        string GetIndices(ComponentInfo[] infos, IndexInfo[] indexInfos) {
            string indices = "";

            indices += CreateIndices<PrimaryKeyIndexAttribute>(CreateIndexAddString(PRIMARY_INDEX_ADD), infos);
            indices += CreateIndices<SetIndexAttribute>(CreateIndexAddString(SET_INDEX_ADD), infos);
            indices += CreateIndices(CreateCustomIndexAddString(CUSTOM_INDEX_ADD), indexInfos);

            return indices;
        }

        string CreateIndices<T>(string format, ComponentInfo[] infos) where T : ANamedIndexAttribute {
            string indices = "";
            List<Tuple<ComponentInfo, PublicMemberInfo>> infosWithSetIndex = GetInfosWithIndex<T>(infos);

            foreach (var tuple in infosWithSetIndex) {
                var attributes = tuple.second.attributes.Where(attr => attr.attribute is T);
                foreach (var attr in attributes) {
                    indices += CreateIndexAdd(format, tuple.first, tuple.second, (T)(attr.attribute));
                }
            }
            return indices;
        }

        string CreateIndices(string format, IndexInfo[] indexInfos) {
            string indices = "";

            foreach (var info in indexInfos) {
                indices += CreateIndexCustomAdd(format, info);
            }
            return indices;
        }

        string CreateIndexAdd(string format, ComponentInfo componentInfo, PublicMemberInfo memberInfo, ANamedIndexAttribute attribute) {
            return CreateIndexAdd(format, componentInfo, memberInfo, attribute.name, attribute.matchers);
        }

        string CreateIndexAdd(string format, ComponentInfo componentInfo, PublicMemberInfo memberInfo, string attributeName, int[] attributeMatchers) {
            string poolName = componentInfo.pools.Length > 0 ? componentInfo.pools[0].LowercaseFirst() : CodeGenerator.DEFAULT_POOL_NAME.LowercaseFirst();
            string indexName = attributeName;
            string componentType = componentInfo.fullTypeName;
            string componentName = componentInfo.typeName.Replace("Component", "").LowercaseFirst();
            string memberType = memberInfo.type.ToString();
            string memberName = memberInfo.name;
            string matcherGroup = GetIndexGroup(poolName, attributeMatchers);

            return string.Format(format, poolName, indexName, componentType, componentName, memberType, memberName, matcherGroup);
        }

        string CreateIndexCustomAdd(string format, IndexInfo info) {
            string poolName = info.poolName;
            string indexName = info.name;
            string indexType = info.typeName;
            string matcherGroup = info.matchers == null || info.matchers.Length == 0 ? poolName : GetIndexGroup(poolName, info.matchers);

            return string.Format(format, poolName, indexName, indexType, matcherGroup);
        }

        string GetIndexGroup(string poolName, int[] matchers) {
            string matcher = "Matcher.AllOf(" + string.Join(",", matchers.Select(m => "" + m).ToArray()) + ")";

            return poolName + ".GetGroup(" + matcher + ")";
        }

        string GetIndicesExtensions(ComponentInfo[] componentInfos, IndexInfo[] indexInfos) {
            string extensions = "";

            extensions += CreateExtensions<PrimaryKeyIndexAttribute>(CreateIndexExtensionFunctionString(PRIMARY_INDEX_POOL_EXTENSION_FUNCTIONS), componentInfos);
            extensions += CreateExtensions<SetIndexAttribute>(CreateIndexExtensionFunctionString(SET_INDEX_POOL_EXTENSION_FUNCTIONS), componentInfos);
            extensions += CreateExtensions(CreateCustomIndexExtensionFunctionString(CUSTOM_INDEX_POOL_EXTENSION_FUNCTION), indexInfos);

            return extensions;
        }

        string CreateExtensions<T>(string format, ComponentInfo[] infos) where T : ANamedIndexAttribute {
            string extensions = "";

            List<Tuple<ComponentInfo, PublicMemberInfo>> infosWithIndex = GetInfosWithIndex<T>(infos);

            foreach (var tuple in infosWithIndex) {
                var attributes = tuple.second.attributes.Where(attr => attr.attribute as T != null);
                foreach (var attr in attributes) {
                    extensions += CreateIndexExtensionFunctions(format, tuple.first, tuple.second, (T)(attr.attribute));
                }
            }
            return extensions;
        }

        string CreateExtensions(string format, IndexInfo[] infos) {
            string extensions = "";

            foreach (var info in infos) {
                foreach (var func in info.functions) {
                    extensions += CreateCustomIndexExtensionFunction(format, info, func);
                }
            }
            return extensions;
        }

        string CreateIndexExtensionFunctions(string format, ComponentInfo componentInfo, PublicMemberInfo memberInfo, ANamedIndexAttribute attribute) {
            string indexName = attribute.name;
            string memberType = memberInfo.type.ToString();
            string memberName = memberInfo.name;

            return string.Format(format, indexName, memberType, memberName);
        }

        string CreateCustomIndexExtensionFunction(string format, IndexInfo info, IndexFunction func) {
            string indexName = info.name;
            string indexType = info.typeName;
            string returnFuncName = func.returnFuncName;
            string indexFuncName = func.indexFuncName;
            string returnType = func.returnType;
            string parameters = func.parameters;
            string callParameters = func.callParameters;

            return string.Format(format, indexName, indexType, returnFuncName, indexFuncName, returnType, parameters, callParameters);
        }

        List<Tuple<ComponentInfo, PublicMemberInfo>> GetInfosWithIndex<T>(ComponentInfo[] infos) {
            List<Tuple<ComponentInfo, PublicMemberInfo>> infosWithIndex = new List<Tuple<ComponentInfo, PublicMemberInfo>>();

            foreach (ComponentInfo info in infos) {
                var memberInfosWithIndex = info.memberInfos.Where((memberInfo) => memberInfo.attributes.Any(attr => attr.attribute is T));
                foreach (var memberInfo in memberInfosWithIndex) {
                    infosWithIndex.Add(new Tuple<ComponentInfo, PublicMemberInfo>(info, memberInfo));
                }
            }
            return infosWithIndex;
        }

        string CreateFormatString(string format) {
            return format.Replace("{", "{{")
                .Replace("}", "}}")
                .Replace("$poolName", "{0}")
                .Replace("$PoolPrefix", "{1}")
                .Replace("$PoolNameWithSpace", "{2}")
                .Replace("$Ids", "{3}");
        }

        string CreateIndexAddString(string format) {
            return format.Replace("{", "{{")
                         .Replace("}", "}}")
                         .Replace("$poolName", "{0}")
                         .Replace("$indexName", "{1}")
                         .Replace("$componentType", "{2}")
                         .Replace("$componentName", "{3}")
                         .Replace("$memberType", "{4}")
                         .Replace("$memberName", "{5}")
                         .Replace("$group", "{6}");
        }

        string CreateIndexExtensionFunctionString(string format) {
            return format.Replace("{", "{{")
                         .Replace("}", "}}")
                         .Replace("$indexName", "{0}")
                         .Replace("$memberType", "{1}")
                         .Replace("$memberName", "{2}");
        }

        string CreateCustomIndexAddString(string format) {
            return format.Replace("{", "{{")
                .Replace("}", "}}")
                .Replace("$poolName", "{0}")
                .Replace("$indexName", "{1}")
                .Replace("$indexType", "{2}")
                .Replace("$constructorParams", "{3}");
        }

        string CreateCustomIndexExtensionFunctionString(string format) {
            return format.Replace("{", "{{")
                .Replace("}", "}}")
                .Replace("$indexName", "{0}")
                .Replace("$indexType", "{1}")
                .Replace("$returnFuncName", "{2}")
                .Replace("$indexFuncName", "{3}")
                .Replace("$returnType", "{4}")
                .Replace("$params", "{5}")
                .Replace("$callParams", "{6}");
        }
    }
}