using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Windows_XP_Message_Box
{
    class Program
    {
        private static TaskDialogStandardIcon dialogIcon = TaskDialogStandardIcon.None;
        private static bool grayCancelable = false;
        private static string text = "";
        private static string caption = "Windows XP Message Box";
        private static string instruction = "Windows XP";
        private static bool hideCancelable = true;
        private static TaskDialogButton button;
        private static int timeout = -1;

        public static void Main(string[] args)
        {
            Program program = new Program();
            program.RunProgram(args);
            while (true) { }
        }

        private async void RunProgram(string[] args)
        {
            string commandLine = Environment.CommandLine;

            #region Icons
            if (commandLine.Contains("-errorIcon") || commandLine.Contains("-err"))
            {
                dialogIcon = TaskDialogStandardIcon.Error;
            }
            else if (commandLine.Contains("-warningIcon") || commandLine.Contains("-warn"))
            {
                dialogIcon = TaskDialogStandardIcon.Warning;
            }
            else if (commandLine.Contains("-infoIcon") || commandLine.Contains("-info"))
            {
                dialogIcon = TaskDialogStandardIcon.Information;
            }
            else if (commandLine.Contains("-uacIcon") || commandLine.Contains("-uac"))
            {
                dialogIcon = TaskDialogStandardIcon.Shield;
            }
            #endregion

            #region Text
            if (commandLine.Contains("-text "))
            {
                text = ParseCommand("-text");
            }
            #endregion

            #region Caption
            if (commandLine.Contains("-caption"))
            {
                caption = ParseCommand("-caption");
            }
            #endregion

            #region Instruction
            if (commandLine.Contains("-instruction"))
            {
                instruction = ParseCommand("-instruction");
            }
            #endregion

            #region X button
            if (commandLine.Contains("-disableX"))
            {
                grayCancelable = true;
            }
            else if (commandLine.Contains("-hideX"))
            {
                hideCancelable = false;
            }
            #endregion

            #region Button text
            if (commandLine.Contains("-buttonText"))
            {
                button = new TaskDialogButton()
                {
                    Text = ParseCommand("-buttonText")
                };
            }
            #endregion

            #region Timeout
            if (commandLine.Contains("-timeout"))
            {
                int i = 0;
                foreach (string arg in args)
                {
                    i++;
                    if (arg == "-timeout")
                    {
                        break;
                    }
                }
                timeout = (int.Parse(args[i])) * 1000;
            }
            #endregion

            #region Display dialog
            TaskDialog dialog = new TaskDialog()
            {
                Icon = dialogIcon,
                Cancelable = hideCancelable,
                InstructionText = instruction,
                Caption = caption,
                Text = text,
            };
            if (button != null)
            {
                dialog.Controls.Add(button);
            }
            dialog.Opened += Dialog_OpenedAsync;
            Task dialogRun = new Task(() =>
            {
                dialog.Show();
            });

            dialogRun.Start();

            if (timeout != -1)
            {
                Console.Write(timeout);
                await Task.Delay(timeout);
                dialog.Close();
            }
            #endregion
            await dialogRun;

            Environment.Exit(0);
        }

        #region Grat out icon when dialog opens
        private static async void Dialog_OpenedAsync(object sender, EventArgs e)
        {
            if (grayCancelable)
            {
                await Task.Delay(100);
                CloseButton.Disable(Process.GetCurrentProcess().ProcessName);
            }
        }
        #endregion

        #region Gray out icon
        internal static class NativeMethods
        {
            public const int SC_CLOSE = 0xF060;
            public const int MF_BYCOMMAND = 0;
            public const int MF_ENABLED = 0;
            public const int MF_GRAYED = 1;

            [DllImport("user32.dll")]
            public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool revert);

            [DllImport("user32.dll")]
            public static extern int EnableMenuItem(IntPtr hMenu, int IDEnableItem, int enable);
        }
        internal static class NativeMethods2
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

            [DllImport("user32.dll")]
            public static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);


            public const uint SC_CLOSE = 0xF060;
            public const uint MF_BYCOMMAND = 0x00000000;
        }
        internal static class CloseButton
        {
            public static void Disable(string processToDisable)
            {
                Process[] process = Process.GetProcessesByName(processToDisable);
                List<IntPtr> handles = new List<IntPtr>();
                List<IntPtr> hMenu = new List<IntPtr>();
                try
                {
                    foreach (Process processes in process)
                    {
                        handles.Add(processes.MainWindowHandle);
                    }
                    foreach (IntPtr handle in handles)
                    {
                        if (handle != null)
                        {
                            hMenu.Add(NativeMethods.GetSystemMenu(handle, false));
                        }
                    }
                }
                catch (Exception)
                {
                }
                foreach (IntPtr hMenues in hMenu)
                {
                    if (hMenues != IntPtr.Zero)
                    {
                        NativeMethods.EnableMenuItem(hMenues, NativeMethods.SC_CLOSE, NativeMethods.MF_GRAYED);
                    }
                }
            }
        }
        #endregion

        #region Command Parser
        private static string ParseCommand(string keyword)
        {
            string localArgs = Environment.CommandLine + "\"";
            string row = "";
            int x = 0;
            foreach (char letter in localArgs.ToCharArray())
            {
                if (letter != ' ')
                {
                    row += letter.ToString();
                    x++;
                }
                else if (row == keyword)
                {
                    string temp = localArgs.Substring(x + 1, localArgs.Length - 2 - x);
                    return temp.Substring(temp.IndexOf("\"") + 1, temp.IndexOf("\"", 1)-1);
                }
                else
                {
                    x++;
                    row = "";
                }
            }
            return null;
        }
        #endregion
    }
}
