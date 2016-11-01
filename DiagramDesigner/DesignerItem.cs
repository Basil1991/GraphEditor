using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DiagramDesigner.Controls;
using DiagramDesigner.Utils;

namespace DiagramDesigner {
    //These attributes identify the types of the named parts that are used for templating
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_ResizeDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ConnectorDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    public class DesignerItem : ContentControl, ISelectable, IGroupable {
        #region ID
        private Guid id;
        public Guid ID {
            get { return id; }
        }
        #endregion

        #region ParentID
        public Guid ParentID {
            get { return (Guid)GetValue(ParentIDProperty); }
            set { SetValue(ParentIDProperty, value); }
        }
        public static readonly DependencyProperty ParentIDProperty = DependencyProperty.Register("ParentID", typeof(Guid), typeof(DesignerItem));
        #endregion

        #region IsGroup
        public bool IsGroup {
            get { return (bool)GetValue(IsGroupProperty); }
            set { SetValue(IsGroupProperty, value); }
        }
        public static readonly DependencyProperty IsGroupProperty =
            DependencyProperty.Register("IsGroup", typeof(bool), typeof(DesignerItem));
        #endregion

        #region IsSelected Property

        public bool IsSelected {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register("IsSelected",
                                       typeof(bool),
                                       typeof(DesignerItem),
                                       new FrameworkPropertyMetadata(false));

        #endregion

        #region DragThumbTemplate Property

        // can be used to replace the default template for the DragThumb
        public static readonly DependencyProperty DragThumbTemplateProperty =
            DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetDragThumbTemplate(UIElement element) {
            return (ControlTemplate)element.GetValue(DragThumbTemplateProperty);
        }

        public static void SetDragThumbTemplate(UIElement element, ControlTemplate value) {
            element.SetValue(DragThumbTemplateProperty, value);
        }

        #endregion

        #region ConnectorDecoratorTemplate Property

        // can be used to replace the default template for the ConnectorDecorator
        public static readonly DependencyProperty ConnectorDecoratorTemplateProperty =
            DependencyProperty.RegisterAttached("ConnectorDecoratorTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetConnectorDecoratorTemplate(UIElement element) {
            return (ControlTemplate)element.GetValue(ConnectorDecoratorTemplateProperty);
        }

        public static void SetConnectorDecoratorTemplate(UIElement element, ControlTemplate value) {
            element.SetValue(ConnectorDecoratorTemplateProperty, value);
        }

        #endregion

        #region IsDragConnectionOver

        // while drag connection procedure is ongoing and the mouse moves over 
        // this item this value is true; if true the ConnectorDecorator is triggered
        // to be visible, see template
        public bool IsDragConnectionOver {
            get { return (bool)GetValue(IsDragConnectionOverProperty); }
            set { SetValue(IsDragConnectionOverProperty, value); }
        }
        public static readonly DependencyProperty IsDragConnectionOverProperty =
            DependencyProperty.Register("IsDragConnectionOver",
                                         typeof(bool),
                                         typeof(DesignerItem),
                                         new FrameworkPropertyMetadata(false));

        #endregion
        static DesignerItem() {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }
        public DesignerItem(Guid id) {
            this.id = id;
            this.Loaded += new RoutedEventHandler(DesignerItem_Loaded);
        }
        public DesignerItem()
            : this(Guid.NewGuid()) {
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseDown(e);
            DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

            // update selection
            if (designer != null) {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    if (this.IsSelected) {
                        designer.SelectionService.RemoveFromSelection(this);
                    }
                    else {
                        designer.SelectionService.AddToSelection(this);
                    }
                else if (!this.IsSelected) {
                    designer.SelectionService.SelectItem(this);
                }
                Focus();
            }
            ItemPropertiesShow(((System.Windows.Controls.Panel)((System.Windows.Controls.ContentControl)((System.Windows.FrameworkElement)e.OriginalSource).DataContext).Content));

            e.Handled = false;
        }
        void DesignerItem_Loaded(object sender, RoutedEventArgs e) {
            if (base.Template != null) {
                ContentPresenter contentPresenter =
                    this.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
                if (contentPresenter != null) {
                    UIElement contentVisual = contentPresenter.Content as UIElement;//VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
                    ItemPropertiesShow((Panel)contentVisual);
                    if (contentVisual != null) {
                        DragThumb thumb = this.Template.FindName("PART_DragThumb", this) as DragThumb;
                        if (thumb != null) {
                            ControlTemplate template =
                                DesignerItem.GetDragThumbTemplate(contentVisual) as ControlTemplate;
                            if (template != null)
                                thumb.Template = template;
                        }
                    }
                }
            }
        }
        private void ItemPropertiesShow(Panel p) {
            object propsPanel = this.FindNameByUtil(PropertiesControlHelp.GetName());
            StackPanel sp = ((StackPanel)propsPanel);
            sp.Children.Clear();

            foreach (UIElement c in p.Children) {
                if (c is Image) {
                    SetImageMaxWidth((Image)c);
                }
                else if (c.Visibility == Visibility.Collapsed && c is Label) {
                    Label label = (Label)c;
                    if (label.Tag != null && label.Tag.ToString() == "Properties") {
                        label.Name = "HiddenLabel_" + Guid.NewGuid().ToString("N");
                        RegisterName(label.Name, label);
                        StackPanel grid = new StackPanel();
                        grid.Name = label.Name.Replace("HiddenLabel_", "PropGrid_");
                        var propsStr = label.Content.ToString();
                        var propsArr = propsStr.Split(new string[] { "@," }, StringSplitOptions.None);
                        for (int i = 0; i < propsArr.Length; ++i) {
                            var prop = propsArr[i];
                            StackPanel propPanel = new StackPanel();
                            Label l = new Label();
                            l.Content = prop.Split(new string[] { "@:" }, StringSplitOptions.None)[0];
                            l.Tag = prop.Split(new string[] { "@:" }, StringSplitOptions.None)[2];
                            TextBox t = new TextBox();
                            t.TextChanged += new TextChangedEventHandler(PropertieskeyChanged);
                            t.Text = prop.Split(new string[] { "@:" }, StringSplitOptions.None)[1];
                            propPanel.Children.Add(l);
                            propPanel.Children.Add(t);
                            grid.Children.Add(propPanel);
                            //备注
                            if (i == propsArr.Length - 1) {
                                if (prop.Split(new string[] { "@:" }, StringSplitOptions.None)[0] != "备注") {
                                    StackPanel commentPropPanel = new StackPanel();
                                    Label commnetLabel = new Label();
                                    commnetLabel.Content = "备注";
                                    TextBox commentText = new TextBox();
                                    commentText.TextChanged += new TextChangedEventHandler(PropertieskeyChanged);
                                    commentPropPanel.Children.Add(commnetLabel);
                                    commentPropPanel.Children.Add(commentText);
                                    grid.Children.Add(commentPropPanel);
                                }
                            }
                        }
                        sp.Children.Add(grid);
                    }
                }
            }
        }
        private void PropertieskeyChanged(object sender, TextChangedEventArgs e) {
            object propsPanel = this.FindNameByUtil("ItemProperties");
            StackPanel sp = ((StackPanel)propsPanel);
            if (sp.Children.Count > 0) {
                var grid = (Panel)sp.Children[0];
                var hiddenLabel = (Label)this.FindNameByUtil(grid.Name.Replace("PropGrid_", "HiddenLabel_"));
                string content = "";
                for (int i = 0; i < grid.Children.Count; ++i) {
                    Panel panel = (Panel)grid.Children[i];
                    bool isComment = false;
                    for (int ii = 0; ii < panel.Children.Count; ++ii) {
                        object control = panel.Children[ii];
                        if (control is Label) {
                            Label label = ((Label)control);
                            if (label.Content.ToString() == "备注") {
                                isComment = true;
                            }
                            if (ii == 0) {
                                content += label.Content.ToString() + "@:" + "<%" + i + "%>" + "@:" + label.Tag;
                            }
                            else {
                                content += "," + label.Content.ToString() + "@:" + "<%" + i + "%>" + "@:" + label.Tag;
                            }
                        }
                        else if (control is TextBox) {
                            content = content.Replace("<%" + i + "%>", ((TextBox)control).Text);
                            if (i != grid.Children.Count - 1) {
                                content += "@,";
                            }
                            //备注（这里有优化空间，只要记录修改的是comment才触发，目前没去做）
                            if (isComment) {
                                var chidrens = ((Grid)hiddenLabel.Parent).Children;
                                bool isExistComment = false;
                                foreach (var child in chidrens) {
                                    if (child is TextBlock) {
                                        TextBlock childLabel = (TextBlock)child;
                                        if (childLabel.Tag != null && childLabel.Tag.ToString() == "Comment") {
                                            isExistComment = true;
                                            childLabel.Text = ((TextBox)control).Text;
                                        }
                                    }
                                }
                                //For old version
                                if (isExistComment == false) {
                                    var childLabel = new TextBlock();
                                    childLabel.Tag = "Comment";
                                    childLabel.VerticalAlignment = VerticalAlignment.Bottom;
                                    childLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                    childLabel.Foreground = Brushes.Red;
                                    childLabel.FontSize = 10;
                                    childLabel.TextWrapping = TextWrapping.Wrap;
                                    childLabel.Text = ((TextBox)control).Text;
                                    ((Grid)hiddenLabel.Parent).Children.Add(childLabel);
                                }
                            }
                        }
                    }
                }
                hiddenLabel.Content = content;
            }
            e.Handled = true;
        }
        private void SetImageMaxWidth(Image image) {
            image.MaxWidth = 1000;
        }
    }
}
