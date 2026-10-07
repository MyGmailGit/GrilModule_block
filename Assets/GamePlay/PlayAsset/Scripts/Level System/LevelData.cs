using Unity.Mathematics;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using Utility;
using System.Collections.Generic;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/Level/Level Data", fileName = "Level Data")]
    public class LevelData : ScriptableObject
    {
        [SerializeField] int levelID;
        public int LevelID => levelID;

        [SerializeField] int tubeCount;
        public int TubeCount => tubeCount;
        [SerializeField] string puzzleConfig;

        [SerializeField] bool isHideLiquid;

        [SerializeField] int emptyTubeCount;
        public int EmptyTubeCount => emptyTubeCount;

        [SerializeField] int difficulty;
        public int Difficulty => difficulty;

        private string encryptionKeyPrefix = "_ball_";

        /// <summary>
        /// 解密方法（供运行时使用）
        /// </summary>
        private string XorDecrypt(string encrypted, string key)
        {
            if (string.IsNullOrEmpty(encrypted))
                return encrypted;

            try
            {
                byte[] encryptedBytes = System.Convert.FromBase64String(encrypted);
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] resultBytes = CryptoHelper.XorAll(encryptedBytes, keyBytes);

                return Encoding.UTF8.GetString(resultBytes);
            }
            catch
            {
                Debug.LogError("Failed to decrypt puzzle config!");
                return encrypted;
            }
        }

        public List<int4> GetPuzzleConfig()
        {
            var decryptedConfig = XorDecrypt(puzzleConfig, $"{levelID}{encryptionKeyPrefix}{levelID}");

            if (string.IsNullOrEmpty(decryptedConfig))
                return new List<int4>();

            string[] tubeConfigs = decryptedConfig.Split('|');
            List<int4> result = new List<int4>();

            // 每4个数字组成一个int4
            for (int i = 0; i < tubeConfigs.Length; i += 4)
            {
                int x = int.Parse(tubeConfigs[i].Trim());
                int y = (i + 1 < tubeConfigs.Length) ? int.Parse(tubeConfigs[i + 1].Trim()) : 0;
                int z = (i + 2 < tubeConfigs.Length) ? int.Parse(tubeConfigs[i + 2].Trim()) : 0;
                int w = (i + 3 < tubeConfigs.Length) ? int.Parse(tubeConfigs[i + 3].Trim()) : 0;

                result.Add(new int4(x, y, z, w));
            }

            return result;
        }





        Vector2Int size = new Vector2Int(8, 8);
        public Vector2Int Size => size;

        // LevelElementData[] levelElements;
        // public LevelElementData[] LevelElements => levelElements;

        float duration = 115;
        public float Duration => duration;

        LevelType type;
        public LevelType Type => type;

        string specialNote;
        public string SpecialNote => specialNote;

        bool useInRandomizer = true;
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
            // foreach (LevelElementData element in levelElements)
            // {
            //     element.OnValidate();
            // }

            RuntimeEditorUtils.SetDirty(this);
        }
    }
}