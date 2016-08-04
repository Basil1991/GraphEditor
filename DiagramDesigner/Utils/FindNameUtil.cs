using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiagramDesigner.Utils {
    public static class FindNameUtil {
        public static object FindNameByUtil(this FrameworkElement e, string name) {
            object obj = null;
            obj = e.FindName(name);
            if (obj == null && e.Parent != null) {
                return FindNameByUtil((FrameworkElement)e.Parent, name);
            }
            return obj;
        }
    }
}
