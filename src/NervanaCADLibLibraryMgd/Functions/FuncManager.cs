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
        ImportRevitSharedParametersFile2,
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
            if (!CADLibData.CADLIB_Library.IsAccessible) return;

            switch (funcType)
            {
                case FuctionVariant.ImportRevitSharedParametersFile:
                    RevitSharedParamsIO.CreateInstance().Import(false);
                    break;
                case FuctionVariant.ImportRevitSharedParametersFile2:
                    RevitSharedParamsIO.CreateInstance().Import(true);
                    break;
                case FuctionVariant.ExportRevitSharedParametersFile:
                    RevitSharedParamsIO.CreateInstance().Export();
                    break;
            }
        }



        private static FuncManager? mInstance = null;
    }
}
