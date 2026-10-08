#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Blocks Visuals Data", menuName = "Data/Level/Blocks Visuals Data")]
    public class BlocksVisualsData : ScriptableObject
    {
        [SerializeField] BlockData[] blocks;
        public BlockData[] Blocks => blocks;

        [SerializeField] BlockColorData[] colors;
        public BlockColorData[] Colors => colors;

        public void Init()
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].Init();
            }
        }

        public BlockData GetBlockData(BlockType blockType)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i].Type == blockType)
                    return blocks[i];
            }

            Debug.LogError($"Block data for {blockType} not found.");

            return null;
        }

        public BlockColorData GetColorData(BlockColor type)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].Type == type)
                    return colors[i];
            }

            Debug.LogError($"Block color data for {type} not found.");

            return null;
        }
    }
}
