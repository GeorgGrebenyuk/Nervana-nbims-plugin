using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;

using Teigha.DatabaseServices;
using Teigha.Runtime;
using Teigha.Geometry;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using HostMgd.Windows;

using NervanaNcBIMsMgd.UI.Controls;

namespace NervanaNcBIMsMgd.Functions.ParametersEditor
{
    internal class Palette
    {
        static HostMgd.Windows.PaletteSet? ps;
        public static void CreatePalette()
        {
            if (ps == null)
            {
                //use constructor with Guid so that we can save/load user data
                ps = new HostMgd.Windows.PaletteSet("Объект BIM Строительство", "BIMSDataPalette2", new Guid("C150FB2D-7767-48DD-84AB-28063133B4F7"));
                ps.MinimumSize = new System.Drawing.Size(241, 300);
                ps.Size = new System.Drawing.Size(241, 300);

                var hostView = new ElementHost
                {
                    AutoSize = false,
                    Dock = System.Windows.Forms.DockStyle.Fill,
                    Child = new Nervana_ParametersEditor()
                };

                ps.Add("Структура объекта", hostView);
            }
            ps.Visible = true;
        }
    }
}
