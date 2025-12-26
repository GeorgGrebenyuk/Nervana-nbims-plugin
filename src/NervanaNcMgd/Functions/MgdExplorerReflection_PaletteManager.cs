using System;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NervanaNcMgd.UI.Windows;
using NervanaNcMgd.UI.Controls;

namespace NervanaNcMgd.Functions
{
    internal class MgdExplorerReflection_PaletteManager
    {
        private static Guid ps_Attrs_id = Guid.Parse("{4c382f2b-229d-4b12-b1a1-d8e80ad766e7}");
        static HostMgd.Windows.PaletteSet? ps_Attrs;
        static Nervana_ExplorerSpace? mExplorer;

        public static void CreatePalette()
        {
            if (ps_Attrs == null)
            {
                var hostView = new ElementHost
                {
                    AutoSize = false,
                    Dock  = System.Windows.Forms.DockStyle.Fill,
                    Child = new Nervana_MgdExplorer4Entity(true)
                };

                //use constructor with Guid so that we can save/load user data
                ps_Attrs = new HostMgd.Windows.PaletteSet("MgdExplorerReflection", "MgdExplorerReflectionObject_Palette", ps_Attrs_id);
                ps_Attrs.MinimumSize = new Size(241, 300);
                ps_Attrs.Size = new Size(241, 300);
                ps_Attrs.Add("Обозреватель свойств классов", hostView);
            }

            ps_Attrs.Visible = true;
        }

        //public static void SetData(object? data)
        //{
        //    mExplorer = new Nervana_ExplorerSpace(data);
        //}
    }
}
