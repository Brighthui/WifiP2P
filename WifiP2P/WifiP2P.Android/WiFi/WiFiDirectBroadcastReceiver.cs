using Android.Content;
using Android.Net;
using Android.Net.Wifi.P2p;
using Java.Net;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WifiP2P.Droid.WiFi
{
    public class WiFiDirectBroadcastReceiver : BroadcastReceiver
    {
        private readonly WifiP2pManager _manager;
        private readonly WifiP2pManager.Channel _channel;
        private readonly MainActivity _activity;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="manager">WifiP2pManager system service</param>
    /// <param name="channel">Wifi p2p channel</param>
    /// <param name="activity">activity associated with the receiver</param>
    public WiFiDirectBroadcastReceiver(WifiP2pManager manager, WifiP2pManager.Channel channel,
                                           MainActivity activity)
        {
            _manager = manager;
            _channel = channel;
            _activity = activity;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;

            if (WifiP2pManager.WifiP2pStateChangedAction.Equals(action))
            {
                // UI update to indicate wifi p2p status.
                var state = intent.GetIntExtra(WifiP2pManager.ExtraWifiState, -1);
                if (state == (int)WifiP2pState.Enabled)
                {
                    WiFiService.wifiService.RefreshStateChanged(true);
                }
                    // Wifi Direct mode is enabled
                    //_activity.IsWifiP2PEnabled = true;
                else
                {
                    WiFiService.wifiService.RefreshStateChanged(false);
                    //_activity.IsWifiP2PEnabled = false;
                    //_activity.ResetData();
                }
                //Log.Debug(WiFiDirectActivity.Tag, "P2P state changed - " + state);
            }
            else if (WifiP2pManager.WifiP2pPeersChangedAction.Equals(action))
            {
                // request available peers from the wifi p2p manager. This is an
                // asynchronous call and the calling activity is notified with a
                // callback on PeerListListener.onPeersAvailable()
                if (_manager != null)
                {
                    try
                    {
                        _manager.RequestPeers(_channel, new PeersListener());
                    }
                    catch (Exception ex)
                    {

                        string ss = ex.Message;
                    }
                }
                //Log.Debug(WiFiDirectActivity.Tag, "P2P peers changed");
            }
            else if (WifiP2pManager.WifiP2pConnectionChangedAction.Equals(action))
            {
                if (_manager == null)
                    return;

                var networkInfo = (NetworkInfo)intent.GetParcelableExtra(WifiP2pManager.ExtraNetworkInfo);

                if (networkInfo.IsConnected)
                {
                    // we are connected with the other device, request connection
                    // info to find group owner IP
                    _manager.RequestConnectionInfo(_channel, new ServerReceiveMessage());
                }
                
            }
            else if (WifiP2pManager.WifiP2pThisDeviceChangedAction.Equals(action))
            {
                int m = 0;
                //var fragment =
                //        _activity.FragmentManager.FindFragmentById<DeviceListFragment>(Resource.Id.frag_list);
                //fragment.UpdateThisDevice((WifiP2pDevice)intent.GetParcelableExtra(WifiP2pManager.ExtraWifiP2pDevice));
            }
        }
    }

    public class ServerReceiveMessage : Java.Lang.Object, WifiP2pManager.IConnectionInfoListener
    {
        public void OnConnectionInfoAvailable(WifiP2pInfo info)
        {
            if (info.GroupFormed)
            {
                WifiManager.HostInfo = info;

                if (info.IsGroupOwner)
                {
                    WiFiService.wifiService.RefreshConnectionChanged("I am Host");
                    Task.Run(() => ReceiveMessageAsync(true));
                }
                else
                {
                    WiFiService.wifiService.RefreshConnectionChanged("I am Client");
                    Task.Run(() => ReceiveMessageAsync(false));
                }

            }
        }

        //Task.Run(() => ReceiveMessageAsync());
        public async Task<bool> ReceiveMessageAsync(bool IsHost)
        {
            ServerSocket serverSocket = null;
            Socket client = null;

            while (true)
            {
                try
                {
                    if (serverSocket == null)
                    {
                        serverSocket = new ServerSocket(8888);
                        client = await serverSocket.AcceptAsync();
                    }
                    WifiManager.DeviceMessage.ReceivedError = "";
                    WifiManager.DeviceMessage.IsHost = IsHost;

                    var inputstream = client.InputStream;
                    StreamReader reader = new StreamReader(inputstream);
                    WifiManager.DeviceMessage.ReceivedMessage = reader.ReadToEnd();
                    WifiManager.DeviceMessage.ReceivedFrom = client.InetAddress.ToString().Replace(@"/", ""); 
                    serverSocket.Close();
                    serverSocket = null;
                    client = null;
                }
                catch (Exception ex)
                {
                    WifiManager.DeviceMessage.ReceivedError = ex.Message;
                }
            }
        }

    }

}