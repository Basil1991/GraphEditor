using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlowDesign.Utils {
    interface LanguageDecorate {
        string Decorate(string previousContent, string content);
        string EndStr();
        string ElseStr();
    }
}
