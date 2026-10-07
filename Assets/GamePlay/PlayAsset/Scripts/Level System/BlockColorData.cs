#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class BlockColorData
    {
        [SerializeField] BlockColor type;
        public BlockColor Type => type;

        [SerializeField] Material material;
        public Material Material => material;

        [SerializeField] Color color;
        public Color Color => color;
    }
}
