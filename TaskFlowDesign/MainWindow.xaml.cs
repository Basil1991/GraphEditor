using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskFlowDesign.Model;
using System.IO;
using System.Security.Cryptography;
using TaskFlowDesign.Utils;
using System.Xml;
using System.Windows.Markup;
using MahApps.Metro.Controls.Dialogs;
using System.Threading;
using DiagramDesigner;
using System.Collections.Concurrent;

namespace TaskFlowDesign {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow {
        SynchronizationContext m_SyncContext = null;
        public MainWindow() {
            InitializeComponent();
            checkToolBox();
            m_SyncContext = SynchronizationContext.Current;
            //TaskPreview();
            TaskToolBarSearch();
        }

        #region First Load Tool Box
        private void checkToolBox() {
            using (HashAlgorithm hash = HashAlgorithm.Create()) {
                using (FileStream file1 = new FileStream(Setting.ZFToolBoxControlFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    byte[] hashByte1 = hash.ComputeHash(file1);
                    string hashCode = BitConverter.ToString(hashByte1);
                    string version = File.ReadAllText(Setting.ZFVersionFile);
                    if (hashCode == version) {
                    }
                    else {
                        new XMLUtil().ConvertToMenuXAML(Setting.ZFToolBoxControlFile, Setting.ZFToolBoxXamlFile, Setting.ToolBoxConfig);
                        File.WriteAllText(Setting.ZFVersionFile, hashCode);
                    }
                }
            }
            loadToolBar(Setting.ZFToolBoxXamlFile);
        }
        public void loadToolBar(string file) {
            XmlTextReader xmlreader = new XmlTextReader(file);
            UIElement obj = XamlReader.Load(xmlreader) as UIElement;
            this.ToolBox.Children.Add(obj);
        }
        #endregion

        #region Task Preview
        private void TaskPreview() {
            Task.Run(() => {
                while (true) {
                    Thread.Sleep(1000);
                    m_SyncContext.Post(getPreviewResult, null);
                }
            });
        }
        private void getPreviewResult(object obj) {
            try {
                ScriptGenerateUtil sg = new ScriptGenerateUtil();
                var scrollViewer = (ScrollViewer)(this.MyTab.SelectedContent);
                if (scrollViewer == null)
                    return;
                var designer = (DesignerCanvas)scrollViewer.Content;
                var children = designer.Children;
                //No node
                if (children == null || children.Count == 0) {
                    return;
                }
                //No Start Node
                var start = sg.FindGrid(children, "Start");
                if (start == null) {
                    return;
                }
                string result = sg.GetFinalResultByStartNode((DesignerItem)start, children);
                this.MyPreview.Text = result;
            }
            catch (Exception ex) {
                if (ExceptionSetting.PreviewExTimes < 3) {
                    LogUtil.WriteSync(ex);
                    ExceptionSetting.PreviewExTimes++;
                }
            }
        }
        #endregion

        #region Task ToolBarSearch
        private void TaskToolBarSearch() {
            Task.Run(() => {
                while (true) {
                    Thread.Sleep(ToolBarSearchArgs.Interval);
                    m_SyncContext.Post((objNull) => {
                        ToolBarSearchArgs.OldValue = ToolBarSearchArgs.NewValue;
                        ToolBarSearchArgs.NewValue = this.MySearch.Text.ToUpper().Trim();
                        if (ToolBarSearchArgs.OldValue != ToolBarSearchArgs.NewValue) {
                            toolBarSearch();
                        }
                    }, null);
                }
            });
        }
        private void toolBarSearch() {
            string result;
            XMLUtil xu = new XMLUtil();
            if (ToolBarKeeper.List == null) {
                ToolBarKeeper.List = xu.GetToolBars(Setting.ZFToolBoxControlFile);
            }
            var list = ToolBarKeeper.List;
            ConcurrentBag<ToolBarModel> cb = new ConcurrentBag<ToolBarModel>();
            string text = ToolBarSearchArgs.NewValue;
            if (string.IsNullOrEmpty(text)) {
                result = xu.ConvertToXAML(list, Setting.ConfigDirectory);
            }
            else {
                Parallel.ForEach(list, (l) => {
                    var items = l.Items.Where(a => a.Name.ToUpper().Contains(text)).ToList();
                    if (items.Count > 0) {
                        cb.Add(new ToolBarModel() {
                            Header = l.Header,
                            Items = items
                        });
                    }
                });
                result = xu.ConvertToXAML(cb, Setting.ConfigDirectory);
            }
            byte[] byteArray = Encoding.UTF8.GetBytes(result);
            MemoryStream stream = new MemoryStream(byteArray);
            XmlTextReader xmlreader = new XmlTextReader(stream);
            UIElement obj = XamlReader.Load(xmlreader) as UIElement;
            this.ToolBox.Children.Clear();
            this.ToolBox.Children.Add(obj);
        }
        #endregion
    }
}
