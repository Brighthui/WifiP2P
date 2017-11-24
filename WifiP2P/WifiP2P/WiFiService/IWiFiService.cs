using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WifiP2P.WiFiService
{
    public interface IWiFiService
    {
        void DiscoveringPeers();
        TestMessage GetServerReceivedMessage();
        event EventHandler<PhoneDeviceEventArgs> WifiP2pPeersChangedAction;
        event EventHandler<PhoneDeviceEventArgs> WifiP2pStateChangedAction;
        event EventHandler<PhoneDeviceEventArgs> WifiP2pConnectSuccessAction;
        event EventHandler<PhoneDeviceEventArgs> WifiP2pConnectionChangedAction;
        event EventHandler<PhoneDeviceEventArgs> WifiP2pReceiveMessageAction;

        void ConnectDevice(string DeviceName);

        bool SendDeviceMessage(TestMessage message);

    }

    public class PhoneDeviceEventArgs
    {
        public List<string> PhoneDevices { get; set; }
        public bool WiFiEnable { get; set; }
        public bool ConnectSuccess { get; set; }
        public string ConnectDeviceName { get; set; }
        public string ReceivedMessage { get; set; }

        public PhoneDeviceEventArgs()
        {
            PhoneDevices = null;
            WiFiEnable = false;
            ConnectSuccess = false;
            ConnectDeviceName = "";
            ReceivedMessage = "";
        }
    }

    public class TestMessage
    {
        public string ReceivedMessage { get; set; }
        public string ReceivedError { get; set; }
        public string ReceivedFrom { get; set; }

        public string SendMessage { get; set; }
        public string SendError { get; set; }
        public string SendTo { get; set; }

        public bool IsHost { get; set; }

        public TestMessage()
        {
            Clear();
        }

        public void Clear()
        {
            ReceivedMessage = "";
            ReceivedError = "";
            ReceivedFrom = "";

            SendMessage = "";
            SendError = "";
            SendTo = "";
        }
    }

}