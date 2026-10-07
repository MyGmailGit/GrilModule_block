#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class LevelFigure
    {
        [LevelEditorSetting]
        [SerializeField] Vector2Int size = new Vector2Int(3, 3);
        public Vector2Int Size => size;

        [SerializeField, LevelEditorSetting] PointData[] points;
        public PointData[] Points => points;

        [SerializeField, LevelEditorSetting] int activePoints = -1;
        public int ActivePoints => activePoints;

        [SerializeField, LevelEditorSetting] Vector2Int pivotPoint = new Vector2Int(0, 0);
        public Vector2Int PivotPoint => pivotPoint;

        public LevelFigure Clone()
        {
            LevelFigure levelFigure = new LevelFigure();
            levelFigure.size = size;
            levelFigure.points = new PointData[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                levelFigure.points[i] = new PointData(points[i]);
            }
            levelFigure.activePoints = activePoints;
            levelFigure.pivotPoint = pivotPoint;

            return levelFigure;
        }

        public PointData GetRegularPoint()
        {
            if (points.Length == 0)
                return null;

            int startIndex = Random.Range(0, points.Length + 1);
            for (int i = 0; i < points.Length; i++)
            {
                int index = (startIndex + i) % points.Length;
                if (points[index].IsFilled)
                    return points[index];
            }

            return null;
        }

        public Bounds GetHorizontalCenterBounds()
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            float minY = float.MaxValue;
            float maxY = float.MinValue;
            
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].UseInHorizontalCenteredBounds)
                {
                    float x = i % size.x;
                    float y = i / size.x;

                    if (x < minX)
                        minX = x;
                    if (x > maxX)
                        maxX = x;
                    if (y < minY)
                        minY = y;
                    if(y > maxY)
                        maxY = y;
                }
            }

            if (minX == float.MaxValue || maxX == float.MinValue || minY == float.MaxValue || maxY == float.MinValue)
                return new Bounds(Vector3.zero, Vector3.zero);

            return new Bounds(new Vector3((minX + maxX) / 2, 0, (minY + maxY) / 2), new Vector3(maxX - minX + 1, 0, maxY - minY + 1));
        }

        public Bounds GetVerticalCenterBounds()
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].UseInVerticalCenteredBounds)
                {
                    float x = i % size.x;
                    float y = i / size.x;

                    if (x < minX)
                        minX = x;
                    if (x > maxX)
                        maxX = x;
                    if (y < minY)
                        minY = y;
                    if (y > maxY)
                        maxY = y;
                }
            }

            if (minX == float.MaxValue || maxX == float.MinValue || minY == float.MaxValue || maxY == float.MinValue)
                return new Bounds(Vector3.zero, Vector3.zero);

            return new Bounds(new Vector3((minX + maxX) / 2, 0, (minY + maxY) / 2), new Vector3(maxX - minX + 1, 0, maxY - minY + 1));
        }
    }
}