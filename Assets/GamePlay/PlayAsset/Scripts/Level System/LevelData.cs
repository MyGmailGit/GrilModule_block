#pragma warning disable 0649

using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/Level/Level Data", fileName = "Level Data")]
    public class LevelData : ScriptableObject
    {
        [SerializeField, LevelEditorSetting] Vector2Int size = new Vector2Int(8, 8);
        public Vector2Int Size => size;

        [SerializeField, LevelEditorSetting] LevelElementData[] levelElements;
        public LevelElementData[] LevelElements => levelElements;

        [SerializeField] float duration = 115;
        public float Duration => duration;

        [SerializeField] LevelType type;
        public LevelType Type => type;

        [SerializeField] string specialNote;
        public string SpecialNote => specialNote;

        [SerializeField] bool useInRandomizer = true;
        public bool UseInRandomizer => useInRandomizer;

        public void ApplyDurationOverride(int duration)
        {
            this.duration = duration;
        }

        public string GetCompressedLevelString()
        {
            string json = JsonUtility.ToJson(this);

            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(jsonBytes, 0, jsonBytes.Length);
                }

                byte[] result = output.ToArray();

                return System.Convert.ToBase64String(result);
            }
        }

        public LevelData DecompressLevel(string compressedLevelString)
        {
            byte[] compressedBytes = System.Convert.FromBase64String(compressedLevelString);
            using (MemoryStream input = new MemoryStream(compressedBytes))
            {
                using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(gzip, Encoding.UTF8))
                    {
                        string json = reader.ReadToEnd();
                        JsonUtility.FromJsonOverwrite(json, this);
                        return this;
                    }
                }
            }
        }

        [Button]
        public void Validate()
        {
            foreach (LevelElementData element in levelElements)
            {
                element.OnValidate();
            }

            RuntimeEditorUtils.SetDirty(this);
        }
    }
}