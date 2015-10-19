﻿#region

using System.ComponentModel;
using System.Windows;
using SteamAccountSwitcher.Properties;

#endregion

namespace SteamAccountSwitcher
{
    public static class AccountHelper
    {
        public static void Add(Account account)
        {
            App.Accounts.Add(account);
        }

        public static void SwitchTo(this Account account, bool onStart = false)
        {
            App.SwitchWindow.HideWindow();
            var worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                if (SteamClient.LogOutTimeout())
                    SteamClient.LogIn(account, onStart);
            };
            worker.RunWorkerCompleted += delegate
            {
                if (!Settings.Default.AlwaysOn)
                    ClickOnceHelper.ShutdownApplication();
            };
            worker.RunWorkerAsync();
        }

        public static void Edit(this Account account)
        {
            var dialog = new AccountProperties(account);
            dialog.ShowDialog();
            if (dialog.NewAccount == null)
                return;
            App.Accounts[App.Accounts.IndexOf(account)] = dialog.NewAccount;
        }

        public static void New()
        {
            var dialog = new AccountProperties();
            dialog.ShowDialog();
            if (dialog.NewAccount == null)
                return;
            if (string.IsNullOrWhiteSpace(dialog.NewAccount.Username) &&
                string.IsNullOrWhiteSpace(dialog.NewAccount.Password) &&
                string.IsNullOrWhiteSpace(dialog.NewAccount.DisplayName))
                return;
            Add(dialog.NewAccount);
        }

        public static void Remove(this Account account, bool msg = false)
        {
            if (msg &&
                Popup.Show(
                    $"Are you sure you want to delete this account?\r\n\r\n\"{account.GetDisplayName()}\"",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.No)
                return;
            App.Accounts.Remove(account);
        }

        public static void MoveUp(this Account account, bool toEnd = false)
        {
            var index = App.Accounts.IndexOf(account);
            if (toEnd)
            {
                App.Accounts.Swap(index, 0);
            }
            else
            {
                if (index == 0)
                    MoveDown(account, true);
                else
                    App.Accounts.Swap(index, index - 1);
            }
        }

        public static void MoveDown(this Account account, bool toEnd = false)
        {
            var index = App.Accounts.IndexOf(account);
            if (toEnd)
            {
                App.Accounts.Swap(index, App.Accounts.Count - 1);
            }
            else
            {
                if (App.Accounts.Count < index)
                    MoveUp(account, true);
                else
                    App.Accounts.Swap(index, index + 1);
            }
        }
    }
}