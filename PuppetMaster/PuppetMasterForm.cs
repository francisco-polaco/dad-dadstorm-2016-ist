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
        private PuppetMaster mPuppetForm;

        public PuppetMasterForm()
        {
            mPuppetForm = new PuppetMaster(null);
            InitializeComponent();
        }
    }
}
