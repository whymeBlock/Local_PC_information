using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace hostname_info
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int destIp, int srcIP, byte[] macAddr, ref uint physicalAddrLen);
        public static MainWindow? Window;

        public MainWindow()
        {
            InitializeComponent();
            Window = this;
        }

        private void Drag(object sender, RoutedEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                MainWindow.Window.DragMove();
            }
        }

        private void label_Copy_Click(object sender, EventArgs e)
        {
            Label? label = sender as Label;

            if (label != null)
            {
                Clipboard.SetText((string)label.Content, TextDataFormat.UnicodeText);
            }
        }

        private void InitializeInfo()
        {
            // === Получение имени ПК ===
            string host_n = Dns.GetHostName();
            hostname.Content = host_n;


            // === Получение IP-адреса ===
            // формируем массив с интерфейсами
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            // Создаем список с нашими Ip адресами
            List<string> list = new List<string>();


            // Перебираем сетевые интерфейсы
            foreach (NetworkInterface nic in nics)
            {
                // фильтруем сетевые интерфейсы по префиксу DHCP который нам и нужен
                if (nic.GetIPProperties().UnicastAddresses.LastOrDefault()?.PrefixOrigin == System.Net.NetworkInformation.PrefixOrigin.Dhcp)
                {
                    // Узнаем IP у нужного интерфейса и записываем в List.
                    IPAddress? res = nic.GetIPProperties().UnicastAddresses.LastOrDefault()?.Address;
                    list.Add(res.ToString());
                    ip_adr.Content = ip_adr.Content + res.ToString() + "\n";

                    // === Получение MAC-адреса ===
                    // Преобразуем IP адрес в тип IPAddress
                    IPAddress dst = IPAddress.Parse(res.ToString());

                    byte[] macAddr = new byte[6];
                    uint macAddrLen = (uint)macAddr.Length;

                    if (SendARP(BitConverter.ToInt32(dst.GetAddressBytes(), 0), 0, macAddr, ref macAddrLen) != 0)
                        throw new InvalidOperationException("SendARP failed.");

                    string[] str = new string[(int)macAddrLen];
                    for (int i = 0; i < macAddrLen; i++)
                        str[i] = macAddr[i].ToString("x2");

                    // Выводим MAC-адрес основного сетевого интерфейса
                    mac_adr.Content = mac_adr.Content + string.Join(":", str) + "\n";
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeInfo();
        }

        private void Button_Update(object sender, RoutedEventArgs e)
        {
            ip_adr.Content = "";
            mac_adr.Content = "";
            InitializeInfo();
        }

        private void Button_Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Image_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
