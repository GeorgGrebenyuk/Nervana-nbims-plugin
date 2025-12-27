using System;
using System.Collections.Generic;
using System.Windows.Forms.Integration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using NervanaNcComMgd.UI.Controls;

namespace NervanaNcComMgd
{
    enum PaletteType
    {
        ParametersExplorerSpace
    }

    class NervanaUI_PaletteDef
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Caption { get; set; }

        public UserControl? Control { get; set; }

        public static NervanaUI_PaletteDef Create_ParametersExplorerSpace()
        {
            return new NervanaUI_PaletteDef()
            {
                Id = Guid.Parse("{167049f9-7212-46e4-851b-5c40ab85df04}"),
                Name = "Nervana_ParametersExplorerSpace",
                Caption = "Обозреватель параметров COM",
                Control = new Nervana_ParametersExplorerSpace()
            };
        }
    }


    internal class NervanaUI_PaletteManager
    {
        static Dictionary<PaletteType, HostMgd.Windows.PaletteSet?>? mPalettes;

        public static void CreatePalette(PaletteType palType)
        {
            if (mPalettes == null) mPalettes = new Dictionary<PaletteType, HostMgd.Windows.PaletteSet?>();
            if (!mPalettes.ContainsKey(palType))
            {
                if (palType == PaletteType.ParametersExplorerSpace)
                {
                    mPalettes[palType] = CreatePalette2(NervanaUI_PaletteDef.Create_ParametersExplorerSpace());
                }
            }
            else if (mPalettes[palType] != null) mPalettes[palType].Visible = true;
        }

        private static HostMgd.Windows.PaletteSet? CreatePalette2(NervanaUI_PaletteDef? paletteDef)
        {
            if (paletteDef == null) return null;
            HostMgd.Windows.PaletteSet psSet = new HostMgd.Windows.PaletteSet(paletteDef.Name, paletteDef.Caption, paletteDef.Id);
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
}
