using System;

using NervanaCommonMgd;
using NervanaCommonMgd.Common;

namespace NervanaTestsCmd
{
    public class Program
    {
        public static void Main(string[] args)
        {

            CSoftParametersFile? csFile = CSoftParametersFile.LoadFrom(@"E:\Temp\0003.xml");
            csFile?.Save(@"E:\Temp\0003_1.xml");

            Console.WriteLine("\nEnd!");
            Console.ReadKey();
        }
    }
}
