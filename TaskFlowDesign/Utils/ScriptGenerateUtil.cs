using DiagramDesigner;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskFlowDesign.Model;

namespace TaskFlowDesign.Utils {
    public class ScriptGenerateUtil {
        static LanguageDecorate ld = LanguageFactory.Create(LanguageType.Lua);
        public UIElement FindMainStart(UIElementCollection es) {
            foreach (var e in es) {
                if (e is DesignerItem) {
                    DesignerItem item = (DesignerItem)e;
                    if (checkNodeResult(item, "Start")) {
                        bool isFirstStart = true;
                        foreach (var c in es) {
                            if (c is Connection) {
                                Connection connector = (Connection)c;
                                if (connector.Sink.ParentDesignerItem == item) {
                                    isFirstStart = false;
                                }
                            }
                        }
                        if (isFirstStart) {
                            return item;
                        }
                    }
                }
            }
            return null;
        }
        public string GetFinalResultByStartNode(DesignerItem source, UIElementCollection es) {
            string funtionContent = "";
            string result = GetNodeResult("", ref funtionContent, source, es) + "\r\n" + ld.EndStr();
            return result + funtionContent;
        }
        private string GetNodeResult(string previousContent, ref string functionContent, DesignerItem source, UIElementCollection es, DesignerItem untiltem = null, bool isMulti = false) {
            string content = "";
            //如果包含多个方法
            if (previousContent != "" && checkNodeResult(source, "Start")) {
                functionContent += ("\r\n" + GetFinalResultByStartNode(source, es));
            }
            else {
                var nextNodes = findNextNodes(source, es);
                content += getScript(source);
                content = ld.Decorate(previousContent, content);
                if (nextNodes != null && nextNodes.Count == 1) {
                    var singleSink = nextNodes.FirstOrDefault();
                    if (singleSink != null) {
                        //直到source=untilItem的时候这次递归终止
                        if (untiltem == singleSink) {
                            return content;
                        }
                        if (isMulti && checkNodeResultByTag(singleSink, "checkEnd")) {
                            return content;
                        }
                        string singleResult = GetNodeResult(content, ref functionContent, singleSink, es, untiltem, isMulti);
                        if (singleResult != "") {
                            content = content + "\r\n" + singleResult;
                        }
                    }
                    else {
                        content = ld.Decorate(previousContent, content);
                    }
                }
                //
                else if (nextNodes != null &&
                    nextNodes.Count == 2) {
                    var Sinks = nextNodes;
                    bool isCheck = true;
                    //如果是分支语句
                    foreach (var sink in Sinks) {
                        if (checkNodeResult(sink, "Start")) {
                            isCheck = false;
                        }
                    }
                    //如果是判断语句
                    if (isCheck == true) {
                        DesignerItem itemEnd = new DesignerItem();
                        content = content + "\r\n" + getMultiItemsScript(content, ref functionContent, Sinks, es, ref itemEnd);
                        content = content + "\r\n" + GetNodeResult(content, ref functionContent, itemEnd, es, untiltem = null, isMulti);
                    }
                    //如果是分支语句
                    else {
                        foreach (var sink in Sinks) {
                            if (checkNodeResult(sink, "Start")) {
                                GetNodeResult(content, ref functionContent, sink, es);
                            }
                            else {
                                content = content + "\r\n" + GetNodeResult(content, ref functionContent, sink, es);
                            }
                        }
                    }
                }
            }
            //
            return content;
        }
        private bool checkNodeResult(DesignerItem item, string name) {
            var objs = item.GetChildObjects();
            foreach (var obj in objs) {
                if (obj is Grid) {
                    var grid = (Grid)obj;
                    if (!string.IsNullOrEmpty(grid.Name) && grid.Name == name) {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool checkNodeResultByTag(DesignerItem item, string name) {
            var objs = item.GetChildObjects();
            foreach (var obj in objs) {
                if (obj is Grid) {
                    var grid = (Grid)obj;
                    if (!string.IsNullOrEmpty(grid.Tag.ToString()) && grid.Tag.ToString() == name) {
                        return true;
                    }
                }
            }
            return false;
        }
        private List<DesignerItem> findNextNodes(DesignerItem from, UIElementCollection es) {
            List<DesignerItem> items = null;
            foreach (var e in es) {
                if (e is Connection) {
                    Connection connector = (Connection)e;
                    if (connector.Source.ParentDesignerItem == from) {
                        if (items == null) {
                            items = new List<DesignerItem>();
                        }
                        items.Add(connector.Sink.ParentDesignerItem);
                    }
                }
            }
            return items;
        }
        private DesignerItem findNextNode(DesignerItem from, UIElementCollection es) {
            foreach (var e in es) {
                if (e is Connection) {
                    Connection connector = (Connection)e;
                    if (connector.Source.ParentDesignerItem == from) {
                        return connector.Sink.ParentDesignerItem;
                    }
                }
            }
            return null;
        }
        private string getScript(DesignerItem item) {
            var grid = (Grid)item.GetChildObjects().FirstOrDefault();
            string script = "";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var child in grid.Children) {
                if (child is Label) {
                    var label = (Label)child;
                    if (label.Tag != null && label.Tag.ToString() == "ScriptFile") {
                        if (File.Exists(Setting.ZFScriptDirectory + label.Content)) {
                            script = File.ReadAllText(Setting.ZFScriptDirectory + label.Content);
                        }
                        else {
                            throw new ArgumentException("找不到" + label.Content);
                        }
                    }
                    else if (label.Tag != null && label.Tag.ToString() == "Properties") {
                        var props = label.Content.ToString().Split(new string[] { "@," }, StringSplitOptions.None);

                        foreach (var prop in props) {
                            var arr = prop.Split(new string[] { "@:" }, StringSplitOptions.None);
                            dic.Add(arr[2], arr[1]);
                        }
                    }
                }
            }
            foreach (var kv in dic) {
                script = script.Replace("<%" + kv.Key + "%>", kv.Value);
            }
            return script;
        }
        private string getMultiItemsScript(string previousContent, ref string functionContent, List<DesignerItem> items, UIElementCollection es, ref DesignerItem endItem) {
            List<DesignerItem> ifItems = null;
            List<DesignerItem> elseItems = null;
            List<DesignerItem> itemsGroupOne = new List<DesignerItem>();
            itemsGroupOne.Add(items[0]);
            if (checkNodeResultByTag(items[0], "check")) {
                ifTimes++;
            }
            if (checkNodeResultByTag(items[0], "checkEnd")) {
                ifTimes--;
            }
            var oneLastItem = ifTimes != 0 ? getEndItem(items[0], es) : null;
            if (oneLastItem != null) {
                itemsGroupOne.Add(oneLastItem);
            }
            if (ifTimes == 0) {
                ifItems = itemsGroupOne;
            }
            else {
                elseItems = itemsGroupOne;
            }
            List<DesignerItem> itemsGroupTwo = new List<DesignerItem>();
            itemsGroupTwo.Add(items[1]);
            if (checkNodeResultByTag(items[1], "check")) {
                ifTimes++;
            }
            if (checkNodeResultByTag(items[1], "checkEnd")) {
                ifTimes--;
            }
            var twoLastItem = ifTimes != 0 ? getEndItem(items[1], es) : null;
            if (twoLastItem != null) {
                itemsGroupTwo.Add(twoLastItem);
            }
            if (ifItems == null) {
                ifItems = itemsGroupTwo;
            }
            else {
                elseItems = itemsGroupTwo;
            }
            ifTimes = 1;
            string ifScripts = "";
            if (ifItems.Count > 1) {
                ifScripts = getIfItemsScript(previousContent, ref functionContent, ifItems, es) + "\r\n";
            }
            //Else设置连线颜色
            setElseConnectionColor(elseItems[0], es);
            string elseScripts = getElseItemsScript(ifScripts == "" ? previousContent : ifScripts, ref functionContent, elseItems, es);

            endItem = ifItems[ifItems.Count - 1];

            return ifScripts + elseScripts;
        }
        int ifTimes = 1;
        private DesignerItem getEndItem(DesignerItem item, UIElementCollection es) {
            var nodes = findNextNodes(item, es);
            if (nodes != null && nodes.Count == 1) {
                var node = nodes.FirstOrDefault();
                if (checkNodeResultByTag(node, "check")) {
                    ifTimes++;
                }
                else if (checkNodeResultByTag(node, "checkEnd")) {
                    ifTimes--;
                }
                if (ifTimes != 0) {
                    return getEndItem(node, es);
                }
                else {
                    return node;
                }
            }
            else if (nodes != null && nodes.Count > 1) {
                foreach (var node in nodes) {
                    if (checkNodeResultByTag(node, "check")) {
                        ifTimes++;
                    }
                    else if (checkNodeResultByTag(node, "checkEnd")) {
                        ifTimes--;
                    }
                    //mark
                    var endNode = getEndItem(node, es);
                    if (endNode != null) {
                        return endNode;
                    }
                }
            }
            return null;
        }
        private string getIfItemsScript(string previousContent, ref string functionContent, List<DesignerItem> items, UIElementCollection es) {
            string ifItemsScript = GetNodeResult(previousContent, ref functionContent, items[0], es, items[items.Count - 1], true);
            return ifItemsScript;
        }
        private string getElseItemsScript(string previousContent, ref string functionContent, List<DesignerItem> items, UIElementCollection es) {
            string elseStr = ld.Decorate(previousContent, ld.ElseStr());
            string elseItemsScript = elseStr + "\r\n" + GetNodeResult(elseStr, ref functionContent, items[0], es, isMulti: true);
            return elseItemsScript;
        }
        private void setElseConnectionColor(DesignerItem sink, UIElementCollection es) {
            foreach (var e in es) {
                if (e is Connection) {
                    Connection connector = (Connection)e;
                    if (connector.Sink.ParentDesignerItem == sink) {
                        var path = connector.FindChild<System.Windows.Shapes.Path>("PART_ConnectionPath");
                        if (path != null) {
                            path.Stroke = (Brush)System.ComponentModel.TypeDescriptor.GetConverter(
        typeof(Brush)).ConvertFromInvariantString("Red");
                        }
                    }
                }
            }
        }
    }
}
