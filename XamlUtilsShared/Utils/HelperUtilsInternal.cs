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

namespace Haley.Utils
{
    internal sealed class HelperUtilsInternal {
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
                return true;
            } catch (Exception) {
                return false;
            }
        }

    }
}
