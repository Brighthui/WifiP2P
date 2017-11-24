
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net.Wifi.P2p;
using Android.OS;
using WifiP2P.Droid.WiFi;

namespace WifiP2P.Droid
{
    [Activity(Label = "WifiP2P", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        
        private readonly IntentFilter _intentFilter = new IntentFilter();

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            WifiManager.manager = (WifiP2pManager)base.GetSystemService(WifiP2pService);
            WifiManager.channel = WifiManager.manager.Initialize(this, base.MainLooper, null);

            _intentFilter.AddAction(WifiP2pManager.WifiP2pStateChangedAction);
            _intentFilter.AddAction(WifiP2pManager.WifiP2pPeersChangedAction);
            _intentFilter.AddAction(WifiP2pManager.WifiP2pConnectionChangedAction);
            _intentFilter.AddAction(WifiP2pManager.WifiP2pThisDeviceChangedAction);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        protected override void OnResume()
        {
            base.OnResume();
            WifiManager.receiver = new WiFiDirectBroadcastReceiver(WifiManager.manager, WifiManager.channel, this);
            RegisterReceiver(WifiManager.receiver, _intentFilter);
        }

        protected override void OnPause()
        {
            base.OnPause();
            UnregisterReceiver(WifiManager.receiver);
        }
    }
}

