using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlowDesign.Model {
    public class ToolBarModel {
        public ToolBarModel() {
            this.Items = new List<ToolBarItem>();
        }
        public string Header { get; set; }
        public bool IsExpander { get; set; }
        public List<ToolBarItem> Items { get; set; }
    }

    public class ToolBarItem {
        public string Key { get; set; }
        public string Name { get; set; }
        public string ScriptPath { get; set; }
        public string ImageSource { get; set; }
        public string Type { get; set; }
        public List<FlowDesignItemPropertyModel> Properties { get; set; }
    }
}
