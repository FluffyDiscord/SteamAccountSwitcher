﻿#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using SteamAccountSwitcher.Properties;

#endregion

namespace SteamAccountSwitcher
{
    internal class SteamClient
    {
        public static void Launch()
        {
            Process.Start(Settings.Default.SteamPath);
        }

        public static void BigPicture()
        {
            Process.Start(Settings.Default.SteamPath, Resources.SteamBigPictureArg);
        }

        public static void LogIn(Account account)
        {
            var args = new List<string>();

            args.Add($"{Resources.SteamLoginArgument} \"{account.Username}\" \"{account.Password}\"");
            if (Settings.Default.BigPictureMode)
                args.Add(Resources.SteamBigPictureArg);
            if (Settings.Default.StartSteamMinimized)
                args.Add(Resources.SteamSilentArg);
            args.Add(Settings.Default.SteamLaunchArgs);

            Process.Start(Settings.Default.SteamPath, string.Join(" ", args));
        }

        public static void LogOut()
        {
            Process.Start(Settings.Default.SteamPath, Resources.SteamShutdownArgument);
        }

        public static void ForceClose()
        {
            using (var proc = Process.GetProcessesByName(Resources.Steam)[0])
                proc.CloseMainWindow();
        }

        public static string GetSteamTitle()
        {
            using (var proc = Process.GetProcessesByName(Resources.Steam)[0])
                return proc.MainWindowTitle;
        }

        public static void LogOutAuto()
        {
            if (GetSteamTitle() == Resources.SteamNotLoggedInTitle)
                ForceClose();
            else
                LogOut();
        }

        public static bool LogOutTimeout()
        {
            var timeout = 0;
            const int maxtimeout = 10000;
            const int waitstep = 100;
            if (IsSteamOpen())
            {
                LogOutAuto();
                while (IsSteamOpen())
                {
                    if (timeout >= maxtimeout)
                    {
                        Popup.Show("Logout operation has timed out. Please force close steam and try again.");
                        return false;
                    }
                    Thread.Sleep(waitstep);
                    timeout += waitstep;
                }
            }
            return true;
        }

        public static string GetPath()
        {
            string path;
            try
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                    path = (string)registryKey.GetValue("SteamExe");
            }
            catch
            {
                path = "";
            }

            if (!string.IsNullOrWhiteSpace(path))
                return path;

            Popup.Show("Default Steam path could not be located.\r\n\r\nPlease enter Steam executable location.");
            var dia = new SteamPath();
            dia.ShowDialog();
            return dia.Path;
        }

        public static bool ResolvePath()
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.SteamPath) || !File.Exists(Settings.Default.SteamPath))
                Settings.Default.SteamPath = GetPath();
            return !string.IsNullOrWhiteSpace(Settings.Default.SteamPath);
        }

        public static bool IsSteamOpen()
        {
            return (Process.GetProcessesByName(Resources.Steam).Length > 0);
        }
    }
}