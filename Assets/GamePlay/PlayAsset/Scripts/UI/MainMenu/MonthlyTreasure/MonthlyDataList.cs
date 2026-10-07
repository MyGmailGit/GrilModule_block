using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/Monthly/MonthlyDataList", fileName = "MonthlyDataList")]
    public class MonthlyDataList : ScriptableObject
    {
        [SerializeField] private List<MonthlyData> monthlyDataList;
        public List<MonthlyData> MonthlyDataListData => monthlyDataList;

        [SerializeField] private List<FiveHeartDailyData> fiveHeartDailyTask;
        public List<FiveHeartDailyData> FiveHeartDailyTask => fiveHeartDailyTask;
        public static MonthlyDataList Data { get; private set; }

        public void Init()
        {
            Data = this;
        }
    }
}
