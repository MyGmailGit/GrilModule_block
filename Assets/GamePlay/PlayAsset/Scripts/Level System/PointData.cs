using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    [System.Serializable]
    public class PointData
    {
        [SerializeField, FormerlySerializedAs("isActive")] bool isFilled;
        public bool IsFilled => isFilled;

        [SerializeField] bool useInHorizontalCenteredBounds;
        public bool UseInHorizontalCenteredBounds => useInHorizontalCenteredBounds;

        [SerializeField] bool useInVerticalCenteredBounds;
        public bool UseInVerticalCenteredBounds => useInVerticalCenteredBounds;

        public PointData(PointData pointData)
        {
            this.isFilled = pointData.isFilled;
            this.useInHorizontalCenteredBounds = pointData.useInHorizontalCenteredBounds;
            this.useInVerticalCenteredBounds = pointData.useInVerticalCenteredBounds;
        }

        public PointData(bool isFilled, bool useInHorizontalCenteredBounds, bool useInVerticalCenteredBounds)
        {
            this.isFilled = isFilled;
            this.useInHorizontalCenteredBounds = useInHorizontalCenteredBounds;
            this.useInVerticalCenteredBounds = useInVerticalCenteredBounds;
        }
    }
}
