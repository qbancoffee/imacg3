﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace crtcpl
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Mutex m = new Mutex(true, "crtcpl", out bool result);

            if (!result)
            {
                try
                {
#if !MONO
                    // Best effort switch to active instance
                    Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
                    foreach (Process p in procs)
                    {
                        if (p.Id == Process.GetCurrentProcess().Id)
                        {
                            continue;
                        }

                        if (p.MainWindowHandle == IntPtr.Zero)
                        {
                            continue;
                        }

                        const int SW_SHOW = 5;

                        NativeMethods.SetForegroundWindow(p.MainWindowHandle);
                        NativeMethods.ShowWindow(p.MainWindowHandle, SW_SHOW);

                        break;
                    }
#else
                    // Sorry, don't know what to do.
                    MessageBox.Show(StringRes.StringRes.AlreadyRunning, StringRes.StringRes.AlreadyRunningTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                }
                catch (Exception)
                {
#if DEBUG
                    throw;
#endif
                }

                return;
            }

            try
            {
                if (Settings.Default.NeedsUpgrade)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.NeedsUpgrade = false;
                    Settings.Default.Save();
                }

                if (!string.IsNullOrWhiteSpace(Settings.Default.SerialPort))
                {
                    try
                    {
                        UCCom.Open(Settings.Default.SerialPort, Settings.Default.SerialRate);
                    }
                    catch (UCComException)
                    {
                        Settings.Default.SerialPort = null;
                    }
                }

                using (AppletForm a = new AppletForm())
                {
                    Application.Run(a);
                }
            }
            finally
            {
                m.ReleaseMutex();
                m.Dispose();

                try
                {
                    if (UCCom.IsOpen)
                    {
                        UCCom.Close();
                    }
                }
                catch (Exception) { }

                Application.Exit();
            }
        }
    }
}
