using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TaskFlowDesign.Utils {
    public class LuaUtil : LanguageDecorate {
        const int indentCount = 4;
        const string keyIfRegex = "(\\s)*if(\\s)";
        const string keyElseRegex = "(\\s)*else(\\s)*";
        const string keyFunctionRegex = "(\\s)*function(\\s)";
        const string keyStartSpace = "^(\\s)*";

        bool lastPreRowIfRegex;
        bool lastPreRowElseRegex;
        bool lastPreRowFunctionRegex;
        bool firstContentRowIfRegex;
        bool firstContentRowElseRegex;

        public string Decorate(string previousContent, string content) {
            int spaceCount = getSpaceCount(previousContent);
            return getResult(spaceCount, content);
        }
        public string EndStr() {
            return "end";
        }
        public string ElseStr() {
            return "else";
        }
        private int getSpaceCount(string previousContent) {
            string previousLastRow = "";
            int SpaceCount = 0;
            if (!previousContent.Contains("\r\n")) {
                previousLastRow = previousContent;
            }
            else {
                var arr = previousContent.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                int arrCount = arr.Count();
                previousLastRow = arr[arrCount - 1];
            }
            //previousContent
            var StartSpaceResult = Regex.Match(previousLastRow, keyStartSpace);
            if (StartSpaceResult.Success == true) {
                SpaceCount = StartSpaceResult.Length;
            }
            lastPreRowFunctionRegex = Regex.Match(previousLastRow, keyFunctionRegex, RegexOptions.IgnoreCase).Success;
            lastPreRowIfRegex = Regex.Match(previousLastRow, keyIfRegex, RegexOptions.IgnoreCase).Success;
            lastPreRowElseRegex = Regex.Match(previousLastRow, keyElseRegex, RegexOptions.IgnoreCase).Success;

            if (lastPreRowIfRegex || lastPreRowElseRegex || lastPreRowFunctionRegex) {
                SpaceCount += indentCount;
            }
            return SpaceCount;
        }
        private string getResult(int spaceCount, string content) {
            string contentFirstRow = "";
            string spaceStr = "";
            string[] arr = null;
            if (!content.Contains("\r\n")) {
                contentFirstRow = content;
            }
            else {
                arr = content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                contentFirstRow = arr[0];
            }
            firstContentRowIfRegex = Regex.Match(contentFirstRow, keyIfRegex, RegexOptions.IgnoreCase).Success;
            firstContentRowElseRegex = Regex.Match(contentFirstRow, keyElseRegex, RegexOptions.IgnoreCase).Success;
            //end indent
            if (content.Trim().ToLower().Replace("\r\n", "") == "end") {
                spaceCount -= indentCount;
            }
            //else indent
            if (firstContentRowElseRegex) {
                spaceCount -= indentCount;
            }

            while (spaceCount > 0) {
                spaceStr += " ";
                spaceCount -= 1;
            }
            if (arr == null) {
                return spaceStr + content;
            }
            else {
                string result = "";
                foreach (var a in arr) {
                    if (result != "") {
                        result += ("\r\n" + spaceStr + a);
                    }
                    else {
                        result += (spaceStr + a);
                    }
                }
                return result;
            }
        }
    }
}
