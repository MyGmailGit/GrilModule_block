using System.Collections;
using System.Collections.Generic;
using FirebaseRemote;
using GameLogic;
using UnityEngine;
using Watermelon;

public partial class AB_Ctrl : Singleton<AB_Ctrl>
{
    protected override void OnInit()
    {
        base.OnInit();
    }

    // 需要测试如果在过程中出现A面转B面的情况

    #region Video SerilNumber

    public List<string> allMainIds
    {
        get
        {
#if TEST_MODE
            // 如果设置强制走视频 ，就要修改哪些按钮不可见
            if (DevPanelEnabler.IsDevForceToVideo)
            {
                return allMainIds_B;
            }
#endif
            // string enterType = PlayerPrefs.GetString(Watermelon.AnalyticsEventType.ad_network.ToString(), AdjustAnalyticsModule.AdjustOrganic);//"organic");
            // if (string.Equals(enterType, AdjustAnalyticsModule.AdjustOrganic, System.StringComparison.OrdinalIgnoreCase)
            //     || FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA())

            if (!FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
            {
                return allMainIds_A;
            }

            return allMainIds_B;
        }
    }

    public List<string> fixedFirstMainIds
    {
        get
        {
#if TEST_MODE
            // 如果设置强制走视频 ，就要修改哪些按钮不可见
            if (DevPanelEnabler.IsDevForceToVideo)
            {
                return fixedFirstMainIds_B;
            }
#endif
            // string enterType = PlayerPrefs.GetString(Watermelon.AnalyticsEventType.ad_network.ToString(), AdjustAnalyticsModule.AdjustOrganic);//"organic");
            // if (string.Equals(enterType, AdjustAnalyticsModule.AdjustOrganic, System.StringComparison.OrdinalIgnoreCase)
            //     || FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA())
            if (!FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
            {
                return fixedFirstMainIds_A;
            }

            return fixedFirstMainIds_B;
        }
    }

    public List<int[]> stageFileRanges
    {
        get
        {
#if TEST_MODE
            // 如果设置强制走视频 ，就要修改哪些按钮不可见
            if (DevPanelEnabler.IsDevForceToVideo)
            {
                return stageFileRanges_B;
            }
#endif
            // string enterType = PlayerPrefs.GetString(Watermelon.AnalyticsEventType.ad_network.ToString(), AdjustAnalyticsModule.AdjustOrganic);//"organic");
            // if (string.Equals(enterType, AdjustAnalyticsModule.AdjustOrganic, System.StringComparison.OrdinalIgnoreCase)
            //     || FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA())
            if (!FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
            {
                return stageFileRanges_A;
            }
            return stageFileRanges_B;
        }
    }

    public (string mainId, int[] fileRange)? specialList
    {
        get
        {
#if TEST_MODE
            // 如果设置强制走视频 ，就要修改哪些按钮不可见
            if (DevPanelEnabler.IsDevForceToVideo)
            {
                return ("6200", new int[2] { 1, 100 });
            }
#endif
            // string enterType = PlayerPrefs.GetString(Watermelon.AnalyticsEventType.ad_network.ToString(), AdjustAnalyticsModule.AdjustOrganic);//"organic");
            // if (string.Equals(enterType, AdjustAnalyticsModule.AdjustOrganic, System.StringComparison.OrdinalIgnoreCase)
            //     || FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA())
            if (!FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
            {
                return null;
            }
            return ("6200", new int[2] { 1, 100 });
        }
    }

    /// <summary>
    /// 返回列表和是否是B面
    /// </summary>
    /// <returns></returns>
    public (List<SupriseData> data, bool isB) GetSupriseList()
    {
#if TEST_MODE
        // 如果设置强制走视频 ，就要修改哪些按钮不可见
        if (DevPanelEnabler.IsDevForceToVideo)
        {
            return (supriseDatas_B, true);
        }
#endif
        // string enterType = PlayerPrefs.GetString(Watermelon.AnalyticsEventType.ad_network.ToString(), AdjustAnalyticsModule.AdjustOrganic);//"organic");
        // if (string.Equals(enterType, AdjustAnalyticsModule.AdjustOrganic, System.StringComparison.OrdinalIgnoreCase)
        //     || FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA())
        if (!FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
        {
            return (supriseDatas_A, false);
        }
        return (supriseDatas_B, true);
    }

    // B

    // 所有主编号列表
    private List<string> allMainIds_B = new List<string>
        {
            "60001060","60001540","60002040","60002420","60002460","60002490","60002730","63303030",
            "63303330","63303360","63303700","63303790","63304450","63304640","63304660","63305030",
            "63305170","60000300","60000320","60002410","60002510","60002650","60002750","60002780",
            "60002970","63303100","63303190","63303300","63303420","63303430","63303470","63303520",
            "63303580","63303750","63303900","63303950","63304750","63304820","63304840","63305210",
            // "63305520",
        };

    // 63305520这个主编号只有12张，神经病

    // 固定的前4个主编号（用于第一次加载）
    private List<string> fixedFirstMainIds_B = new List<string>
        {
            "60001060", "60001540", "60002040", "60002420"
        };

    // 阶段顺序（需求：13-16, 09-12, 05-08, 01-04）
    private List<int[]> stageFileRanges_B = new List<int[]>
        {
            new int[] { 13, 16 }, // 阶段 0
            new int[] { 9, 12 },  // 阶段 1
            new int[] { 5, 8 },   // 阶段 2
            new int[] { 1, 4 }    // 阶段 3
        };

    public class SupriseData
    {
        public string titletxt;
        public CurrencyType currencyType;
        public int price;
        public string mainId;
        public int[] fileRange;
    }

    private List<SupriseData> supriseDatas_B = new List<SupriseData>()
    {
        new SupriseData(){titletxt = "Sexy", currencyType = CurrencyType.Coins, price = 150,
                mainId = "650010", fileRange = new int[2]{1, 150}},
        new SupriseData(){titletxt = "Uniform", currencyType = CurrencyType.Diamond, price = 15,
                mainId = "650020", fileRange = new int[2]{1, 100}},
        new SupriseData(){titletxt = "Anime", currencyType = CurrencyType.Diamond, price = 15,
                mainId = "650030", fileRange = new int[2]{1, 100}},
        new SupriseData(){titletxt = "Sexy Lingerie", currencyType = CurrencyType.Coins, price = 150,
                mainId = "650040", fileRange = new int[2]{1, 150}},
        new SupriseData(){titletxt = "Forbidden 18+", currencyType = CurrencyType.Diamond, price = 15,
                mainId = "650050", fileRange = new int[2]{1, 100}},
        new SupriseData(){titletxt = "Sweet Temptation", currencyType = CurrencyType.Diamond, price = 15,
                mainId = "650060", fileRange = new int[2]{1, 100}},
        new SupriseData(){titletxt = "Midnight Desire", currencyType = CurrencyType.Coins, price = 150,
                mainId = "650070", fileRange = new int[2]{1, 150}},
        new SupriseData(){titletxt = "Lovely", currencyType = CurrencyType.Diamond, price = 15,
                mainId = "650080", fileRange = new int[2]{1, 30}},
    };


    #endregion Video SerilNumber


    #region 姓名
    private Dictionary<string, string> namesKeyValuePairs = new Dictionary<string, string>();
    public string GetNameWithMainId(string mainid)
    {
        if (namesKeyValuePairs.TryGetValue(mainid, out var namestr))
        {
            return namestr;
        }

        for (int i = 0; i < allMainIds_B.Count; ++i)
        {
            if (mainid == allMainIds_B[i])
            {
                namesKeyValuePairs.Add(mainid, nameKayValue[i]);
                return nameKayValue[i];
            }
        }
        for (int i = 0; i < allMainIds_A.Count; ++i)
        {
            if (mainid == allMainIds_A[i])
            {
                namesKeyValuePairs.Add(mainid, nameKayValue[i]);
                return nameKayValue[i];
            }
        }
        return "";
    }
    private List<string> nameKayValue = new()
    {
        "Scarlett",
        "Vanessa",
        "Chloe",
        "Bella",
        "Sofia",
        "Isabella",
        "Victoria",
        "Natalia",
        "Bianca",
        "Adriana",
        "Valentina",
        "Camila",
        "Selena",
        "Sabrina",
        "Jasmine",
        "Naomi",
        "Nicole",
        "Samantha",
        "Jessica",
        "Ashley",
        "Madison",
        "Megan",
        "Amber",
        "Brooke",
        "Lexi",
        "Lola",
        "Jade",
        "Sasha",
        "Sienna",
        "Ruby",
        "Stella",
        "Mia",
        "Ava",
        "Zoe",
        "Layla",
        "Maya",
        "Elena",
        "Eva",
        "Ivy",
        "Nina",
        "Alexa",
        "Ariana",
        "Gabriella",
        "Daniela",
        "Alessandra",
        "Vivienne",
        "Celeste",
        "Roxanne",
        "Summer",
        "Skye",
        // "Emma",
        // "Olivia",
        // "Sophia",
        // "Isabella",
        // "Charlotte",
        // "Amelia",
        // "Elizabeth",
        // "Victoria",
        // "Catherine",
        // "Margaret",
        // "Eleanor",
        // "Alice",
        // "Grace",
        // "Claire",
        // "Anna",
        // "Ava",
        // "Mia",
        // "Harper",
        // "Evelyn",
        // "Abigail",
        // "Ella",
        // "Luna",
        // "Lily",
        // "Nora",
        // "Hazel",
        // "Violet",
        // "Aurora",
        // "Willow",
        // "Ruby",
        // "Ivy",
        // "Seraphina",
        // "Juniper",
        // "Clementine",
        // "Ophelia",
        // "Dahlia",
        // "Celeste",
        // "Freya",
        // "Maisie",
        // "Elodie",
        // "Gemma",
        // "Zoe",
        // "Kate",
        // "Jane",
        // "Rose",
        // "Anne",
        // "May",
        // "Eve",
        // "Joy",
        // "Skye",
        // "Bryn",
    };

    #endregion
}
