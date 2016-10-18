using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class PuppetMasterForm : Form
    {
        private PuppetMaster mPuppetMaster;
        private FullLog mLog = new FullLog();

        public PuppetMasterForm()
        {
            mPuppetMaster = new PuppetMaster(null);
            InitializeComponent();
            mLog.Update("batata");
        }

        private void PuppetMasterForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Determine if text has changed in the textbox by comparing to original text.
            mPuppetMaster.Exit();
            mLog.Exit();
            e.Cancel = true;
        }
    }
}
