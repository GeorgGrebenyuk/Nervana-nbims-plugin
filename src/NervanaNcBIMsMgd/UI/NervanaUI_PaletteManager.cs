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

    class NervanaUI_PaletteDef
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Name_Palette { get; set; }
        public string Caption { get; set; }

        public UserControl? Control { get; set; }

        public static NervanaUI_PaletteDef Create_AssemblyRefsExplorer()
        {
            return new NervanaUI_PaletteDef()
            {
#if DEBUG
                Id = Guid.Parse("{24e95c1c-838f-48e2-a16a-4fb8c56ca436}"),
                 //Id = Guid.NewGuid(),
#else
                Id = Guid.Parse("{24e95c1c-838f-48e2-a16a-4fb8c56ca436}"),
#endif
                Name = "Nervana_AssemblyRefsExplorer",
                Name_Palette = "Nervana_AssemblyRefsExplorer_Palette",
                Caption = "Обозреватель конструктивных сборок",
                Control = new Nervana_AssemblyRefsExplorer()
            };
        }
    }


    internal class NervanaUI_PaletteManager
    {
        static Dictionary<PaletteType, HostMgd.Windows.PaletteSet>? mPalettes;

        public static void CreatePalette(PaletteType palType)
        {
            if (mPalettes == null) mPalettes = new Dictionary<PaletteType, HostMgd.Windows.PaletteSet>();
            if (!mPalettes.ContainsKey(palType))
            {
                if (palType == PaletteType.AssemblyRefsExplorer)
                {
                    mPalettes[palType] = CreatePalette2(NervanaUI_PaletteDef.Create_AssemblyRefsExplorer());
                }
            }
            else if (mPalettes[palType] != null) mPalettes[palType].Visible = true;
        }

        private static HostMgd.Windows.PaletteSet CreatePalette2(NervanaUI_PaletteDef paletteDef)
        {
            HostMgd.Windows.PaletteSet psSet = new HostMgd.Windows.PaletteSet(paletteDef.Name, paletteDef.Name_Palette, paletteDef.Id);
            psSet.MinimumSize = new System.Drawing.Size(200, 300);

            var hostView = new ElementHost
            {
                AutoSize = false,
                Dock = System.Windows.Forms.DockStyle.Fill,
                Child = paletteDef.Control
            };

            psSet.Add(paletteDef.Caption, hostView);
            return psSet;
        }

    }

    internal class NervanaUI_PaletteManager2
    {
        private static Guid ps_Attrs_id = Guid.Parse("{24e95c1c-838f-48e2-a16a-4fb8c56ca436}");
        static HostMgd.Windows.PaletteSet? ps_Attrs;
        static Nervana_AssemblyRefsExplorer? mExplorer;

        public static void CreatePalette()
        {
            if (ps_Attrs == null)
            {
                var hostView = new ElementHost
                {
                    AutoSize = false,
                    Dock = System.Windows.Forms.DockStyle.Fill,
                    Child = new Nervana_AssemblyRefsExplorer()
                };

                //use constructor with Guid so that we can save/load user data
                ps_Attrs = new HostMgd.Windows.PaletteSet("Nervana_AssemblyRefsExplorer", "Nervana_AssemblyRefsExplorer_Palette", ps_Attrs_id);
                ps_Attrs.MinimumSize = new Size(241, 300);
                ps_Attrs.Size = new Size(241, 300);
                ps_Attrs.Add("Обозреватель конструктивных сборок", hostView);
            }

            ps_Attrs.Visible = true;
        }
    }
}
