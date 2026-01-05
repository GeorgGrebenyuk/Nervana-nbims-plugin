using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervanaCommonMgd
{
    public enum LogType
    {
        Warning,
        Error,
        Add, //some Info
        Modify,
        Delete
    }
    public static class TraceWriter
    {
        public static void Log(string message, LogType t = LogType.Add)
        {
            System.Diagnostics.Trace.WriteLine($"{t.ToString()}: {message}");
        }
    }
}
