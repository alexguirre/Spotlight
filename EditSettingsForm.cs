using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spotlight
{
    public partial class EditSettingsForm : Form
    {
        public bool CanCloseDefinitely { get; set; } = false;

        public EditSettingsForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            Hide();

            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CanCloseDefinitely)
            {
                Hide();
                e.Cancel = true;
            }

            base.OnClosing(e);
        }
    }
}
