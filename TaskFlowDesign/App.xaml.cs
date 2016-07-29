using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TaskFlowDesign.Model;
using TaskFlowDesign.Utils;

namespace TaskFlowDesign {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        public App() {
#if RELEASE
            AppDomain.CurrentDomain.UnhandledException += Current_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
#endif
        }
        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            LogUtil.WriteSync(e.Exception);
            MessageBox.Show("未被捕获的异常，请记下操作步骤尝试是否可以重现，并联系开发人员", "未捕获的异常", MessageBoxButton.OK, MessageBoxImage.Information);
            e.Handled = true;
        }
        void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            if (e.ExceptionObject is Exception) {
                LogUtil.WriteSync((Exception)e.ExceptionObject);
            }
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题，该操作已经终止，请进行重试，如果问题继续存在，请联系管理员.", "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
