using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CysmicEngine;

namespace CysmicEngine.Editor
{
    class CysmicEditor
    {
        Canvas window = null;

        public CysmicEditor()
        {
            window = new Canvas();
            window.WindowState = FormWindowState.Maximized;

            Application.Run(window);
        }
    }
}
