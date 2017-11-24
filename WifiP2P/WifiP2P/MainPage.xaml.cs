using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WifiP2P.WiFiService;
using Xamarin.Forms;

namespace WifiP2P
{
    public partial class MainPage : ContentPage
    {
        IWiFiService wifi;
        TestMessage testMessage;

        public MainPage()
        {
            InitializeComponent();

            wifi = DependencyService.Get<IWiFiService>();
            wifi.WifiP2pPeersChangedAction += OnPeersChanged;
            wifi.WifiP2pStateChangedAction += OnStateChanged;
            wifi.WifiP2pConnectSuccessAction += OnConnectSuccessChanged;
            wifi.WifiP2pConnectionChangedAction += OnConnectionChanged;
            wifi.WifiP2pReceiveMessageAction += OnReceivedMessageChanged;

            LoopForServerReceivedMessage();
        }

        private void Find_Clicked(object sender, EventArgs e)
        {
            lstDevice.ItemsSource = null;
            wifi.DiscoveringPeers();

        }

        private void OnPeersChanged(object sender, PhoneDeviceEventArgs e)
        {
            lstDevice.ItemsSource = e.PhoneDevices;
        }

        private void OnStateChanged(object sender, PhoneDeviceEventArgs e)
        {
            if (e.WiFiEnable)
                lblWiFi.Text = "WiFi Enabled";
            else
                lblWiFi.Text = "WiFi Disabled";
        }

        private void OnConnectSuccessChanged(object sender, PhoneDeviceEventArgs e)
        {
            if (e.ConnectSuccess)
                lblConnectSuccess.Text = "Connect Success";
            else
                lblConnectSuccess.Text = "No Connection";
        }

        private void OnConnectionChanged(object sender, PhoneDeviceEventArgs e)
        {
            lblConnectDevice.Text = e.ConnectDeviceName;
        }

        private void OnReceivedMessageChanged(object sender, PhoneDeviceEventArgs e)
        {
            lblReceivedMessage.Text = e.ReceivedMessage;
        }

        private void lstDevice_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            wifi.ConnectDevice(e.SelectedItem as string);
        }

        private void Send_Clicked(object sender, EventArgs e)
        {
            TestMessage sendMessage = new TestMessage();

            if (testMessage != null)
            {
                sendMessage.SendTo = testMessage.ReceivedFrom;
                sendMessage.IsHost = testMessage.IsHost;
            }

            sendMessage.SendMessage = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

            wifi.SendDeviceMessage(sendMessage);
        }

        private async void LoopForServerReceivedMessage()
        {
            while (true)
            {
                testMessage = wifi.GetServerReceivedMessage();
                lblServerReceived.Text = testMessage.ReceivedMessage;
                await Task.Delay(1000);
            }
        }

    }
}
