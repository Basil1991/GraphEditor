using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlowDesign.Model;

namespace TaskFlowDesign.Utils {
    public class LogUtil {
        private static object obj = new object();
        public static void WriteSync(Exception ex) {
            DateTime now = DateTime.Now;
            string contents = getContent(ex, now);
            lock (obj) {
                Task.Run(() => File.AppendAllText(Setting.LogFile, contents));
            }
        }
        private static string getContent(Exception ex, DateTime now) {
            string content = "Datetime:" + now + "\r\n";
            content += ("Message:" + ex.Message) + "\r\n";
            content += ("Stack:" + ex.StackTrace) + "\r\n";
            if (ex.InnerException != null) {
                content += getContent(ex.InnerException, now);
            }
            return content + "\r\n";
        }
    }
}
