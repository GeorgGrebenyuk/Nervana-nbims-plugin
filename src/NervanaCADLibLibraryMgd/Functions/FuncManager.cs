using NervanaCADLibLibraryMgd.Functions.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervanaCADLibLibraryMgd.Functions
{
    internal enum FuctionVariant
    {
        ImportRevitSharedParametersFile,
        ExportRevitSharedParametersFile


    }
    internal class FuncManager
    {
        public static FuncManager CreateInstance ()
        {
            if (mInstance == null) mInstance = new FuncManager();
            return mInstance;
        }

        public void RunCommand(FuctionVariant funcType)
        {
            switch (funcType)
            {
                case FuctionVariant.ImportRevitSharedParametersFile:
                    RevitSharedParamsIO.CreateInstance().Import();
                    break;
                case FuctionVariant.ExportRevitSharedParametersFile:
                    RevitSharedParamsIO.CreateInstance().Export();
                    break;
            }
        }



        private static FuncManager? mInstance = null;
    }
}
