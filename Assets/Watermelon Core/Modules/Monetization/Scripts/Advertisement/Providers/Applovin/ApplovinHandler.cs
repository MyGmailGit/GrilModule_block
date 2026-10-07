using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace Watermelon
{
#if MODULE_APPLOVIN
    public class ApplovinHandler : AdProviderHandler
    {
        private bool isBannerLoaded = false;

        public ApplovinHandler(AdProvider moduleType) : base(moduleType) { }

        protected override async Task<bool> InitProviderAsync()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if (adsSettings.RewardedVideoType == AdProvider.Applovin)
            {
                //Add AdInfo Rewarded Video Events
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += RewardedVideoOnAdOpenedEvent;

                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += RewardedVideoOnAdClosedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += RewardedVideoOnAdShowFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += RewardedVideoOnAdRewardedEvent;

                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnMaxRewardedAdLoadedEvent;
                // MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnMaxRewardedAdLoadFailedEvent;
                // MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnMaxRewardedVideoAdClickedEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnMaxRewardedVideoAdRevenuePaidEvent;
            }

            if (adsSettings.InterstitialType == AdProvider.Applovin)
            {
                //Add AdInfo Interstitial Events
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += InterstitialOnAdReadyEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += InterstitialOnAdLoadFailed;
                MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += InterstitialOnAdOpenedEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += InterstitialOnAdClosedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialOnAdShowFailedEvent;

                // MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaidEvent;
            }

            if (adsSettings.BannerType == AdProvider.Applovin)
            {
                //Add AdInfo Banner Events
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += BannerOnAdLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            }

            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                // Mark initialization as successful
                tcs.SetResult(true);

                AnalyticsController.OnAdMaxTrack(AdStepType.ad_start.ToString(), null, null, null, null);
            };

            MaxSdk.InitializeSdk();

            Debug.Log($"[AdsManager]: Applovin Handler InitializeSdk");

            return await tcs.Task;
        }

        #region RewardedAd callback handlers
        private void RewardedVideoOnAdOpenedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdOpenedEvent event received");

                AdsManager.OnProviderAdDisplayed(AdProvider.Applovin, AdType.RewardedVideo);
            });

            AnalyticsController.OnAdMaxTrack(AdStepType.ad_show.ToString(), AnalyticsStr.adTypeRv, adInfo?.Placement, null, adInfo?.NetworkName);
            AnalyticsController.OnRVDisplayed();
        }

        private void RewardedVideoOnAdClosedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(false);

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdClosedEvent event received");

                AdsManager.OnProviderAdClosed(AdProvider.Applovin, AdType.RewardedVideo);

                AdsManager.RequestRewardBasedVideo();
            });

            AnalyticsController.OnAdMaxTrack(AdStepType.ad_closed.ToString(), AnalyticsStr.adTypeRv, adInfo?.Placement, null, adInfo?.NetworkName);
        }

        private void RewardedVideoOnAdRewardedEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(true);

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdRewardedEvent event received");

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestRewardBasedVideo();
            });

            AnalyticsController.OnAdMaxTrack(AdStepType.ad_complete.ToString(), AnalyticsStr.adTypeRv, adInfo?.Placement, null, adInfo?.NetworkName);
        }

        private void RewardedVideoOnAdShowFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(false);

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdShowFailedEvent event received with message: " + errorInfo);

                rewardedRetryAttempt++;
                float retryDelay = Mathf.Pow(2, rewardedRetryAttempt);

                DOVirtual.DelayedCall(rewardedRetryAttempt, () => AdsManager.RequestRewardBasedVideo(), true);//, UpdateMethod.Update);
            });

            AnalyticsController.OnAdMaxTrack(AdStepType.ad_fail.ToString(), AnalyticsStr.adTypeRv, adInfo?.Placement, errorInfo.Code.ToString(), adInfo?.NetworkName);
        }
        private void OnMaxRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AnalyticsController.OnAdMaxTrack(AdStepType.ad_ready.ToString(), AnalyticsStr.adTypeRv, adInfo?.Placement, null, adInfo?.NetworkName);
        }

        /// <summary>
        /// 广告产生收入时
        /// </summary>
        private void OnMaxRewardedVideoAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //adjust 上报广告收入
#if TEST_MODE
            Debug.Log($"[AdsManager]: Rewarded Video Ad Revenue Paid{adInfo.AdUnitIdentifier}");
#endif

            AnalyticsController.OnAdRevenue(
                MaxSdk.GetSdkConfiguration().CountryCode,
                adInfo.Revenue,
                adInfo.NetworkName,
                adInfo.AdUnitIdentifier,
                adInfo.AdFormat,
                "MAX",
                0,
                adInfo.Placement,
                adInfo.Placement);
        }
        #endregion

        #region Interstitial callback handlers
        private void InterstitialOnAdReadyEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Interstitial ad loaded");

                interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                AdsManager.OnProviderAdLoaded(AdProvider.Applovin, AdType.Interstitial);
            });

            AnalyticsController.OnAdMaxTrack(AdStepType.ad_ready.ToString(), AnalyticsStr.adTypeIv, adInfo?.Placement, null, adInfo?.NetworkName);
        }

        private void InterstitialOnAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Interstitial ad failed to load an ad with error: " + errorInfo);

                interstitialRetryAttempt++;
                float retryDelay = Mathf.Pow(2, interstitialRetryAttempt);

                DOVirtual.DelayedCall(interstitialRetryAttempt, () => AdsManager.RequestInterstitial(), true);//, UpdateMethod.Update);
            });
        }

        private void InterstitialOnAdOpenedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: InterstitialOnAdOpenedEvent event received");

                AdsManager.OnProviderAdDisplayed(AdProvider.Applovin, AdType.Interstitial);
            });

            AnalyticsController.OnAdMaxTrack(AdStepType.ad_show.ToString(), AnalyticsStr.adTypeIv, adInfo?.Placement, null, adInfo?.NetworkName);

            AnalyticsController.OnInterstitialDisplayed();
        }

        private void InterstitialOnAdShowFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Interstitial ad failed to load an ad with error: " + errorInfo);

                interstitialRetryAttempt++;
                float retryDelay = Mathf.Pow(2, interstitialRetryAttempt);

                DOVirtual.DelayedCall(interstitialRetryAttempt, () => AdsManager.RequestInterstitial(), true);//, UpdateMethod.Update);
            });

            AnalyticsController.OnAdMaxTrack(AdStepType.ad_fail.ToString(), AnalyticsStr.adTypeIv, adInfo?.Placement, errorInfo.Code.ToString(), adInfo?.NetworkName);
        }

        private void InterstitialOnAdClosedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: InterstitialOnAdClosedEvent event received");

                AdsManager.OnProviderAdClosed(AdProvider.Applovin, AdType.Interstitial);

                AdsManager.ExecuteInterstitialCallback(true);

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestInterstitial();
            });
        }

        private void OnInterstitialRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {

#if TEST_MODE
            Debug.Log($"[AdsManager]: Interstitial Ad Revenue Paid{adInfo.AdUnitIdentifier}");
#endif
            //adjust 上报广告收入
            AnalyticsController.OnAdRevenue(
                MaxSdk.GetSdkConfiguration().CountryCode,
                adInfo.Revenue,
                adInfo.NetworkName,
                adInfo.AdUnitIdentifier,
                adInfo.AdFormat,
                "MAX",
                0,
                adInfo.Placement,
                adInfo.Placement);
        }
        #endregion

        #region Banner callback handlers
        private void BannerOnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: BannerOnAdLoadedEvent event received");

                AdsManager.OnProviderAdLoaded(AdProvider.Applovin, AdType.Banner);
            });

#if TEST_MODE
            Debug.Log($"[AdsManager]: Banner Ad loaded:{adUnitId}");
#endif
        }
        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //adjust 上报广告收入
#if TEST_MODE
            Debug.Log($"[AdsManager]: Banner Ad Revenue Paid{adInfo.AdUnitIdentifier}");
#endif

            AnalyticsController.OnAdRevenue(
                MaxSdk.GetSdkConfiguration().CountryCode,
                adInfo.Revenue,
                adInfo.NetworkName,
                adInfo.AdUnitIdentifier,
                adInfo.AdFormat,
                "MAX",
                0,
                adInfo.Placement,
                adInfo.Placement);
        }

        #endregion

        public override void DestroyBanner()
        {
            MaxSdk.DestroyBanner(GetBannerID());

            isBannerLoaded = false;

            AdsManager.OnProviderAdClosed(AdProvider.Applovin, AdType.Banner);
        }

        public override void HideBanner()
        {
            if (isBannerLoaded)
                MaxSdk.HideBanner(GetBannerID());

            AdsManager.OnProviderAdClosed(AdProvider.Applovin, AdType.Banner);
        }

        public override void ShowBanner()
        {
            if (!isBannerLoaded)
            {
                MaxSdk.CreateBanner(GetBannerID(), new MaxSdkBase.AdViewConfiguration((MaxSdkBase.AdViewPosition)adsSettings.ApplovinContainer.BannerPosition));

                // Set background or background color for banners to be fully functional
                MaxSdk.SetBannerBackgroundColor(GetBannerID(), Color.white);

                isBannerLoaded = true;

                MaxSdk.ShowBanner(GetBannerID());
            }
            else
            {
                MaxSdk.ShowBanner(GetBannerID());

            }

            AdsManager.OnProviderAdDisplayed(AdProvider.Applovin, AdType.Banner);

#if TEST_MODE
            Debug.Log($"[AdsManager]: ShowBanner");
#endif
        }

        public override void RequestInterstitial()
        {
            MaxSdk.LoadInterstitial(GetInterstitialID());
        }

        public override void ShowInterstitial(AdvertisementCallback callback, string placement = "")
        {
            MaxSdk.ShowInterstitial(GetInterstitialID(), placement);
        }

        public override void RequestRewardedVideo()
        {
            MaxSdk.LoadRewardedAd(GetRewardedVideoID());
        }

        public override void ShowRewardedVideo(AdvertisementCallback callback, string placement = "")
        {
            MaxSdk.ShowRewardedAd(GetRewardedVideoID(), placement);
        }

        public override bool IsInterstitialLoaded()
        {
            return MaxSdk.IsInterstitialReady(GetInterstitialID());
        }

        public override bool IsRewardedVideoLoaded()
        {
            return MaxSdk.IsRewardedAdReady(GetRewardedVideoID());
        }

        public string GetBannerID()
        {
#if UNITY_ANDROID
            return adsSettings.ApplovinContainer.AndroidBannerID;
#elif UNITY_IOS
            return adsSettings.ApplovinContainer.IOSBannerID;
#else
            return string.Empty;
#endif
        }

        public string GetInterstitialID()
        {
#if UNITY_ANDROID
            return adsSettings.ApplovinContainer.AndroidInterstitialID;
#elif UNITY_IOS
            return adsSettings.ApplovinContainer.IOSInterstitialID;
#else
            return string.Empty;
#endif
        }

        public string GetRewardedVideoID()
        {
#if UNITY_ANDROID
            return adsSettings.ApplovinContainer.AndroidRewardedVideoID;
#elif UNITY_IOS
            return adsSettings.ApplovinContainer.IOSRewardedVideoID;
#else
            return string.Empty;
#endif
        }
    }
#endif
}