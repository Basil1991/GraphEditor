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
        public void CommondBinding() {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, new_Executed));

            ScriptGenerateCommand.InputGestures.Add(new KeyGesture(Key.F5));
            this.CommandBindings.Add(new CommandBinding(ScriptGenerateCommand, create_Executed));
            this.mCreate.Command = ScriptGenerateCommand;

            ToolBoxCollapseCommand.InputGestures.Add(new KeyGesture(Key.F10));
            this.CommandBindings.Add(new CommandBinding(ToolBoxCollapseCommand, toolboxCollapse_Excuted));
            this.mCollapse.Command = ToolBoxCollapseCommand;

            ToolBoxExpanseCommand.InputGestures.Add(new KeyGesture(Key.F11));
            this.CommandBindings.Add(new CommandBinding(ToolBoxExpanseCommand, toolboxExpanse_Excuted));
            this.mExpanse.Command = ToolBoxExpanseCommand;
            // this.CommandBindings.Add(new CommandBinding(SingleCloseTabCommand, tabClose_Excuted));
        }
        public static RoutedCommand ScriptGenerateCommand = new RoutedCommand();
        public static RoutedCommand ToolBoxCollapseCommand = new RoutedCommand();
        public static RoutedCommand ToolBoxExpanseCommand = new RoutedCommand();

        //private RoutedCommand SingleCloseTabCommand = new RoutedCommand();
        private void new_Executed(object sender, ExecutedRoutedEventArgs e) {
            addTagItem();
        }
        private void create_Executed(object sender, ExecutedRoutedEventArgs e) {
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
        private void tabClose_Excuted(object sender, ExecutedRoutedEventArgs e) {
        }
        private void toolboxCollapse_Excuted(object sender, ExecutedRoutedEventArgs e) {
            var children = this.ToolBox.FindChildren<Expander>();
            foreach (var child in children) {
                child.IsExpanded = false;
            }
        }
        private void toolboxExpanse_Excuted(object sender, ExecutedRoutedEventArgs e) {
            var children = this.ToolBox.FindChildren<Expander>();
            foreach (var child in children) {
                child.IsExpanded = true;
            }
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
            " Header=\"" + header + "\" Tag=\"" + maxTag + "\" CloseButtonEnabled=\"True\" " +
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
            //tabItem.CloseTabCommand = SingleCloseTabCommand;
            tabControl.Items.Add(tabItem);
            tabControl.SelectedItem = tabItem;
        }
    }
}
