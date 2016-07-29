using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiagramDesigner.Model {

    public enum StackType {
        Add,
        Delete
    }
    public class StackObject {
        public StackObject(List<UIElement> es, StackType type) {
            this.Value = Sort(es, type);
            this.Type = type;
        }
        public StackType Type { get; set; }
        public List<UIElement> Value { get; set; }
        private List<UIElement> Sort(List<UIElement> es, StackType type) {
            List<UIElement> items = new List<UIElement>();
            List<UIElement> cons = new List<UIElement>();
            List<UIElement> list = new List<UIElement>();
            foreach (var e in es) {
                if (e is DesignerItem) {
                    items.Add(e);
                }
                else if (e is Connection) {
                    cons.Add(e);
                }
            }
            if (type == StackType.Delete) {
                list.AddRange(items);
                list.AddRange(cons);
            }
            else {
                list.AddRange(cons);
                list.AddRange(items);
            }
            return list;
        }
    }
    public class UndoStack {
        public static Stack<StackObject> Stack = new Stack<StackObject>();
    }
    public class RedoStack {
        public static Stack<StackObject> Stack = new Stack<StackObject>();
    }
}
