using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervanaNcMgd.Common
{
    public static class UserInput
    {
        public static double GetUserInput(string message, double defValue, bool allowNegative = false)
        {
            Editor ed = CommonUtils.CurrentDoc.Editor;

            PromptDoubleOptions opts = new PromptDoubleOptions(message);
            opts.AllowNegative = allowNegative;
            opts.DefaultValue = defValue;
            PromptDoubleResult res = ed.GetDouble(opts);
            if (res.Status == PromptStatus.OK) return res.Value;
            return defValue;
        }

        public static int GetUserInput(string message, int defValue, bool allowNegative = false)
        {
            Editor ed = CommonUtils.CurrentDoc.Editor;

            PromptIntegerOptions opts = new PromptIntegerOptions(message);
            opts.AllowNegative = allowNegative;
            opts.DefaultValue = defValue;
            PromptIntegerResult res = ed.GetInteger(opts);
            if (res.Status == PromptStatus.OK) return res.Value;
            return defValue;
        }

        public static bool GetUserInput(string message, bool defValue)
        {
            Editor ed = CommonUtils.CurrentDoc.Editor;

            PromptIntegerOptions opts = new PromptIntegerOptions(message);
            opts.AllowNegative = false;
            if (defValue == true) opts.DefaultValue = 1;
            else opts.DefaultValue = 0;

            PromptIntegerResult res = ed.GetInteger(opts);
            if (res.Status == PromptStatus.OK) 
            {
                if (res.Value == 1) return true;
                else return false;
            }
            return defValue;
        }
    }
}
