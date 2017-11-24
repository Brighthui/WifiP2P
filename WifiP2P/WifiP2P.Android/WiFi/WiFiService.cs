using Android.Content;
using Android.Net.Wifi.P2p;
using System;
using WifiP2P.Droid.WiFi;
using WifiP2P.WiFiService;
using static Android.Net.Wifi.P2p.WifiP2pManager;
using Android.Net.Wifi;
using Java.Net;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Android.OS;
using Java.Lang;

[assembly: Xamarin.Forms.Dependency(typeof(WiFiService))]
namespace WifiP2P.Droid.WiFi
{
    public class WiFiService: IWiFiService
    {
        public static WiFiService wifiService;
        public event EventHandler<PhoneDeviceEventArgs> WifiP2pStateChangedAction;
        public event EventHandler<PhoneDeviceEventArgs> WifiP2pPeersChangedAction;
        public event EventHandler<PhoneDeviceEventArgs> WifiP2pConnectSuccessAction;
        public event EventHandler<PhoneDeviceEventArgs> WifiP2pConnectionChangedAction;
        public event EventHandler<PhoneDeviceEventArgs> WifiP2pReceiveMessageAction;

        public WiFiService()
        {
            wifiService = this;
        }

        public TestMessage GetServerReceivedMessage()
        {
            return WifiManager.DeviceMessage;
        }

        //step 1
        public void DiscoveringPeers()
        {
            WifiManager.manager?.DiscoverPeers(WifiManager.channel, new MyActionListner(() => { }));
        }

        public void RefreshPeersChanged(List<string> devices)
        {
            PhoneDeviceEventArgs args = new PhoneDeviceEventArgs();
            args.PhoneDevices = devices;
            WifiP2pPeersChangedAction(this, args);
        }

        public void RefreshStateChanged(bool bEnable)
        {
            PhoneDeviceEventArgs args = new PhoneDeviceEventArgs();
            args.WiFiEnable = bEnable;
            WifiP2pStateChangedAction(this, args);
        }

        //step2
        public void ConnectDevice(string DeviceName)
        {
            foreach (var p in WifiManager.PhoneDevices)
            {
                if (p.DeviceName != DeviceName)
                    continue;

                WifiP2pConfig config = new WifiP2pConfig();
                config.DeviceAddress = p.DeviceAddress;
                config.Wps.Setup = WpsInfo.Pbc;
                config.GroupOwnerIntent = 0;

                WifiManager.manager.Connect(WifiManager.channel, config, new MyActionListner(ConnectSuccess));
            }

        }

        public void RefreshConnectSuccessChanged(bool bEnable)
        {
            PhoneDeviceEventArgs args = new PhoneDeviceEventArgs();
            args.ConnectSuccess = bEnable;
            WifiP2pConnectSuccessAction(this, args);
        }

        public void RefreshConnectionChanged(string ConnectDeviceName)
        {
            PhoneDeviceEventArgs args = new PhoneDeviceEventArgs();
            args.ConnectDeviceName = ConnectDeviceName;
            WifiP2pConnectionChangedAction(this, args);
        }

        static void ConnectSuccess()
        {
            WiFiService.wifiService.RefreshConnectSuccessChanged(true);
        }

        public void RefreshReceivedMessage(string ReceivedMessage)
        {
            PhoneDeviceEventArgs args = new PhoneDeviceEventArgs();
            args.ReceivedMessage = ReceivedMessage;
            WifiP2pReceiveMessageAction(this, args);
        }

        //Step3
        public bool SendDeviceMessage(TestMessage message)
        {
            string address = "";

            if (message.IsHost)
            {
                if (string.IsNullOrEmpty(message.SendTo))
                {
                    WiFiService.wifiService.RefreshReceivedMessage("Can't send msg to client, no client address.");
                    return false;
                }
                address = message.SendTo;
            }
            else // i am client
            {
                if (WifiManager.HostInfo == null)
                {
                    WiFiService.wifiService.RefreshReceivedMessage("can't send msg to host, host not connected.");
                    return false;
                }
                address = WifiManager.HostInfo.GroupOwnerAddress.ToString().Replace(@"/", "");
            }

            Task.Run(() =>
                {
                    var host = address; // WifiManager.GroupOwnerInfo.GroupOwnerAddress;
                    var port = 8888;
                    var socket = new Socket();

                    try
                    {
                        socket.Bind(null);
                        socket.Connect(new InetSocketAddress(host, port), 5000);

                        var stream = socket.OutputStream;
                        var inputStream = GenerateStreamFromString(message.SendMessage);
                        CopyStream(inputStream, stream);
                    }
                    catch (Java.Lang.Exception e)
                    {
                        WiFiService.wifiService.RefreshReceivedMessage(e.Message);
                    }
                    finally
                    {
                        if (socket != null)
                        {
                            if (socket.IsConnected)
                            {
                                try
                                {
                                    socket.Close();
                                }
                                catch (IOException e)
                                {
                                    WiFiService.wifiService.RefreshReceivedMessage(e.Message);
                                }
                            }
                        }
                    }
                }
            );


            return true;
        }


        public bool CopyStream(Stream inputStream, Stream outputStream)
        {
            var buf = new byte[1024];
            try
            {
                int n;
                while ((n = inputStream.Read(buf, 0, buf.Length)) != 0)
                    outputStream.Write(buf, 0, n);
                outputStream.Close();
                inputStream.Close();
            }
            catch (Java.Lang.Exception e)
            {
                return false;
            }
            return true;
        }

        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public Java.Lang.Runnable ClientThread = new Java.Lang.Runnable(async ()=>
        {
            DatagramSocket socket = null;
            InetAddress host = WifiManager.HostInfo.GroupOwnerAddress;
            int port = 8888;

            byte[] sendData;
            byte[] receiveData = new byte[1024];


            while (true)
            {
                sendData = WiFiService.wifiService.String2Bytes(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                try
                {
                    if (socket == null)
                    {
                        socket = new DatagramSocket(port);
                    }
                }
                catch (Java.Lang.Exception)
                {

                    throw;
                }


                //Client Send
                try
                {
                    DatagramPacket packetSend = new DatagramPacket(sendData, sendData.Length, host, port);
                    socket.Send(packetSend);
                }
                catch (Java.Lang.Exception)
                {

                    throw;
                }

                await Task.Delay(5000);

                //Client Receive
                try
                {
                    DatagramPacket packetReceive = new DatagramPacket(receiveData, receiveData.Length);
                    socket.Receive(packetReceive);
                }
                catch (Java.Lang.Exception)
                {

                    throw;
                }
            }

        });

        public Java.Lang.Runnable ServerThread = new Java.Lang.Runnable(() =>
        {
            DatagramSocket socket = null;
            InetAddress client = null;
            int port = 8888;

            byte[] sendData = new byte[1024];
            byte[] receiveData = new byte[1024];


            while (true)
            {
                try
                {
                    if (socket == null)
                    {
                        socket = new DatagramSocket(port);
                    }
                }
                catch (Java.Lang.Exception)
                {

                    throw;
                }

                DatagramPacket receivePacket = new DatagramPacket(receiveData, receiveData.Length);


                //Server Receive, have to receive first to know clinet address
                try
                {
                    socket.Receive(receivePacket);
                    receiveData = receivePacket.GetData();

                    WiFiService.wifiService.RefreshReceivedMessage(WiFiService.wifiService.Bytes2String(receiveData));
                    if (client == null)
                        client = receivePacket.Address;
                    
                }
                catch (Java.Lang.Exception)
                {

                    throw;
                }

                //Server Send
                try
                {
                    if (client != null)
                    {
                        DatagramPacket packetSend = new DatagramPacket(sendData, 0, sendData.Length, client, port);
                        socket.Send(packetSend);
                    }
                }
                catch (Java.Lang.Exception)
                {

                    throw;
                }



            }

        });

        public byte[] String2Bytes(string input)
        {
            return Encoding.ASCII.GetBytes(input);
        }

        public string Bytes2String(byte[] input)
        {
            return Encoding.Default.GetString(input);
        }


    }

    public class WifiManager
    {
        public static WifiP2pManager manager { get; set; }
        public static Channel channel { get; set; }
        public static BroadcastReceiver receiver { get; set; }

        public static List<WifiP2pDevice> PhoneDevices = new List<WifiP2pDevice>();

        public static WifiP2pInfo HostInfo { get; set; }

        public static TestMessage DeviceMessage = new TestMessage();

    }

    public class MyActionListner : Java.Lang.Object, WifiP2pManager.IActionListener
    {
        //private readonly Context _context;
        //private readonly string _failure;
        private readonly Action _action;

        public MyActionListner(Action onSuccessAction)
        {
            //_context = context;
            //_failure = failure;
            _action = onSuccessAction;
        }

        public void OnFailure(WifiP2pFailureReason reason)
        {
            //Toast.MakeText(_context, _failure + " Failed : " + reason,
            //                ToastLength.Short).Show();
        }

        public void OnSuccess()
        {
            //Toast.MakeText(_context, _failure + "Discovery Initiated",
            //                ToastLength.Short).Show();
            _action.Invoke();
        }
    }

    public class PeersListener : Java.Lang.Object, IPeerListListener
    {
        public void OnPeersAvailable(WifiP2pDeviceList peers)
        {
            WifiManager.PhoneDevices.Clear();

            foreach (var peer in peers.DeviceList)
            {
                foreach (var p in WifiManager.PhoneDevices)
                {
                    if (p.DeviceName == peer.DeviceName)
                        continue;
                }

                WifiManager.PhoneDevices.Add(peer);
            }

            List<string> lstDevice = new List<string>();
            foreach (var p in WifiManager.PhoneDevices)
                lstDevice.Add(p.DeviceName);

            WiFiService.wifiService.RefreshPeersChanged(lstDevice);
        }
    }


    //below class not use because AsyncTask need to return result, but receive has to keep alive, no return.
    public class ServerReceive : AsyncTask
    {
        protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
        {
            ServerSocket serverSocket = null;
            Socket client = null;
            string Received = "";

            try
            {
                if (serverSocket == null)
                {
                    serverSocket = new ServerSocket(8888);
                    client = serverSocket.Accept();
                }

                var inputstream = client.InputStream;
                StreamReader reader = new StreamReader(inputstream);
                string text = reader.ReadToEnd();
                serverSocket.Close();
                serverSocket = null;
                client = null;
            }
            catch (Java.Lang.Exception ex)
            {
                return ex.Message;
            }

            return Received;
        }

        protected void OnPostExecute(string received)
        {
            WiFiService.wifiService.RefreshReceivedMessage(received);
        }
    }


}