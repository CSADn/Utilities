using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSFGUI
{
    public static class Extensions
    {
        public static void Invoke(this Control input, Action action)
        {
            input.Invoke((Delegate)action);
        }
    }
}
