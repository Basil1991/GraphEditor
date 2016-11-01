using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaskFlowDesign.Model;

namespace TaskFlowDesign.Utils {
    public class XMLUtil {
        public void ConvertToMenuXAML(string filePath, string savePath, string configPath) {
            List<ToolBarModel> toolBars = GetToolBars(filePath);
            ToolBarKeeper.List = toolBars;
            string result = this.ConvertToXAML(toolBars, configPath);
            File.WriteAllText(savePath, result);
        }
        public List<ToolBarModel> GetToolBars(string filePath) {
            var doc = loadDoc(filePath);
            var toolBox = doc.ChildNodes.Where(a => a.Name == "ToolBox").FirstOrDefault();
            var controlTypes = toolBox.ChildNodes.Where(a => a.Name == "ControlType");
            List<ToolBarModel> toolBars = new List<ToolBarModel>();
            foreach (var ct in controlTypes) {
                ToolBarModel tooBar = new ToolBarModel();
                tooBar.Header = ct.Attributes.Where(a => a.Name == "name").Value;
                var isExpanderAttr = ct.Attributes.Where(a => a.Name == "isExpander");
                tooBar.IsExpander = isExpanderAttr == null ? false : bool.Parse(isExpanderAttr.Value);
                foreach (XmlNode c in ct.ChildNodes) {
                    tooBar.Items.Add(getToolBarItem(c));
                }
                toolBars.Add(tooBar);
            }
            return toolBars;
        }
        private XmlDocument loadDoc(string filePath) {
            XmlDocument doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(filePath, settings);
            doc.Load(reader);
            return doc;
        }
        private ToolBarItem getToolBarItem(XmlNode c) {
            ToolBarItem item = new ToolBarItem();
            item.Key = c.Attributes.Where(a => a.Name == "key").Value;
            item.Name = c.Attributes.Where(a => a.Name == "name").Value;
            item.ScriptPath = c.Attributes.Where(a => a.Name == "ScriptPath").Value;
            item.ImageSource = c.Attributes.Where(a => a.Name == "ImageSource").Value;
            var typeAttr = c.Attributes.Where(a => a.Name == "type");
            if (typeAttr != null) {
                item.Type = typeAttr.Value;
            }
            if (c.ChildNodes.Has(a => a.Name == "Properties")) {
                var propsNode = c.ChildNodes.Where(a => a.Name == "Properties").FirstOrDefault();
                item.Properties = new List<FlowDesignItemPropertyModel>();
                foreach (XmlNode prop in propsNode.ChildNodes) {
                    FlowDesignItemPropertyModel model = new FlowDesignItemPropertyModel();
                    model.Name = prop.Attributes.Where(a => a.Name == "name").Value;
                    model.Value = prop.Attributes.Where(a => a.Name == "value").Value;
                    model.Filed = prop.Attributes.Where(a => a.Name == "field").Value;
                    item.Properties.Add(model);
                }
            }
            return item;
        }
        public string ConvertToXAML(IEnumerable<ToolBarModel> list, string configPath) {
            string begin = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"" +
                      " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"" +
                      " xmlns:s=\"clr-namespace:DiagramDesigner;assembly=DiagramDesigner\"" +
                      " xmlns:c=\"clr-namespace:DiagramDesigner.Controls;assembly=DiagramDesigner\" >" +
                      " <ScrollViewer x:Key = \"ZFToolBox\" VerticalScrollBarVisibility=\"Hidden\"> <StackPanel Grid.Column = \"0\" Margin = \"0,0,5,0\" > ";
            string content = getContentByList(list, configPath);
            string end = " </StackPanel></ScrollViewer></Grid>";
            return begin + content + end;
        }
        private string getContentByList(IEnumerable<ToolBarModel> list, string configPath) {
            string content = "";
            foreach (var l in list) {
                content += getContentByModel(l, configPath);
            }
            return content;
        }
        private string getContentByModel(ToolBarModel model, string configPath) {
            string begin = string.Format("<Expander  Header = \"{0}\"  IsExpanded = \"{1}\" >  <s:Toolbox> <ItemsControl.Items>", model.Header, model.IsExpander);
            string content = "";
            model.Items.ForEach(l => {
                content += getContentByItem(l, configPath);
            });
            string end = " </ItemsControl.Items></s:Toolbox></Expander>";
            return begin + content + end;
        }
        private string getContentByItem(ToolBarItem item, string configPath) {
            string content = string.Format(" <Grid  x:Name=\"{0}\" Tag=\"{1}\" IsHitTestVisible=\"false\" >", item.Key, item.Type == null ? "" : item.Type) +
                string.Format(" <Image Source=\"{0}\" MinWidth=\"0\" MaxWidth=\"100\"  />", configPath + item.ImageSource) +
                string.Format(" <Label  HorizontalAlignment=\"Center\" HorizontalContentAlignment=\"Center\" Content=\"{0}\"  VerticalContentAlignment=\"Center\"/>", item.Name) +
            "<TextBlock  Foreground=\"Red\" FontSize=\"10\" TextWrapping=\"Wrap\" HorizontalAlignment=\"Center\" Tag=\"Comment\" VerticalAlignment=\"Bottom\" />";
            if (item.Properties != null) {
                content += string.Format(" <Label Tag=\"Properties\" Visibility=\"Collapsed\" Content=\"{0}\"></Label>", getPropsXaml(item.Properties));
            }
            if (!string.IsNullOrEmpty(item.ScriptPath)) {
                content += string.Format("<Label Visibility=\"Collapsed\" Tag=\"ScriptFile\" Content=\"{0}\"></Label> ", item.ScriptPath);
            }
            return content + "</Grid>";
        }
        private string getPropsXaml(IEnumerable<FlowDesignItemPropertyModel> props) {
            string content = "";
            bool firstTime = true;
            foreach (var p in props) {
                if (firstTime) {
                    content = content + p.Name + "@:" + p.Value + "@:" + p.Filed;
                    firstTime = false;
                }
                else
                    content = content + "@," + p.Name + "@:" + p.Value + "@:" + p.Filed;
            }
            return content;
        }
    }
    public static class XMLUtilExtentions {
        public static List<XmlNode> Where(this XmlNodeList nodeList, Func<XmlNode, bool> express) {
            List<XmlNode> nodes = new List<XmlNode>();
            foreach (XmlNode node in nodeList) {
                if (express(node)) {
                    nodes.Add(node);
                }
            }
            return nodes;
        }
        public static bool Has(this XmlNodeList nodeList, Func<XmlNode, bool> express) {
            List<XmlNode> nodes = new List<XmlNode>();
            foreach (XmlNode node in nodeList) {
                if (express(node)) {
                    return true;
                }
            }
            return false;
        }
        public static XmlAttribute Where(this XmlAttributeCollection attrCollection, Func<XmlNode, bool> express) {
            foreach (XmlAttribute attr in attrCollection) {
                if (express(attr)) {
                    return attr;
                }
            }
            return null;
        }
    }
}
