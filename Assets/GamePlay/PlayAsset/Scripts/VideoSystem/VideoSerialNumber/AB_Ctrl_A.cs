using System.Collections;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;
using Watermelon;

public partial class AB_Ctrl : Singleton<AB_Ctrl>
{
    // A

    private List<string> allMainIds_A = new List<string>
    {
        "60002428",
        "60002468",
        "63303038",
        "63303798",
        "63305038",
        "63303108",
        "63303528",
        "63303758",
        "63304828",
        "63304848",
    };
    // 固定的前4个主编号（用于第一次加载）
    private List<string> fixedFirstMainIds_A = new List<string>
        {
            "60002428", "60002468", "63303038", "63303798"
        };

    private List<int[]> stageFileRanges_A = new List<int[]>
        {
            new int[] { 9, 12 },  // 阶段 0
            new int[] { 5, 8 },   // 阶段 1
            new int[] { 1, 4 }    // 阶段 2
        };

    private List<SupriseData> supriseDatas_A = new List<SupriseData>()
    {
        new SupriseData(){titletxt = "Sexy", currencyType = CurrencyType.Coins, price = 300,
                mainId = "650010", fileRange = new int[2]{1, 150}},
        new SupriseData(){titletxt = "Dom's Discipline", currencyType = CurrencyType.Diamond, price = 15,
                mainId = "650020", fileRange = new int[2]{1, 100}},
        new SupriseData(){titletxt = "Silky Seduction", currencyType = CurrencyType.Diamond, price = 15,
                mainId = "650030", fileRange = new int[2]{1, 100}},
    };
}
