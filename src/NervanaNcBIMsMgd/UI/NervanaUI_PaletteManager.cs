using System;
using System.Collections.Generic;
using System.Windows.Forms.Integration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using NervanaNcBIMsMgd.UI.Controls;
using System.Drawing;

namespace NervanaNcBIMsMgd
{
    enum PaletteType
    {
        AssemblyRefsExplorer
    }

    struct NervanaUI_PaletteDef
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Name_Palette { get; set; }
        public string Caption { get; set; }

        public static NervanaUI_PaletteDef CreateDefault()
        {
            NervanaUI_PaletteDef paletteDef = new NervanaUI_PaletteDef();
            paletteDef.Id = Guid.Empty;
            paletteDef.Name = "";
            paletteDef.Name_Palette = "";
            paletteDef.Caption = "";

            return paletteDef;
        }
    }


    internal class NervanaUI_PaletteManager
    {
        private static HostMgd.Windows.PaletteSet? mPalette_AssemblyRefsExplorer;

        public static void CreatePalette(PaletteType paletteType)
        {
            switch (paletteType)
            {
                case PaletteType.AssemblyRefsExplorer:
                    if (mPalette_AssemblyRefsExplorer == null) mPalette_AssemblyRefsExplorer = createPalette2(createPaletteParameters(paletteType), new Nervana_AssemblyRefsExplorer());
                    if (mPalette_AssemblyRefsExplorer != null) mPalette_AssemblyRefsExplorer.Visible = true;
                    break;
            }
        }

        private static NervanaUI_PaletteDef createPaletteParameters(PaletteType paletteType)
        {
            NervanaUI_PaletteDef paletteDef = NervanaUI_PaletteDef.CreateDefault();
            switch(paletteType)
            {
                case PaletteType.AssemblyRefsExplorer:
                    paletteDef = new NervanaUI_PaletteDef()
                    {
                        Id = Guid.Parse("{24e95c1c-838f-48e2-a16a-4fb8c56ca436}"),
                        Name = "Nervana_AssemblyRefsExplorer",
                        Name_Palette = "Nervana_AssemblyRefsExplorer_Palette",
                        Caption = "Обозреватель конструктивных сборок"
                    };
                    break;
            }
            return paletteDef;
        }

        private static HostMgd.Windows.PaletteSet? createPalette2(NervanaUI_PaletteDef paletteDef, System.Windows.Controls.UserControl control)
        {
            if (paletteDef.Id == Guid.Empty) return null;
            HostMgd.Windows.PaletteSet psSet = new HostMgd.Windows.PaletteSet(paletteDef.Caption, paletteDef.Name_Palette, paletteDef.Id);
            psSet.MinimumSize = new System.Drawing.Size(200, 300);

            var hostView = new ElementHost
            {
                AutoSize = false,
                Dock = System.Windows.Forms.DockStyle.Fill,
                Child = control
            };

            psSet.Add(paletteDef.Name, hostView);
            return psSet;
        }
    }
}
