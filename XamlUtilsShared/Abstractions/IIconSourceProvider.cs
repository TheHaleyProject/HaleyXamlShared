using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

#if HMVVM
namespace Haley.Abstractions {
#elif HWPFR
namespace Haley.IconsPack.Abstractions {
#endif
    
    public interface IIconSourceProvider {
        ImageSource IconSource { get;  }
        void OnDataChanged(object input);
    }
#if HMVVM || HWPFR
    }
#endif
