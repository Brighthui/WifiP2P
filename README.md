This WiFiP2P demonstrates the usage of Android WiFi Direct in Xamarin Forms framework.

How it is working?

1. Publish project into your android devices, you need at least two devices to see result
2. From one of android device, Device A, click 'Find TEAMLEAD', it will list other android devices including your other device, Device B, if B does not show up in A, go to your B setting->Find WiFi direct, then B will show up in A.
3. Click device name from A (we choose B name), then B will pop up confirm connection which is done by android OS, you click 'OK'
4. From A, you will see 'I am Client', and B will show 'I am Host'
5. From A, Click 'Send Message', which send timestamp to B
6. From B, Click 'Send Message', which send timestamp to A
7. The first time send message, only from client to host, because host doesn't know client IP address unless host received client message.

For simple process, i use Task.Delay(1000) (every one second) to fetch communication result from Andriod to PCL.

Below are links i reference most.

https://developer.android.com/guide/topics/connectivity/wifip2p.html

https://github.com/Cheesebaron/WiFiDirectSample

https://developer.android.com/training/connect-devices-wirelessly/wifi-direct.html

https://www.youtube.com/watch?v=st-SQEQJ0dM