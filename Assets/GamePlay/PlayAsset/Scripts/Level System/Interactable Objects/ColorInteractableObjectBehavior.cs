using UnityEngine;

namespace Watermelon
{
    public sealed class ColorInteractableObjectBehavior : InteractableObjectBehavior
    {
        private const string COLOR_PROPERTY = "_BaseColor";

        [SerializeField] MeshRenderer meshRenderer;
        [SerializeField] ColorData[] colorDatas;

        [Space]
        [Slider(0.0f, 1.0f)]
        [SerializeField] float defaultAlpha = 0.8f;
        [Slider(0.0f, 1.0f)]
        [SerializeField] float activeAlpha = 0.6f;

        private Color defaultColor;
        private Material meshMaterial;

        public override void OnCreated()
        {
            ColorData colorData = GetColorData(data.ObstacleColor);
            meshRenderer.material = colorData.Material;

            if (!Application.isPlaying)
            {
                meshMaterial = new Material(meshRenderer.sharedMaterial);
                meshRenderer.sharedMaterial = meshMaterial;
            }
            else
            {
                meshMaterial = meshRenderer.material;
            }

            defaultColor = meshMaterial.GetColor(COLOR_PROPERTY);

            SetAlpha(defaultAlpha);
        }

        public override void OnBlockPicked(LevelBlockBehavior levelBlockBehavior)
        {
            if (levelBlockBehavior.GetActiveBlockColor() == data.ObstacleColor)
            {
                SetAlpha(activeAlpha);
            }
        }

        public override void OnBlockReleased(LevelBlockBehavior levelBlockBehavior)
        {
            SetAlpha(defaultAlpha);
        }

        public override bool IsMovable(LevelBlockBehavior levelBlockBehavior)
        {
            return levelBlockBehavior.GetActiveBlockColor() == data.ObstacleColor;
        }

        public void SetAlpha(float alpha)
        {
            Color color = defaultColor;
            color.a = alpha;

            meshMaterial.SetColor(COLOR_PROPERTY, color);
        }

        public ColorData GetColorData(BlockColor color)
        {
            for (int i = 0; i < colorDatas.Length; i++)
            {
                if (colorDatas[i].Type == color)
                    return colorDatas[i];
            }

            Debug.LogError($"Color {color} not found in color data array.", this);

            return null;
        }

        [System.Serializable]
        public class ColorData
        {
            [SerializeField] BlockColor type;
            public BlockColor Type => type;

            [SerializeField] Material material;
            public Material Material => material;

            public ColorData(BlockColor type, Material material)
            {
                this.type = type;
                this.material = material;
            }
        }
    }
}
