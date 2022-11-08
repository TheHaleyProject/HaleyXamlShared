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
                imgSourceDics.Add(new CommonDictionary() { Source = new Uri("pack://application:,,,/Haley.WPF.Resources;component/Dictionaries/haleyIconsCommon.xaml", UriKind.RelativeOrAbsolute) });
            }

            if (svgDics == null || svgDics.Count == 0) {

                svgDics = new List<CommonDictionary>();
                svgDics.Add(new CommonDictionary() { Source = new Uri("pack://application:,,,/Haley.WPF.Resources;component/Dictionaries/haleySvgBranded.xaml", UriKind.RelativeOrAbsolute) });
            }

            ImageSource result = null;

            //Try icons first, and then SVGS
            if (type == IconTargetType.Any) {
               result = GetIcon(imgSourceDics, resourceKey);
                if (result == null) result = GetIcon(svgDics, resourceKey);
            }

            //Svgs
            if (type == IconTargetType.Svg) {
                result = GetIcon(svgDics, resourceKey);
            }

            //Imgs
            if (type == IconTargetType.Image) {
                result = GetIcon(imgSourceDics, resourceKey);
            }
            return result;
        }

        static ImageSource GetIcon(List<CommonDictionary> dicSource,object key) {
            foreach (var dic in dicSource) {
                if (dic.Contains(key)) {
                    return dic[key] as ImageSource; //Sometimes imagesource could be null, if we try to return a different object.
                }
            }
            return null;
        }
    }

    internal enum IconTargetType {
        Image,
        Svg,
        Any
    }
}
