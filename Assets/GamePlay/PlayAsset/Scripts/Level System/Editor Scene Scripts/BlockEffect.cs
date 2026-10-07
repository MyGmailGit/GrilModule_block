using UnityEngine;

namespace Watermelon
{
    public class BlockEffect : MonoBehaviour
    {
        private static BlockEffect instance;
        public static BlockEffect Instance { get => instance; }

        //[UnpackNested]
        public BlockEffectData[] data; // this should be public for [UnpackNested] to work
        public BlockEffectData[] Data { get => data; set => data = value; }

        public BlockEffect()
        {
            instance = this;
        }
    }
}
