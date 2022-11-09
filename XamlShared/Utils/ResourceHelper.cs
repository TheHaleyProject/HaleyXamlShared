using Haley.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Resources;

namespace Haley.Utils {
    internal static class ResourceHelper {
        public static CornerRadius cornerRadius = new CornerRadius(0.0);
        public static Thickness borderThickness = new Thickness(1);

        private static CommonDictionary colorDictionary;
        private static List<CommonDictionary> imgSourceDics;
        private static List<CommonDictionary> svgDics;

        //[Conditional("HWPF")] //Conditional attribute works only for void returns
        public static SolidColorBrush GetBrush(object resourceKey) {
#if HWPF
            if (colorDictionary == null)
            {
                //in case we decide to implement a Theme in Haley.WPF, we need to figure a way to change this theme at run time. (just like how we change theme using ThemeLoader)
                colorDictionary = new CommonDictionary(); //Even though we use 'new' key word, common dictionary has a static dictionary store where it checks and returns only distinct value.
                colorDictionary.Source = new Uri("pack://application:,,,/Haley.WPF;component/Dictionaries/ThemeColors/ThemeNormal.xaml",
                            UriKind.Absolute);

            }
            if (colorDictionary.Contains(resourceKey))
            {
                return (SolidColorBrush)colorDictionary[resourceKey];
            }
#endif

            return null;
        }
        public static ImageSource GetIcon(object resourceKey, IconTargetType type) {
            //Haley WPF has references to Haley.WPF Resources and also to it's own resources (if any)

            if (imgSourceDics == null || imgSourceDics.Count == 0) {
                imgSourceDics = new List<CommonDictionary>();
                imgSourceDics.Add(new CommonDictionary() { Source = new Uri("pack://application:,,,/Haley.WPF.IconsPack;component/Dictionaries/haleyIconsPack.xaml", UriKind.RelativeOrAbsolute) });
            }

            if (svgDics == null || svgDics.Count == 0) {

                svgDics = new List<CommonDictionary>();
                svgDics.Add(new CommonDictionary() { Source = new Uri("pack://application:,,,/Haley.WPF.IconsPack;component/Dictionaries/haleySvgPack.xaml", UriKind.RelativeOrAbsolute) });
            }

            ImageSource result = null;

            //Try icons first, and then SVGS
            if (type == IconTargetType.Any) {
               result = GetIconInternal(imgSourceDics, resourceKey);
                if (result == null) result = GetIconInternal(svgDics, resourceKey);
            }

            //Svgs
            if (type == IconTargetType.Svg) {
                result = GetIconInternal(svgDics, resourceKey);
            }

            //Imgs
            if (type == IconTargetType.Image) {
                result = GetIconInternal(imgSourceDics, resourceKey);
            }
            return result;
        }

        static ImageSource GetIconInternal(List<CommonDictionary> dicSource,object key) {
            ImageSource result = null;
            foreach (var dic in dicSource) {
                if (dic.Contains(key)) {
                    result = dic[key] as ImageSource; //Sometimes imagesource could be null, if we try to return a different object.
                    if (result != null) return result; //else continue to other dictionaries
                }
            }
            return result;
        }

        static ImageSource GetIconInternal(ResourceDictionary dic,object key) {
            ImageSource result = null;
            if (dic.Contains(key)) {
                result = dic[key] as ImageSource; //Sometimes imagesource could be null, if we try to return a different object.
            }

            if (result == null && dic.MergedDictionaries?.Count() > 0) {
                //try merged dictionaries.
                foreach (var m_dic in dic.MergedDictionaries) {
                    result = GetIconInternal(m_dic, key);
                    if (result != null) return result;
                }
            }
            return result;
        }
    }

    internal enum IconTargetType {
        Image,
        Svg,
        Any
    }
}
