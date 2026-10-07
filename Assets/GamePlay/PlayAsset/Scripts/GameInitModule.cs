using Data;
using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Game Settings")]
    public class GameInitModule : InitModule
    {
        public override string ModuleName => "Game Settings";

        [SerializeField] GameData gameData;
        [SerializeField] TreasureDataList treasureDataList;
        [SerializeField] MonthlyDataList monthlyDataList;

        public override void CreateComponent()
        {
            gameData.Init();
            treasureDataList.Init();
            monthlyDataList.Init();
        }
    }
}