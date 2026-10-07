// #define TEST_MODE

namespace Watermelon
{
    public enum AnalyticsEventType
    {
        // user Perporty
        ads_amout = 1001,
        rewardvideo_amout = 1002,
        interstitial_amout = 1003,

        //用户来源渠道
        ad_network = 1004,
        //广告单元id
        campaign_id = 1005,
        //广告单元名字
        campaign_name = 1006,
        //广告素材名字
        creative_name = 1007,
        //广告单元
        site_id = 1008,
        /// <summary>
        /// 走B看视频
        /// </summary>
        user_video_b = 1009,


        // Event
        InterstitialDisplayed = 2001,
        RVDisplayed = 2002,
        loading_start = 2003,

        loading_end = 2004,

        ad_revenue = 2005,
        MaxTrack = 2006,

        GameLevel = 2007,


        http_error = 2008,
        data_error = 2009,
        ser_error = 2010,
        user_data_error = 2011,

        /// <summary>
        /// 无论是走哪边每次进游戏都传一次是走视频还是本地图
        /// </summary>
        ad_network_video = 2012,

        /// <summary>
        /// Iap Event
        /// </summary>
        iap_event = 2013,
        /// <summary>
        ///  adjust attribution 获取到数据
        /// </summary>
        adj_attribution_get = 2014,







        // // Events
        // CurrencySource = 10,
        // CurrencySink = 11,

        // IAPClicked = 20,
        // IAPPurchased = 21,
        // IAPFailed = 22,
        // IAPFirstPurchase = 23,

        // AdFreePeriodExpired = 25,



    }

    public enum IAPStatus
    {
        start,
        success,
        cancel,
        failed,
    }

    public enum AdStepType
    {
        /// <summary>
        /// :应用请求（应用向SDK发起请求）
        /// </summary>
        ad_start,
        /// <summary>
        /// :填充成功
        /// </summary>
        ad_ready,
        /// <summary>
        /// :广告展示出来
        /// </summary>
        ad_show,
        /// <summary>
        /// :用户获取奖励
        /// </summary>
        ad_complete,
        /// <summary>
        /// :广告请求失败的时候
        /// </summary>
        ad_fail,
        /// <summary>
        /// :广告关闭的时候
        /// </summary>
        ad_closed,
        /// <summary>
        /// :点击播放广告的时候
        /// </summary>
        ad_call,
    }
}
