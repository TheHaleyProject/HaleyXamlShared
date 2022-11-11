using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Haley.Enums;
using Haley.Models;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using dwg = System.Drawing;
using System.Windows.Markup;
using System.ComponentModel;
using System.Reflection;

namespace Haley.Utils
{
    internal sealed class InternalUtilsCommon {
        public static void ClampLimits(ref int actual,int min = 0, int max = 255)
        {
            if (actual > max) actual = max;
            if (actual < min) actual = min;
        }

        public static double ClampLimits(double input, double min, double max)
        {
            if (input < min)
            {
                return min;
            }
            else if (input > max)
            {
                return max;
            }
            else
            {
                return input;
            }
        }

        public static bool GetTargetElement(IServiceProvider serviceProvider, out DependencyElement target) {
            target = null;
            try {
                var targetProvider = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
                target = new DependencyElement() { TargetObject = targetProvider.TargetObject as FrameworkElement, TargetProperty = targetProvider.TargetProperty as DependencyProperty };

                if (target == null || target.TargetObject == null) return false;
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public static object FetchValueAndMonitor(object sourceObject, string prop_name,PropertyChangedEventHandler PropChangeHandler) {

            //A property name can be direct or it can be a class name with dot notation.
            object prop_value = null;
            try {
                //If newvalue is an object.
                Type targetType = sourceObject?.GetType();
                string path = prop_name;
                string sub_path = string.Empty;
                if (prop_name.Contains(".")) {
                    var splitted = prop_name.Split('.');
                    path = splitted.First(); //
                    sub_path = string.Join(".", splitted.Skip(1).ToArray());
                }

                PropertyInfo tarProp = targetType.GetProperty(path);
                prop_value = tarProp?.GetValue(sourceObject);

                if (prop_value == null) return null; //Because either we have some object or we are not able to find that property name itself.

                if (!string.IsNullOrWhiteSpace(sub_path)) {
                    prop_value = FetchValueAndMonitor(prop_value, sub_path, PropChangeHandler);
                } else {
                    //The final property's holding class should be implementing INotifyPropertyChanged.
                    if (typeof(INotifyPropertyChanged).IsAssignableFrom(targetType)) {
                        //Subscribe to this property change also
                        (sourceObject as INotifyPropertyChanged).PropertyChanged -= PropChangeHandler;
                        (sourceObject as INotifyPropertyChanged).PropertyChanged += PropChangeHandler;
                    }
                }
                return prop_value;
            } catch (Exception) {
                return prop_value;
            }
        }
    }
}
