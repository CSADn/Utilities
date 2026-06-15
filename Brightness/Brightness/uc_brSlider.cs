using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brightness
{
    public partial class uc_brSlider : UserControl
    {
        public RichInfoScreen riScreen;
        private bool _isMouseDown = false;

        public uc_brSlider()
        {
            InitializeComponent();
            this.trackBar1.Scroll += new EventHandler(this.trackBar1_Scroll);
            this.trackBar1.MouseUp += new MouseEventHandler(this.TrackBar1_MouseUp);
            this.trackBar1.MouseDown += new MouseEventHandler(this.TrackBar1_MouseDown);
            this.label1.TextChanged += new EventHandler(this.Label1_TextChanged);
        }

        private void Label1_TextChanged(object sender, EventArgs e)
        {
            Task.Factory.StartNew((Action) (() => base.Invoke((MethodInvoker) delegate {
                var form = base.FindForm() as Form1;
                if (form != null)
                {
                    form.UpdateNotifyIconText();
                }
            })));
        }

        private void TrackBar1_MouseDown(object sender, MouseEventArgs e)
        {
            this._isMouseDown = true;
        }

        private void TrackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            this._isMouseDown = false;
            this.trackBar1_Scroll(null, null);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int num = this.riScreen.SetBrightness(this.trackBar1.Value, this._isMouseDown);
            if (num != -1)
            {
                this.label1.Text = num.ToString();
            }
        }

        public void UpdateSliderControl()
        {
            int brightness = this.riScreen.GetBrightness();
            this.label1.Text = brightness.ToString();
            if (brightness != -1)
            {
                this.trackBar1.Value = brightness;
            }
        }

        public string NotifyIconText
        {
            get
            {
                string[] textArray1 = new string[] { " ", this.riScreen.TooltipTitle, " : ", this.label1.Text, "%" };
                return string.Concat(textArray1);
            }
        }
    }
}

