using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Game Data", menuName = "Data/Game Data")]
    public class GameData : ScriptableObject
    {
        [SerializeField] LevelDatabase levelDatabase;
        public LevelDatabase LevelDatabase => levelDatabase;

        [SerializeField] int reviveExtraSeconds = 20;
        public int ReviveExtraSeconds => reviveExtraSeconds;

        [SerializeField] int reviveDuration = 4;
        public int ReviveDuration => reviveDuration;

        [SerializeField] CurrencyAmount defaultReward = new CurrencyAmount(CurrencyType.Coins, 20);
        public CurrencyAmount DefaultReward => defaultReward;

        public static GameData Data { get; private set; }

        public void Init()
        {
            Data = this;
        }
    }
}
