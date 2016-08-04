using DiagramDesigner;
using DiagramDesigner.Utils;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
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
            var scrollViewer = (ScrollViewer)(this.MyTab.SelectedContent);
            if (scrollViewer == null)
                return;
            var designer = (DesignerCanvas)scrollViewer.Content;
            var children = designer.Children;
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
        public void CommondBinding() {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New_Executed));
        }
        private void New_Executed(object sender, ExecutedRoutedEventArgs e) {
            addTagItem();
        }
        private void addTagItem() {
            var tabControl = (TabControl)this.FindNameByUtil("MyTab");
            var tabItems = tabControl.Items;
            int maxTag = 0;
            if (tabItems.Count != 0) {
                foreach (TabItem item in tabItems) {
                    var itemNum = Convert.ToInt32(item.Tag);
                    maxTag = itemNum > maxTag ? itemNum : maxTag;
                }
            }
            string header = "设计" + ++maxTag;
            string itemStr = "<mc:MetroTabItem" +
            " xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"" +
            " xmlns:s=\"clr-namespace:DiagramDesigner;assembly=DiagramDesigner\"" +
            " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"" +
            " xmlns:mc=\"http://metro.mahapps.com/winfx/xaml/controls\"" +
            " Header=\"" + header + "\" Tag=\"" + maxTag + "\" CloseButtonEnabled=\"True\" CloseTabCommand=\"{ Binding SingleCloseTabCommand}\"" +
                               " CloseTabCommandParameter = \"{Binding RelativeSource={RelativeSource Self}, Path=Header}\"" +
                               " mc:ControlsHelper.HeaderFontSize = \"15\" >" +
                             " <ScrollViewer HorizontalScrollBarVisibility = \"Auto\" " +
                                " VerticalScrollBarVisibility = \"Auto\" > " +
                                " <s:DesignerCanvas Height = \"10000\" Focusable = \"true\" " +
                            " Background = \"{StaticResource WindowBackgroundBrush}\"" +
                            " Margin = \"10\" FocusVisualStyle = \"{x:Null}\"" +
                            " ContextMenu = \"{StaticResource DesignerCanvasContextMenu}\" >" +
                                " </s:DesignerCanvas>" +
                            " </ScrollViewer>" +
                         "</mc:MetroTabItem> ";
            byte[] byteArray = Encoding.UTF8.GetBytes(itemStr);
            MemoryStream stream = new MemoryStream(byteArray);
            XmlTextReader xmlreader = new XmlTextReader(stream);
            MetroTabItem tabItem = XamlReader.Load(xmlreader) as MetroTabItem;
            tabControl.Items.Add(tabItem);
            tabControl.SelectedItem = tabItem;
        }
    }
}
