using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
namespace Data
{
    public enum TreasureType
    {
        Coin = 0,
        PowerUp = 1,
        Iap = 2,
    }
    [CreateAssetMenu(fileName = "Treasure Data", menuName = "Data/Treasure/Treasure Data")]
    public class TreasureData : ScriptableObject
    {
        [Group("Treasure")]
        [SerializeField] TreasureType treasureType;
        public TreasureType TreasureType => treasureType;

        [SerializeField] RewardsSet rewardsSet;
        public RewardsSet RewardsSet => rewardsSet;

        // [SerializeField] int coinAmount;
        // public int CoinAmount => coinAmount;

        [SerializeField] ProductKeyType productKeyType;
        public ProductKeyType ProductKeyType => productKeyType;
    }
}
