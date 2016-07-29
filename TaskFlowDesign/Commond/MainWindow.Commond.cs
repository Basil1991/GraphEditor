using DiagramDesigner;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using TaskFlowDesign.Model;
using TaskFlowDesign.Utils;

namespace TaskFlowDesign {
    public partial class MainWindow : MetroWindow {
        #region Window Closing
        private bool confirm;
        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!confirm) {
                e.Cancel = true;
                var mySetting = new MetroDialogSettings() {
                    NegativeButtonText = "取消",
                    AffirmativeButtonText = "退出",
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    ColorScheme = MetroDialogColorScheme.Accented
                };
                MessageDialogResult result = await this.ShowMessageAsync("注意", "是否确定退出？", MessageDialogStyle.AffirmativeAndNegative, mySetting);
                if (result == MessageDialogResult.Affirmative) {
                    confirm = true;
                    this.Close();
                }
            }
        }
        #endregion

        #region Script Generate
        private void mScriptGenerate_Click(object sender, RoutedEventArgs e) {
#if REALEASE
            try {
#endif
            ScriptGenerateUtil sg = new ScriptGenerateUtil();
            var children = this.MyDesigner.Children;
            //No node
            if (children == null || children.Count == 0) {
                ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("脚本生成提示", "没有任何节点");
                return;
            }
            //No Start Node
            var start = sg.FindGrid(children, "Start");
            if (start == null) {
                ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("脚本生成提示", "找不到开始节点");
                return;
            }
            string result = sg.GetFinalResultByStartNode((DesignerItem)start, children);
            ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("脚本生成提示", result);
#if REALEASE
        }
            catch (Exception ex) {
                LogUtil.WriteSync(ex);
                ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("脚本生成提示", "出现异常:" + ex.Message);
            }
#endif
        }
        #endregion
    }
    #region ToolBar Search 修改为后台线程中Check，避免每一次变动都要记录，改为1S查看一次是否变动
    //private void ToolBarSearch(object sender, TextChangedEventArgs e) {
    //    string result;
    //    XMLUtil xu = new XMLUtil();
    //    if (ToolBarKeeper.List == null) {
    //        ToolBarKeeper.List = xu.GetToolBars(Setting.ZFToolBoxControlFile);
    //    }
    //    var list = ToolBarKeeper.List;
    //    ConcurrentBag<ToolBarModel> cb = new ConcurrentBag<ToolBarModel>();
    //    string text = ((TextBox)sender).Text.ToUpper();
    //    if (string.IsNullOrEmpty(text)) {
    //        result = xu.ConvertToXAML(list, Setting.ConfigDirectory);
    //    }
    //    else {
    //        Parallel.ForEach(list, (l) => {
    //            var items = l.Items.Where(a => a.Name.ToUpper().Contains(text)).ToList();
    //            if (items.Count > 0) {
    //                cb.Add(new ToolBarModel() {
    //                    Header = l.Header,
    //                    Items = items
    //                });
    //            }
    //        });
    //        result = xu.ConvertToXAML(cb, Setting.ConfigDirectory);
    //    }
    //    byte[] byteArray = Encoding.UTF8.GetBytes(result);
    //    MemoryStream stream = new MemoryStream(byteArray);
    //    XmlTextReader xmlreader = new XmlTextReader(stream);
    //    UIElement obj = XamlReader.Load(xmlreader) as UIElement;
    //    this.ToolBox.Children.Clear();
    //    this.ToolBox.Children.Add(obj);
    //}
    #endregion
    //}
}
