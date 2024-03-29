﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;
using System.Windows.Input;

#if HMVVM
namespace Haley.Models{
#elif HWPFR
namespace Haley.IconsPack.Models {
#endif

//Reason why we have it here separately is that Haley.MVVM doesn't have any reference to Haley.WPF.IconsPack but common dictionary is also needed in Haley.WPF.IconsPack.
//We do not need to maintain another separate nuget package for the utils that are shared between WPF/MVVM/Resources
//We need only to share a portion of the code between the two
    public class CommonDictionary : ResourceDictionary
    {
        public CommonDictionary() { }//need a new constructor

        //A static dictionary
        public static ConcurrentDictionary<Uri, ResourceDictionary> DictionaryStore = new ConcurrentDictionary<Uri, ResourceDictionary>();

        private Uri _source;

        public new Uri Source
        {
            get
            {
                //if (IsInDesignMode)
                //{
                //    return base.Source;
                //}
                return _source;
            }
            set
            {
                if (value == null) return;

                //if (IsInDesignMode)
                //{
                //    base.Source = value;
                //    return;
                //}

                //Instead of creating a new RD each time, we create and save the RD in a ConcurrentDictionary.
                //So, next time some user control requests a RD for this same URI, we fetch the RD from our store and then add it to merged dictionaries. basically, we are having same dictionary referenced everywhere. The first benefit would be that if we change theme in one place, it should affect all places.
                if (!DictionaryStore.ContainsKey(value))
                {
                    DictionaryStore.TryAdd(value, this);
                    //And finally, we assign the source value to this resource dictionary.
                    //We are also adding the value to the source to generate the dictionary
                }
                else
                {
                    ResourceDictionary result;
                    DictionaryStore.TryGetValue(value, out result);
                    MergedDictionaries.Add(result);
                }
                base.Source = value;
                _source = value;

            }
        }

}
#if HMVVM || HWPFR
}
#endif
