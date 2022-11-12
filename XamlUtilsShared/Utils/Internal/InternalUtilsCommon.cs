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

#if HWPFR
using Haley.IconsPack.Models;
using Haley.IconsPack.Abstractions;
#elif HMVVM
using Haley.Abstractions;
#endif

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

        private static void SetupPropertyMonitor(object sourceObject, PropertyChangedEventHandler PropChangeHandler) {
            Type targetType = sourceObject?.GetType();
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(targetType)) {
                //Subscribe to this property change also
                (sourceObject as INotifyPropertyChanged).PropertyChanged -= PropChangeHandler;
                (sourceObject as INotifyPropertyChanged).PropertyChanged += PropChangeHandler;
            }
        }

        public static void FetchValueAndMonitor(object sourceObject, string prop_name,PropertyChangedEventHandler PropChangeHandler,IIconSourceProvider sourceProvider) {
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
                if (tarProp == null) {
                    //Property itself is not available, no point in checking further.}
                    sourceProvider.OnDataChanged(null); //this will send the default image.
                }
                prop_value = tarProp?.GetValue(sourceObject);

                if (string.IsNullOrWhiteSpace(sub_path)) {
                    //THIS IS THE END.
                    //We are at the target property. Setup the final monitor
                    SetupPropertyMonitor(sourceObject, PropChangeHandler); //Which will eventually trigger further new changes.
                    sourceProvider.OnDataChanged(prop_value); //Trigger directly.
                } else {
                    //If subpath is not null, we should also setup a prop change monitor (even if propvalue is not null). So that in future if the value changes, we can reapply the values.
                    if (typeof(INotifyPropertyChanged).IsAssignableFrom(targetType)) {
                        PropertyChangedEventHandler inline_handler = (sender, e) => {
                            //We can directly fetch the method values.
                            //This could be for any property change. Ensure we are at correct place.
                            if (e.PropertyName != path) return; //ignore don't try to change the image.
                            var updated_value = sender.GetType().GetProperty(path)?.GetValue(sender);
                            if (updated_value == null) sourceProvider.OnDataChanged(null);
                            FetchValueAndMonitor(updated_value, sub_path, PropChangeHandler, sourceProvider); //Now prop is ready, continue the probe.
                        };

                        //Subscribe to this property change also
                        (sourceObject as INotifyPropertyChanged).PropertyChanged -= inline_handler;
                        (sourceObject as INotifyPropertyChanged).PropertyChanged += inline_handler;
                    }
                    if (prop_value != null) {
                       FetchValueAndMonitor(prop_value, sub_path, PropChangeHandler, sourceProvider); //Loop further and when we reach the end where there are no more subpaths, we  will trigger the datachange direclty (for first time).
                    }
                }
            } catch (Exception) {
            }
        }
    }
}
