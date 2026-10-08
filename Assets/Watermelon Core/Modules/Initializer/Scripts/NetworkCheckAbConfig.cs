using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Networking;
using FirebaseRemote;

namespace Watermelon
{

    /// <summary>
    /// 网络请求改成请求remoteconfig，但是又没有正常的请求，所以先只请求一些云函数拿到返回就行
    /// 不用设置remoteconfig
    /// </summary>
    public class NetworkCheckAbConfig
    {
        public IEnumerator CheckConnection(Action<bool> onConnectionChecked)
        {
            // if (Application.internetReachability == NetworkReachability.NotReachable)
            // {
            //     yield return new WaitForSecondsRealtime(0.5f);
            //     onConnectionChecked?.Invoke(false);
            //     yield break;
            // }
            // string SERVER_URL = "https://t38d27g0t2.execute-api.ap-southeast-2.amazonaws.com/block1";
            string SERVER_URL = "https://kq7220kv96.execute-api.ap-southeast-2.amazonaws.com/default/gameblockjam3";
            using (UnityWebRequest request = UnityWebRequest.Get(SERVER_URL))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onConnectionChecked?.Invoke(false);
                    yield break;
                }

                string text = request.downloadHandler.text.Trim();

                //1.0.4|1.0.3|1.0.2
                string[] splitVersions = text.Split("|");
                bool isVsersionFind = false;
                foreach (var it in splitVersions)
                {
                    if (string.Equals(Application.version, it, StringComparison.OrdinalIgnoreCase))
                    {
                        isVsersionFind = true;
                        break;
                    }
                }

                Debug.Log("NetworkCheckAbConfig 服务器返回: " + text);

                ServerRemoteMgr.Instance.SetForceADataByWeb(isVsersionFind);
                Debug.Log("NetworkCheckAbConfig 解析后的 isVsersionFind: " + isVsersionFind);
            }
            onConnectionChecked?.Invoke(true);
        }
    }
}