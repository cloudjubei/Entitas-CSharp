﻿using System;
using System.Collections.Generic;
using Entitas;

namespace Entitas {
	public partial class Matcher : IAllOfMatcher, IAnyOfMatcher, INoneOfMatcher {
        public int[] indices {
            get {
                if (_indices == null) {
                    _indices = mergeIndices();
                }
                return _indices;
            }
        }

        public int[] allOfIndices { get { return _allOfIndices; } }
        public int[] anyOfIndices { get { return _anyOfIndices; } }
        public int[] noneOfIndices { get { return _noneOfIndices; } }
		public IMatcher[] valueMatchers { get { return _valueMatchers; } }

        int[] _indices;
        int[] _allOfIndices;
        int[] _anyOfIndices;
        int[] _noneOfIndices;
		IMatcher[] _valueMatchers;

        Matcher() {
        }

        IAnyOfMatcher IAllOfMatcher.AnyOf(params int[] indices) {
            _anyOfIndices = distinctIndices(indices);
            _indices = null;
            return this;
        }

        IAnyOfMatcher IAllOfMatcher.AnyOf(params IMatcher[] matchers) {
            return ((IAllOfMatcher)this).AnyOf(mergeIndices(matchers));
        }

        public INoneOfMatcher NoneOf(params int[] indices) {
            _noneOfIndices = distinctIndices(indices);
            _indices = null;
            return this;
        }

        public INoneOfMatcher NoneOf(params IMatcher[] matchers) {
            return NoneOf(mergeIndices(matchers));
        }

        public bool Matches(Entity entity) {
            var matchesAllOf = _allOfIndices == null || entity.HasComponents(_allOfIndices);
            var matchesAnyOf = _anyOfIndices == null || entity.HasAnyComponent(_anyOfIndices);
            var matchesNoneOf = _noneOfIndices == null || !entity.HasAnyComponent(_noneOfIndices);
			var matchesValues = _valueMatchers == null || MatchesValues(_valueMatchers, entity);
			return matchesAllOf && matchesAnyOf && matchesNoneOf && matchesValues;
        }

		public bool IsValueMatcher()
		{
			return false;
		}

		static bool MatchesValues(IMatcher[] matchers, Entity e)
		{
			for(int i=matchers.Length-1; i>=0; i--){
				if(!matchers[i].Matches(e)){
					return false;
				}
			}
			return true;
		}

        int[] mergeIndices() {
            var totalIndices = (_allOfIndices != null ? _allOfIndices.Length : 0)
                               + (_anyOfIndices != null ? _anyOfIndices.Length : 0)
                               + (_noneOfIndices != null ? _noneOfIndices.Length : 0);

            var indicesList = new List<int>(totalIndices);
            if (_allOfIndices != null) {
                indicesList.AddRange(_allOfIndices);
            }
            if (_anyOfIndices != null) {
                indicesList.AddRange(_anyOfIndices);
            }
            if (_noneOfIndices != null) {
                indicesList.AddRange(_noneOfIndices);
            }

            return distinctIndices(indicesList);
        }

        static int[] mergeIndices(IMatcher[] matchers) {
            var indices = new int[matchers.Length];
            for (int i = 0, matchersLength = matchers.Length; i < matchersLength; i++) {
                var matcher = matchers[i];
                if (matcher.indices.Length != 1) {
                    throw new MatcherException(matcher);
                }
                indices[i] = matcher.indices[0];
            }

            return indices;
        }

		static IMatcher[] getValueMatchers(IMatcher[] matchers) {
			var valueMatchers = new List<IMatcher>();

			for (int i = matchers.Length-1; i >=0; i--) {
				var matcher = matchers[i];
				if(matcher.IsValueMatcher()){
					valueMatchers.Add(matcher);
				}
			}
			if(valueMatchers.Count > 0){
				return valueMatchers.ToArray();
			}
			return null;
		}

        static string[] getComponentNames(IMatcher[] matchers) {
            for (int i = 0, matchersLength = matchers.Length; i < matchersLength; i++) {
                var matcher = matchers[i] as Matcher;
                if (matcher != null && matcher.componentNames != null) {
                    return matcher.componentNames;
                }
            }

            return null;
        }

        static void setComponentNames(Matcher matcher, IMatcher[] matchers) {
            var componentNames = getComponentNames(matchers);
            if (componentNames != null) {
                matcher.componentNames = componentNames;
            }
        }

        static int[] distinctIndices(IEnumerable<int> indices) {
            var indicesSet = new HashSet<int>(indices);
            var uniqueIndices = new int[indicesSet.Count];
            indicesSet.CopyTo(uniqueIndices);
            Array.Sort(uniqueIndices);
            return uniqueIndices;
        }
    }

    public class MatcherException : Exception {
        public MatcherException(IMatcher matcher) : base("matcher.indices.Length must be 1 but was " + matcher.indices.Length) {
        }
    }
}

