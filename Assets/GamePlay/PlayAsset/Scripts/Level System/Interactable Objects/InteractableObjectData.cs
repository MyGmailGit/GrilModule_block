using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class InteractableObjectData
    {
#if UNITY_EDITOR
        // Mapping from type to the serialized field names we should draw
        public static readonly Dictionary<InteractableObjectType, string[]> FIELDS = new Dictionary<InteractableObjectType, string[]>
        {
            { InteractableObjectType.ColorObstacle, new[] { "obstacleColor" } },
        };
#endif

        [SerializeField] InteractableObjectType type;
        public InteractableObjectType Type => type;

        [SerializeField] BlockColor obstacleColor;
        public BlockColor ObstacleColor => obstacleColor;
    }
}
