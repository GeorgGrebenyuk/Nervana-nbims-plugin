using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervanaCommonMgd.Configs
{
    public class MgdExplorerReflectionConfig : IConfigBase
    {
        public MgdExplorerReflectionConfig()
        {
            MgdLibraries = new string[] { "hostmgd.dll", "hostdbmgd.dll", "imapimgd.dll", "mapibasetypes.dll", "mapimgd.dll", "hostPointCloudsMgd.dll", "ncBIMSmgd.dll" };
        }

        public override void Save()
        {
            IConfigBase.SaveTo(null, this);
        }

        public string[]? MgdLibraries { get; set; }


    }
}
