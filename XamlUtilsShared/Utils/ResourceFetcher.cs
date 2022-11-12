using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Resources;
using System.Collections.Concurrent;
using Haley.Models;

#if HWPFR
using Haley.IconsPack.Models;
#endif

#if HMVVM
namespace Haley.Utils {
#elif HWPFR
namespace Haley.IconsPack.Models {
#endif

    //Since this is internal static, each assembly will retain their own static dictionary which will not be shared/accessed-by other assemblies
    public static class ResourceFetcher {

        static ConcurrentDictionary<Enum, CommonDictionary> _dicSource = new ConcurrentDictionary<Enum, CommonDictionary>();
        static List<Uri> _paths = new List<Uri>();

        //public static bool TryAddSource<T>(Uri dictionary_path) where T: Enum{
        public static bool AddSource(Enum source, Uri dictionary_path) {
            if (_dicSource.ContainsKey(source)) return false;
            if (dictionary_path == null) return false;

            //try to create the dictionary
            var dic = new CommonDictionary();
            dic.Source = dictionary_path;

            bool result = _dicSource.TryAdd(source, dic);

            if (result) {
                _paths.Add(dictionary_path);
            }

            return result;
        }

        public static bool RemoveSource(Enum source) {
            try {
                bool result = _dicSource.TryRemove(source, out var removed);
                if (result) {
                    _paths.RemoveAll(p => p.ToString().ToLower().Equals(removed.Source?.ToString().ToLower()));
                }
                return result;
            } catch (Exception) {
                return false;
            }
        }

        public static object GetResource(Enum source, object resource_key) {
            if (!_dicSource.ContainsKey(source)) return null; //Source key itself not found.
            _dicSource.TryGetValue(source, out var dic);

            //For this dictionary, try to get the value. (only flat level, do not try to fetch merged dictionaries)

            if (dic.Contains(resource_key)) return dic[resource_key];
            return null;
        }

        public static T GetResource<T>(Enum source, object resource_key) where T : class {

            return GetResource(source, resource_key) as T;
        }

        public static object GetResourceAny(object resource_key) {
            object result = null;
            //Loop through all resources and their children
            foreach (var dic in _dicSource.Values) {
                result = GetResourceAny(dic, resource_key);
                if (result != null) return result;
            }
            return result;
        }

        public static T GetResourceAny<T>(object resource_key) where T : class {
            return GetResourceAny(resource_key) as T;
        }

        static object GetResourceAny(ResourceDictionary dic, object key) {
            object result = null;
            if (dic.Contains(key)) {
                result = dic[key];
            }

            if (result == null && dic.MergedDictionaries?.Count() > 0) {
                //try merged dictionaries.
                foreach (var m_dic in dic.MergedDictionaries) {
                    result = GetResourceAny(m_dic, key);
                    if (result != null) return result;
                }
            }
            return result;
        }

        public static List<Uri> GetAllSourcePaths() {
            return _paths.ToList();
        }
    }

#if HMVVM || HWPFR
}
#endif
