using UnityEngine;

namespace Watermelon
{
    public class BlockClip
    {
        private static readonly int CLIP_PLANE_POSITION_ID = Shader.PropertyToID("_ClipPlanePosition");
        private static readonly int CLIP_PLANE_NORMAL_ID = Shader.PropertyToID("_ClipPlaneNormal");
        private static readonly int CLIPPING_ENABLED_ID = Shader.PropertyToID("_ClippingEnabled");

        private MeshRenderer meshRenderer;

        private MaterialPropertyBlock propertyBlock;

        public BlockClip(MeshRenderer meshRenderer, Vector3 position, Vector3 normal)
        {
            this.meshRenderer = meshRenderer;

            propertyBlock = new MaterialPropertyBlock();

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(CLIP_PLANE_POSITION_ID, position);
            propertyBlock.SetVector(CLIP_PLANE_NORMAL_ID, normal);
            propertyBlock.SetFloat(CLIPPING_ENABLED_ID, 1.0f);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        public void Destroy()
        {
            propertyBlock.Clear();
            meshRenderer.SetPropertyBlock(null);

            propertyBlock = null;
        }
    }
}