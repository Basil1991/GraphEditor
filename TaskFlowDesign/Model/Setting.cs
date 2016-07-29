using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlowDesign.Model {
    public class Setting {
        public static string ApplicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
        //ToolBox Icon Path...
        public const string ToolBoxConfig = "..";
        public const string ScriptPathName = "ScriptPath";
        public static string ZFDirectory = ApplicationDirectory + "Config\\ZF\\";
        public static string ZFVersionFile = ZFDirectory + "Version.txt";
        public static string ZFToolBoxControlFile = ZFDirectory + "ToolBoxControlConfig.xml";
        public static string ZFToolBoxXamlFile = ZFDirectory + "ToolBoxControlConfig.xaml";
        public static string ZFScriptDirectory = ZFDirectory + "ScriptTemplate\\";
        public static string ConfigDirectory = ApplicationDirectory + "Config\\";

        public static string LogFile = AppDomain.CurrentDomain.BaseDirectory + "ExceptionLog.txt";
    }
}
