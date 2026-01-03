using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;

namespace Dns
{
    public partial class MainWindow : Window
    {
        private bool isConnected = false;
        private string activeAdapter = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void dnsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = dnsComboBox.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                string[] dnsServers = selectedItem.Tag.ToString().Split(';');
                selectedDnsText.Text = $"انتخاب شده:\nDNS1: {dnsServers[0]}\nDNS2: {dnsServers[1]}";
            }
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem selectedItem = dnsComboBox.SelectedItem as ComboBoxItem;

            if (selectedItem == null)
            {
                MessageBox.Show("لطفاً یک DNS انتخاب کنید.", "DNS Changer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string[] dnsServers = selectedItem.Tag.ToString().Split(';');

            if (!isConnected)
            {
                activeAdapter = GetActiveAdapter();

                if (activeAdapter == null)
                {
                    MessageBox.Show("کارت شبکه فعال یافت نشد.", "DNS Changer", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SetDns(activeAdapter, dnsServers[0], dnsServers[1]);

                isConnected = true;
                connectButton.Content = "قطع اتصال";

                MessageBox.Show("DNS فعال شد.", "DNS Changer", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (activeAdapter != null)
                {
                    ResetDns(activeAdapter);
                }

                isConnected = false;
                connectButton.Content = "اتصال";

                MessageBox.Show("DNS حذف شد و به حالت خودکار برگشت.", "DNS Changer", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string GetActiveAdapter()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    ni.GetIPProperties().GatewayAddresses.Count > 0)
                {
                    return ni.Name;
                }
            }
            return null;
        }

        private void SetDns(string adapterName, string dns1, string dns2)
        {
            RunNetsh($"interface ip set dns name=\"{adapterName}\" static {dns1}");
            RunNetsh($"interface ip add dns name=\"{adapterName}\" {dns2} index=2");
        }

        private void ResetDns(string adapterName)
        {
            RunNetsh($"interface ip set dns name=\"{adapterName}\" dhcp");
        }

        private void RunNetsh(string arguments)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = arguments,
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true
            });
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

    }
}
