using UnityEngine;

namespace Watermelon {

    [System.Serializable]
    public class ElementTypeEditorData
    {
        [SerializeField] private ElementType type;
        [SerializeField] private Color color;
        [SerializeField] private Texture2D texture;
    }
}
