using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CysmicEngine.Editor
{
    public class Inspector : Canvas
    {
        public Inspector()
        {
            Name = "Inspector";
            Text = Name;

            Show();
        }
    }
}
