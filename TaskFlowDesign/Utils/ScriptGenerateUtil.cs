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
using TaskFlowDesign.Model;

namespace TaskFlowDesign.Utils {
    public class ScriptGenerateUtil {
        static LanguageDecorate ld = LanguageFactory.Create(LanguageType.Lua);
        public UIElement FindGrid(UIElementCollection es, string name) {
            foreach (var e in es) {
                if (e is DesignerItem) {
                    DesignerItem item = (DesignerItem)e;
                    if (checkNodeResult(item, name)) {
                        return item;
                    }
                }
            }
            return null;
        }
        public string GetFinalResultByStartNode(DesignerItem source, UIElementCollection es) {
            return GetNodeResult("", source, es) + "\r\n" + ld.EndStr();
        }
        private string GetNodeResult(string previousContent, DesignerItem source, UIElementCollection es, DesignerItem untiltem = null, bool isMulti = false) {
            string content = "";
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
                    content = content + "\r\n" + GetNodeResult(content, singleSink, es, untiltem);
                }
                else {
                    content = ld.Decorate(previousContent, content);
                }
            }
            //
            else if (nextNodes != null &&
                nextNodes.Count > 1) {
                var Sinks = nextNodes;
                DesignerItem itemEnd = new DesignerItem();
                content = content + "\r\n" + getMultiItemsScript(content, Sinks, es, ref itemEnd);
                content = content + "\r\n" + GetNodeResult(content, itemEnd, es, untiltem = null, true);
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
                        var props = label.Content.ToString().Split(',');

                        foreach (var prop in props) {
                            var arr = prop.Split(':');
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
        private string getMultiItemsScript(string previousContent, List<DesignerItem> items, UIElementCollection es, ref DesignerItem endItem) {
            List<DesignerItem> ifItems = null;
            List<DesignerItem> elseItems = null;
            List<DesignerItem> itemsGroupOne = new List<DesignerItem>();
            itemsGroupOne.Add(items[0]);
            var lastItem = getEndItem(items[0], es);
            if (lastItem != null) {
                itemsGroupOne.Add(lastItem);
            }
            if (ifTimes == 0) {
                ifItems = itemsGroupOne;
            }
            else {
                elseItems = itemsGroupOne;
            }
            List<DesignerItem> itemsGroupTwo = new List<DesignerItem>();
            itemsGroupTwo.Add(items[1]);
            lastItem = getEndItem(items[1], es);
            if (lastItem != null) {
                itemsGroupTwo.Add(lastItem);
            }
            if (ifItems == null) {
                ifItems = itemsGroupTwo;
            }
            else {
                elseItems = itemsGroupTwo;
            }
            ifTimes = 1;
            string ifScripts = getIfItemsScript(previousContent, ifItems, es);
            string elseScripts = getElseItemsScript(ifScripts, elseItems, es);

            endItem = ifItems[ifItems.Count - 1];
            return ifScripts + "\r\n" + elseScripts;
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
                    return getEndItem(node, es);
                }
            }
            return null;
        }
        private string getIfItemsScript(string previousContent, List<DesignerItem> items, UIElementCollection es) {
            string ifItemsScript = GetNodeResult(previousContent, items[0], es, items[items.Count - 1]);
            return ifItemsScript;
        }
        private string getElseItemsScript(string previousContent, List<DesignerItem> items, UIElementCollection es) {
            string elseStr = ld.Decorate(previousContent, ld.ElseStr());
            string elseItemsScript = elseStr + "\r\n" + GetNodeResult(elseStr, items[0], es);
            return elseItemsScript;
        }
    }
}
