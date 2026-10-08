using UnityEngine;

namespace Watermelon
{
    public class CombinedEffectVisuals : MonoBehaviour
    {
        [SerializeField] MeshRenderer blockAMeshRenderer;
        [SerializeField] MeshRenderer blockBMeshRenderer;

        private CombinedEffectBehavior.ConnectedBlocks connectedBlocks;

        public void Init(CombinedEffectBehavior.ConnectedBlocks connectedBlocks)
        {
            this.connectedBlocks = connectedBlocks;

            blockAMeshRenderer.material = connectedBlocks.BlockA.ColorData.Material;
            blockBMeshRenderer.material = connectedBlocks.BlockB.ColorData.Material;
        }
    }
}
