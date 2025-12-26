using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervanaNcMgd
{
    public enum MgdMode
    {
        _Unknown,
        Application,
        Documents,
        Document,
        Database,
        Objects,
        Objects2
    }

    public enum EValue_Type
    {
        Common, //classic parameter
        NotImplemented, //this method is not implemented via API
        NotApplicable //look's like as NotImplemented
    }


    public enum EParameter_Type
    {
        Common, //the general property (simple type)
        CanExplore //the other class ot ObjectId (can explore throw new session)
    }

    /// <summary>
    /// The localization (language) of plugin
    /// </summary>
    public enum LanguageLocaleVariant : int
    {
        Ru = 0,
        En = 1
    }

}
