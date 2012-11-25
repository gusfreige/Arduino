using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ArduinoDriverInstaller.Properties;

namespace ArduinoDriverInstaller
{
    public partial class FormMain : Form
    {
        List<String> _drivers;
        public FormMain()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            InitializeComponent();

            // Tip
            if (Settings.Default.ShowHint > 0)
            {
                Settings.Default.ShowHint--;
                Settings.Default.Save();
            }
            else
            {
                groupBoxHint.Visible = false;
                this.Height -= 50;
                panelForm.Height += 50;
            }

            // Detect Windows 8
            /*var os = Environment.OSVersion.Version;
            if (os.Major >= 6 && os.Minor >= 2)
                checkBoxInstallCert.Checked = Environment.Is64BitOperatingSystem;*/

        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            _drivers = new List<string>();
            foreach (var d in Directory.GetFiles(".", "*.inf", SearchOption.AllDirectories))
            {
                var f = Path.GetFullPath(d);
                checkedListBoxDrivers.Items.Add(AddInformation(Path.GetFileNameWithoutExtension(d)), true);
                buttonInstall.Enabled = true;
                _drivers.Add(f);
            }
            this.Focus();
        }

        private String AddInformation(string infFile)
        {
            foreach (var s in from string s in Settings.Default.INFDescriptions where s.StartsWith(infFile, StringComparison.InvariantCultureIgnoreCase) select s)
            {
                return s.Split(new[] { '|' }, 2)[1];
            }
            return infFile;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            var installCertificate = false;
            if (checkBoxInstallCert.Checked)
            {
                if (MessageBox.Show("A test certificate will be installed to allow the unsigned drivers in your machine. This can be a security risk, "+
                    "but if you need to use the drivers the alternative is to disable the 'Driver Signature Enforcement' and then install the unsigned drivers." + 
                    Environment.NewLine + Environment.NewLine + "Are you sure you want to continue and install the certificate?", "Install drivers", MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                {
                    // Install certificate
                    installCertificate = true;
                }
                else
                    return;
            }

            if (MessageBox.Show("Depending on your operative system and settings you may view some warnings (mainly because some of the drivers aren't signed)." + 
                Environment.NewLine + Environment.NewLine + "Are you sure you want to install the checked drivers?", "Install drivers", MessageBoxButtons.YesNo, 
                MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                UpdateGUI(false);

                if (installCertificate)
                {
                    labelOK.Text = "Installing certificate...";
                    labelOK.Visible = true;
                    Application.DoEvents();

                    try
                    {
                        foreach (var s in new[] { "ROOT", "TRUSTEDPUBLISHER" })
                        {
                            Process.Start(new ProcessStartInfo("certinstall.exe",
                                                               "-add arduino.cer -s -r localMachine " + s)
                                              {CreateNoWindow = true}).WaitForExit(30000);
                        }
                    }
                    catch
                    {
                    }
                }

                labelOK.Text = "Starting";
                labelOK.Visible = true;
                Dictionary<int, string> list = checkedListBoxDrivers.CheckedIndices.Cast<int>().ToDictionary(d => d, d => _drivers[d]);
                backgroundWorkerInstall.RunWorkerAsync(list);
            }
        }

        private void UpdateGUI(bool b)
        {
            checkedListBoxDrivers.Enabled = b;
            buttonCancel.Enabled = b;
            buttonInstall.Enabled = b && checkedListBoxDrivers.CheckedIndices.Count > 0;
        }

        private static bool InstallDriver(string infFile)
        {
            var args = "dp_add \"" + infFile + "\"";
            var p = RunDevCon(args);
            
            return !(p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd()).ToLower().Contains("failed");
        }

        private static Process RunDevCon(string args)
        {
            var p = new Process() { StartInfo = new ProcessStartInfo("devcon" + (Environment.Is64BitOperatingSystem ? "64" : "86") + ".exe", args) 
            { CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false } };
            p.Start();

            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();
            return p;
        }

        private void checkedListBoxDrivers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var newState = false;
            if (e.NewValue == CheckState.Checked)
                newState = true;
            else
            {
                newState = checkedListBoxDrivers.CheckedIndices.Count > 1;
            }

            buttonInstall.Enabled = newState;
        }

        private void backgroundWorkerInstall_DoWork(object sender, DoWorkEventArgs e)
        {
            var status = new InstallStatus();
            
            foreach (var d in (Dictionary<int,string>)e.Argument)
            {
                if (InstallDriver(d.Value))
                    status.Installed.Add(d.Key);
                else
                    status.Failed.Add(d.Key);

                backgroundWorkerInstall.ReportProgress(0, status);
                Thread.Sleep(100);
            }

            backgroundWorkerInstall.ReportProgress(100, status);
            RunDevCon("rescan");
        }

        

        private void backgroundWorkerInstall_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            labelOK.Text = "Installation complete.";
            MessageBox.Show(checkedListBoxDrivers.CheckedIndices.Count > 0 ? ("Some drivers failed to install. Remember to allow the warnings, check the driver list for the checked ones. " +
            "Unchecked ones were installed properly." + (checkBoxInstallCert.Checked ? "" : " Also you may try to disable the 'Driver Signature Enforcement' for your system.")) : "All the checked drivers should be properly installed now." + Environment.NewLine + Environment.NewLine + 
                "You can close this window now. Enjoy!", "Installation complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            buttonCancel.Text = "Close";
            UpdateGUI(true);
        }

        private void backgroundWorkerInstall_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var s = (InstallStatus)e.UserState;
            var f = s.Failed.Count;
            labelOK.Text = String.Format("{0} driver(s) installed{1}.{2}", s.Installed.Count, f>0? (" (" + f + " failed)"):"", e.ProgressPercentage < 100 ? " Installing next..." : "");

            if (e.ProgressPercentage == 100)
            {
                labelOK.Text = "Scanning hardware...";

                foreach (var i in s.Installed)
                    this.checkedListBoxDrivers.SetItemChecked(i, false);
            }
        }

        private void linkLabelDriver_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var bcdedit = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "bcdedit.exe");
                if(!File.Exists(bcdedit))
                    bcdedit = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "bcdedit.exe");
                if(!File.Exists(bcdedit))
                    bcdedit = Environment.ExpandEnvironmentVariables("%windir%\\Sysnative\\bcdedit.exe");

                var p = RunExe(bcdedit,"");

                //p.BeginOutputReadLine();
                p.WaitForExit(30000);
                //p.Close();

                //MessageBox.Show(p.StandardError.ReadToEnd());
                foreach (var entry in p.StandardOutput.ReadToEnd().Split(new[] { "---------------------------------" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if(entry.Contains("{current}"))
                    {
                        // Found current entry
                        if (!entry.Contains("DISABLE_INTEGRITY_CHECKS"))
                        {
                            if (MessageBox.Show("Driver Signature Enforcement is enabled in this machine. You may disable this feature to install unsigned drivers." + Environment.NewLine + Environment.NewLine +
                                "Do you want to disable Driver Signature Enforcement policy now?", "Driver Signature Enforcement is enabled", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                            {
                                RunExe(bcdedit, "-set loadoptions DDISABLE_INTEGRITY_CHECKS").WaitForExit(30000);
                                RunExe(bcdedit, "-set TESTSIGNING ON").WaitForExit(30000);

                                if (MessageBox.Show("Driver Signature Enforcement is enabled now." + Environment.NewLine + Environment.NewLine +
                                    "To install the unsigned drivers you need to reboot this machine and then open the 'Configure Board Drivers' application from Start/Arduino subfolder." + Environment.NewLine + Environment.NewLine + "Do you want to reboot your PC now?", "Reboot PC", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                                    RunExe("shutdown.exe", "-r -t 0");
                            }
                        }
                        else
                        {
                            if (MessageBox.Show("Driver Signature Enforcement is already disabled in this machine. You should be able to install any driver now." + Environment.NewLine + Environment.NewLine +
                                "Do you want to restore Driver Signature Enforcement policy?", "Driver Signature Enforcement is already disabled", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                            {
                                RunExe(bcdedit, "/deletevalue loadoptions").WaitForExit(30000);
                                RunExe(bcdedit, "-set TESTSIGNING OFF").WaitForExit(30000);

                                MessageBox.Show("You will have to reboot your machine to fully restore these settings.", "Reboot PC", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("There is no Boot Configuration Data Editor (BCD) in this machine.", "No BCDEdit");
            }
        }

        private static Process RunExe(string bcdedit, string args)
        {
            return Process.Start(new ProcessStartInfo(bcdedit, args) { CreateNoWindow = true, 
                RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false });
        }
    }

    internal class InstallStatus
    {
        public List<int> Failed;
        public List<int> Installed;

        public InstallStatus()
        {
            Failed = new List<int>();
            Installed = new List<int>();
        }
    }
}
