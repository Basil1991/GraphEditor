using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlowDesign.Model;

namespace TaskFlowDesign.Utils {
    public class LanguageFactory {
        internal static LanguageDecorate Create(LanguageType type) {
            switch (type) {
                case LanguageType.Lua: return new LuaUtil();
                default: throw new Exception("Langue Exception");
            }
        }
    }
}
