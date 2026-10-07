using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/Monthly/Monthly Data", fileName = "MonthlyData")]
    public class MonthlyData : ScriptableObject
    {
        [SerializeField] public int need;
        [SerializeField] public bool limitedTag_free;
        [SerializeField] public bool limitedTag_prem;

        [SerializeField] public RewardsSet rewardsFreeSet;
        [SerializeField] public RewardsSet rewardsPremSet;
    }
}
