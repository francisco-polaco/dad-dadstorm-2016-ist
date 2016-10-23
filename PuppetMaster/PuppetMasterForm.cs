using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public delegate void DelAddLog(string log);
    public delegate void DisableStepByStep();

    public partial class PuppetMasterForm : Form
    {
        private PuppetMaster mPuppetMaster;

        public PuppetMasterForm()
        {
            InitializeComponent();
        }

        private void PuppetMasterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(mPuppetMaster != null) mPuppetMaster.Exit();
        }

        private void button_browse_Click(object sender, EventArgs e)
        {
            openConfig.ShowDialog();
            text_file.Text = openConfig.FileName;
            button_slow_parse.Enabled = true;
            button_run_all.Enabled = true;
        }

        private void button_run_all_Click(object sender, EventArgs e)
        {
            if (mPuppetMaster == null)
            {
                if (text_file.Text.Equals(""))
                {
                    MessageBox.Show("Please select a config file.");
                    return;
                }
                mPuppetMaster = new PuppetMaster();
            }
            button_run_all.Enabled = false;
            button_slow_parse.Enabled = false;
            new Thread(() =>
            {
                mPuppetMaster.SetupFullSpeed(this, new DelAddLog(AddLog), text_file.Text);
            }).Start();
        }

        private void button_slow_parse_Click(object sender, EventArgs e)
        {
            if (mPuppetMaster == null) {
                if (text_file.Text.Equals(""))
                {
                    MessageBox.Show("Please select a config file.");
                    return;
                }
                mPuppetMaster = new PuppetMaster();
            }
            button_run_all.Enabled = false;
            new Thread(() =>
            {
                mPuppetMaster.SetupStepByStep(this, new DelAddLog(AddLog), new DisableStepByStep(DisableStepByStep), text_file.Text);
            }).Start();
        }

        private void button_clear_log_Click(object sender, EventArgs e)
        {
            text_log.Text = "";
        }

        void AddLog(string log)
        {
            if (text_log.Text == "")
            {
                text_log.Text = log;
            }
            else {
                text_log.AppendText("\r\n" + log);
            }
        }

        void DisableStepByStep()
        {
            button_slow_parse.Enabled = false;
        }
    }
}
