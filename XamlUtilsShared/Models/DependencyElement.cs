using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

#if HMVVM
namespace Haley.Models {
#elif HWPFR
namespace Haley.IconsPack.Models {
#endif
public class DependencyElement {
        public FrameworkElement TargetObject { get; set; }
        public DependencyProperty TargetProperty { get; set; }
        public object DataContext { get; set; }
    }

#if HMVVM || HWPFR
}
#endif

