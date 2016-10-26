using System;
using System.Threading;
using System.Windows.Forms;

namespace PuppetMaster
{
    public delegate void DelAddLog(string log);
    public delegate void DisableStepByStep();

    public partial class PuppetMasterForm : Form
    {
        private PuppetMaster _puppetMaster;

        public PuppetMasterForm()
        {
            InitializeComponent();
        }

        private void PuppetMasterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(_puppetMaster != null) _puppetMaster.Exit();
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
            if (_puppetMaster == null)
            {
                if (text_file.Text.Equals(""))
                {
                    MessageBox.Show("Please select a config file.");
                    return;
                }
                _puppetMaster = PuppetMaster.GetInstance();
            }
            button_run_all.Enabled = false;
            button_slow_parse.Enabled = false;
            button_browse.Enabled = false;
            button_run_command.Enabled = true;
            text_file.ReadOnly = true;
            text_command_console.ReadOnly = false;

            new Thread(() =>
            {
                _puppetMaster.SetupFullSpeed(this, new DelAddLog(AddLog), text_file.Text);
            }).Start();
        }

        private void button_slow_parse_Click(object sender, EventArgs e)
        {
            if (_puppetMaster == null) {
                if (text_file.Text.Equals(""))
                {
                    MessageBox.Show("Please select a config file.");
                    return;
                }
                _puppetMaster = PuppetMaster.GetInstance();
            }
            button_run_all.Enabled = false;
            button_browse.Enabled = false;
            text_file.ReadOnly = true;
            new Thread(() =>
            {
                _puppetMaster.SetupStepByStep(this, new DelAddLog(AddLog), new DisableStepByStep(DisableStepByStep), text_file.Text);
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
            button_run_command.Enabled = true;
            text_command_console.ReadOnly = false;
        }

        private void button_run_command_Click(object sender, EventArgs e)
        {
            string cmd = (string)text_command_console.Text.Clone();
            new Thread(() =>
            {
                _puppetMaster.RunCommand(cmd);
            }).Start();
            text_command_console.Text = "";
        }

        private void text_command_console_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button_run_command_Click(this, new EventArgs());
            }
        }

        private void RestartApp()
        {
            System.Diagnostics.Process.Start(Application.ExecutablePath); // to start new instance of application
            this.Close(); //to turn off current app
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestartApp();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
