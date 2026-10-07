using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/Monthly/FiveHeartDailyData", fileName = "FiveHeartDailyData")]
    public class FiveHeartDailyData : ScriptableObject
    {
        [SerializeField] public int needHeartNum;
        [SerializeField] public RewardsSet rewardsSet;
    }
}
