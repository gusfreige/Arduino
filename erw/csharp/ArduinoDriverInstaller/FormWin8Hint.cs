using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ArduinoDriverInstaller.Properties;

namespace ArduinoDriverInstaller
{
    public partial class FormWin8Hint : Form
    {
        public FormWin8Hint()
        {
            InitializeComponent();
        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowWin8Hint--;
            Settings.Default.Save();
            Application.Exit();
        }

        private void buttonHide_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowWin8Hint--;
            Settings.Default.Save();
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormWin8Hint_Load(object sender, EventArgs e)
        {
            webBrowserHelp.Navigate(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "win8hint/help.htm"));
        }

        private void linkLabelPrint_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            webBrowserHelp.ShowPrintDialog();
        }
    }
}
