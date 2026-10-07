package com.my.helper;
import android.content.Context;
import android.telephony.TelephonyManager;
import com.google.android.gms.ads.identifier.AdvertisingIdClient;
import android.net.ConnectivityManager;
import android.net.Network;
import android.net.NetworkCapabilities;
import android.os.Build;

public class AndroidUtil {

    public static int getSimState(Context context) {

        try {
            TelephonyManager tm = (TelephonyManager) context.getSystemService(Context.TELEPHONY_SERVICE);

            if (tm == null)
                return 0;

            int state = tm.getSimState();

            if (state == TelephonyManager.SIM_STATE_READY) {
                return 1; // 有SIM
            }

            return 2; // 无SIM或不可用

        } catch (Exception e) {
            e.printStackTrace();
        }

        return 0;
    }

    public static String getGAID(Context context) {

        try {

            AdvertisingIdClient.Info adInfo = AdvertisingIdClient.getAdvertisingIdInfo(context);

            if (adInfo != null) {
                return adInfo.getId();
            }

        } catch (Exception e) {
            e.printStackTrace();
        }

        return "";
    }

        /**
     * 检测当前是否开启 VPN
     */
    public static boolean isVpnActive(Context context) {

        if (context == null)
            return false;

        try {

            ConnectivityManager cm =
                    (ConnectivityManager) context.getSystemService(Context.CONNECTIVITY_SERVICE);

            if (cm == null)
                return false;

            // Android 6+
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {

                Network network = cm.getActiveNetwork();

                if (network == null)
                    return false;

                NetworkCapabilities capabilities =
                        cm.getNetworkCapabilities(network);

                if (capabilities == null)
                    return false;

                return capabilities.hasTransport(NetworkCapabilities.TRANSPORT_VPN);
            }

            // Android 5 fallback
            else {

                android.net.NetworkInfo info = cm.getActiveNetworkInfo();

                if (info == null)
                    return false;

                return info.getType() == ConnectivityManager.TYPE_VPN;
            }

        } catch (Exception e) {

            e.printStackTrace();
            return false;
        }
    }
}