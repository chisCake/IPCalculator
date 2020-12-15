using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IPCalculator {
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		#region KeysRegion

		private void TextBox_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key.ToString() == "System")
				return;

			string key = e.Key.ToString().Trim('D');
			var box = sender as TextBox;
			if (box.Text.Length == 2 || !int.TryParse(key, out _) || Keyboard.IsKeyDown(Key.LeftShift))
				e.Handled = true;

			if (box.Name == "NetmaskLength" || box.Name == "HostsAmt")
				return;



			if (box.Text.Length >= 2 || box.Text.Length == 0 && key == "0") {
				if (box.Text.Length == 2 && int.TryParse(key, out _))
					box.Text += key;

				if (box.Text.Length == 0 && key == "0") {
					box.Text += "0";
					e.Handled = true;
				}

				var nextTBox = NextTBox(box);
				if (nextTBox != null) {
					nextTBox.Focus();
					nextTBox.SelectAll();
				}
			}
		}

		private TextBox tTBox;
		private void TextBox_KeyUp(object sender, KeyEventArgs e) {
			string key = e.Key.ToString();
			var box = sender as TextBox;
			if (key == "Back") {
				if (box.Text.Length == 0) {
					if (box == tTBox) {
						var prevTBox = PrevTBox(box);
						if (prevTBox == null)
							return;
						//if (prevTBox.Text.Length != 0)
						//prevTBox.Text = prevTBox.Text.Substring(0, prevTBox.Text.Length - 1);
						//prevTBox.CaretIndex = prevTBox.Text.Length;
						prevTBox.Focus();
						prevTBox.SelectAll();
					}
					else
						tTBox = box;
				}
			}

			if (key == "Space") {
				box.Text = box.Text.Trim();
				var nextTBox = NextTBox(box);
				if (nextTBox != null) {
					nextTBox.Focus();
					nextTBox.SelectAll();
				}
			}

			if ((sender as TextBox).Parent == userNetmask) {
				string netmask = "";
				foreach (TextBox item in userNetmask.Children) {
					netmask += item.Text.Length == 0 ? "0" : item.Text;
					netmask += ".";
				}
				try {
					NetmaskLength.Text = Netmask.GetLength(netmask.Trim('.')).ToString();
				}
				catch {
					NetmaskLength.Text = "Invalid";
				}
			}
		}

		private void NetmaskLength_KeyUp(object sender, KeyEventArgs e) {
			var senderBox = sender as TextBox;
			if (senderBox.Text.Length == 0)
				return;

			if (!int.TryParse(senderBox.Text, out int length) || length < 0 || length > 32)
				return;

			var netmask = new Netmask(length);
			for (int i = 0; i < 4; i++)
				(userNetmask.Children[i] as TextBox).Text = netmask[i].ToString();
		}

		private TextBox NextTBox(TextBox current) {
			var panel = current.Parent as StackPanel;
			int index = panel.Children.IndexOf(current);
			if (index == 3) {
				if (panel == userIP) {
					panel = userNetmask;
					return panel.Children[0] as TextBox;
				}
				else
					return null;
			}
			else
				return panel.Children[index + 1] as TextBox;
		}

		private TextBox PrevTBox(TextBox current) {
			var panel = current.Parent as StackPanel;
			int index = panel.Children.IndexOf(current);
			if (index == 0) {
				if (panel == userNetmask) {
					panel = userIP;
					return panel.Children[3] as TextBox;
				}
				else
					return null;
			}
			else
				return panel.Children[index - 1] as TextBox;
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
			var box = sender as TextBox;
			box.SelectAll();
		}

		private void TextBox_LostFocus(object sender, RoutedEventArgs e) {
			var box = sender as TextBox;
			if (int.TryParse((box.Text), out int res) && res > 255)
				box.Text = "255";
		}

		#endregion

		public void Log(string msg) {
			logText.Content += $"{msg}\n";
			log.ScrollToBottom();
		}


		private void Button_Click(object sender, RoutedEventArgs e) {
			string strIP = "";
			foreach (TextBox box in userIP.Children)
				strIP += box.Text + ".";
			strIP = strIP.Trim('.');

			IPAddress ip;
			Netmask netmask;

			try {
				ip = new IPAddress(strIP);
			}
			catch (InvalidAddressFormatException) {
				Log("Неверно введён IP адрес");
				return;
			}

			try {
				netmask = new Netmask(Convert.ToInt32(NetmaskLength.Text));
			}
			catch (InvalidNetmaskFormatException) {
				Log("Неверно введён адрес подсети");
				return;
			}
			catch(FormatException) {
				Log("Заполните поля IP адреса");
				return;
			}

			Log("");
			Log(ip.ToString());
			Log(netmask.ToString());

			Log($"Network:   {Calculator.GetNetwork(ip, netmask)}/{netmask.Length}");
			Log($"HostBits:  {Calculator.GetHostBits(netmask)}");
			Log($"Broadcast: {Calculator.GetBroadcast(ip, netmask)}");
			if (HostsAmt.Text.Length != 0) {
				Log($"Mask for hosts: " + Calculator.GetNetmask(Convert.ToInt32(HostsAmt.Text)));
			}
		}
	}
}
