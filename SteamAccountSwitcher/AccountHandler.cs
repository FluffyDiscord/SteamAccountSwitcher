﻿#region

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using SteamAccountSwitcher.Properties;

#endregion

namespace SteamAccountSwitcher
{
	internal class AccountHandler
	{
		private readonly List<Account> _accounts;
		private readonly StackPanel _stackPanel;
		private int SelectedIndex = -1;

		public AccountHandler(StackPanel stackPanel)
		{
			_stackPanel = stackPanel;
			_accounts = Deserialize() ?? new List<Account>();
			Refresh();
		}

		public string Serialize()
		{
			return new Encryption().Encrypt(JsonConvert.SerializeObject(_accounts));
		}

		public List<Account> Deserialize()
		{
			return string.IsNullOrWhiteSpace(Settings.Default.Accounts)
				? new List<Account>()
				: JsonConvert.DeserializeObject<List<Account>>(new Encryption().Decrypt(Settings.Default.Accounts));
		}

		public void LogOut()
		{
			Scripts.Run(Scripts.Close, new Account());
		}

		public void LogIn(Account account)
		{
			Scripts.Run(Scripts.Open, account);
		}

		public void Add(Account account)
		{
			_accounts.Add(account);
			Refresh();
		}

		public void Refresh()
		{
			// Remove all buttons.
			_stackPanel.Children.Clear();

			// Add new buttons with saved shortcut data
			foreach (var account in _accounts)
			{
				var btn = new Button
				{
					Content = new TextBlock {Text = account.Username, TextWrapping = TextWrapping.Wrap},
					Height = 32,
					HorizontalContentAlignment = HorizontalAlignment.Left,
					Margin = new Thickness(0, 0, 0, 8),
					Padding = new Thickness(4,0,0,0)
				};
				btn.Click += Button_Click;
				btn.MouseEnter += delegate{SelectedIndex = _stackPanel.Children.IndexOf(btn);};

				_stackPanel.Children.Add(btn);
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			LogIn(_accounts[SelectedIndex]);
		}
	}
}