using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervanaCommonMgd.Common
{
    public partial class CSoftParametersFile
    {
        public static CSoftParametersFile? ConvertFromRevit(RevitSharedParametersFile? revitData)
        {
            if (revitData == null) return null;

            CSoftParametersFile csParamsXml = new CSoftParametersFile();

            foreach (RevitSharedParametersFile.ParamDefinition revitParamDef in revitData.Parameters)
            {

            }


            return csParamsXml;
        }
    }
}
