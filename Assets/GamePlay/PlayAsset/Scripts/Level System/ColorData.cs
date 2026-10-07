using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class EditorColorData
    {
        [SerializeField] BlockColor type;
        [SerializeField] Color color;
    }
}