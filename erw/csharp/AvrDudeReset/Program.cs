using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace AvrDudeReset
{
    class Program
    {
        static Process _p;
        static void Main(string[] args)
        {
            const string portPrefix = @"-P\\.\COM", baudsPrefix = "-b";
            Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            // Get data from args
            int port = -1, bauds = -1;
            foreach (var arg in args)
            {
                if (arg.StartsWith(portPrefix, StringComparison.InvariantCultureIgnoreCase))
                    port = int.Parse(arg.Remove(0, portPrefix.Length));
                else
                    if (arg.StartsWith(baudsPrefix, StringComparison.InvariantCultureIgnoreCase))
                        bauds = int.Parse(arg.Remove(0, baudsPrefix.Length));

                if (port != -1 && bauds != -1)
                    break;
            }

            // Check if port is available
            if (port != -1 && bauds != -1)
                if (!PortIsOk(port, bauds))
                {

                    // Can't be opened
                    var p = Process.Start("avrdudefix.exe", "-parent" + ParentProc.FindParentProcess().MainWindowHandle);
                    p.WaitForExit();

                    switch (p.ExitCode)
                    {
                        case 2:
                            Console.Error.WriteLine("Serial port 'COM{0}' already in use. Check your board or Click fix on next upload.", port);
                            break;
                        case 9:
                            var timer = Stopwatch.StartNew();

                            do
                            {
                                if (timer.ElapsedMilliseconds > 10000)
                                {
                                    Environment.Exit(1);
                                }
                                Thread.Sleep(1000);
                            } while (!PortIsOk(port, bauds));
                            break;
                        default:
                            Console.WriteLine("Nothing was uploaded to the board. Upload was cancelled.");
                            Environment.Exit(0);
                            break;
                    }
                }

            _p = new Process { StartInfo = new ProcessStartInfo("avrdude2.exe", "\"" + String.Join("\" \"", args) + "\"") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow=true } , EnableRaisingEvents = true };
            _p.OutputDataReceived += p_OutputDataReceived;
            _p.ErrorDataReceived += p_ErrorDataReceived;

            _p.Start();
            _p.WaitForExit();
            Environment.Exit(_p.ExitCode);
        }

        private static bool PortIsOk(int port, int bauds)
        {
            try
            {
                // Check and/or flush
                if (port <= 0 || bauds <= 0)
                    return false;

                var p = new SerialPort("COM" + port, bauds);
                p.Open();

                p.ReadExisting();
                Thread.Sleep(100);
                p.RtsEnable = false;
                p.DtrEnable = false;
                Thread.Sleep(100);
                p.DtrEnable = true;
                p.RtsEnable = true;

                p.Close();
                return true;
            }
            catch { }
            return false;
        }

        static void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.Error.Write(e.Data);
        }

        static void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.Write(e.Data);
        }
    }
}
