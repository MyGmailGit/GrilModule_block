// using UnityEngine;

// namespace Watermelon
// {
//     [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
//     public class RopeBehavior : MonoBehaviour
//     {
//         [OnValueChanged("OnRopeTypeChanged")]
//         [SerializeField] RopeType ropeType;
//         public RopeType RopeType => ropeType;

//         [SerializeField] ParticleSystem cutParticleSystem;

//         [Space]
//         [SerializeField] VisualData[] visualDatas;

//         private RopeEffectBehavior ropeEffectBehavior;

//         private VisualData visualData;
//         private BlockColorData colorData;

//         private MeshFilter meshFilter;
//         private MeshRenderer meshRenderer;

//         private bool isCut;
//         public bool IsCut => isCut;

//         public BlockColor RopeColor => colorData.Type;

//         public void Init(RopeEffectBehavior ropeEffectBehavior, RopeEffectBehavior.RopeTransform transformData, BlockColor color)
//         {
//             this.ropeEffectBehavior = ropeEffectBehavior;

//             meshFilter = GetComponent<MeshFilter>();
//             meshRenderer = GetComponent<MeshRenderer>();

//             visualData = GetVisualData(transformData.RopeType);
//             // colorData = LevelController.GetBlockColorData(color);

//             transform.localPosition = transformData.Position;
//             transform.localEulerAngles = transformData.Rotation;
//             transform.localScale = transformData.Scale;

//             meshFilter.mesh = visualData.Mesh;
//             meshRenderer.material = colorData.Material;

//             ParticleSystem.MainModule mainParticle = cutParticleSystem.main;
//             mainParticle.startColor = colorData.Material.color;
//         }

//         public void OnRopeLinked()
//         {
//             if (isCut) return;

//             isCut = true;
//         }

//         public void OnRopeCut()
//         {
//             cutParticleSystem.Play();

//             meshFilter.mesh = visualData.CutMesh;

//             ropeEffectBehavior.OnRopeCut(this);
//         }

//         public void OnRopeTypeChanged()
//         {
//             VisualData visualData = GetVisualData(ropeType);
//             if (visualData != null)
//             {
//                 MeshFilter meshFilter = GetComponent<MeshFilter>();
//                 meshFilter.mesh = visualData.Mesh;
//             }
//         }

//         private VisualData GetVisualData(RopeType ropeType)
//         {
//             for (int i = 0; i < visualDatas.Length; i++)
//             {
//                 if (visualDatas[i].RopeType == ropeType)
//                 {
//                     return visualDatas[i];
//                 }
//             }

//             Debug.LogError($"Rope visuals data not found for element type: {ropeType} in {gameObject.name}.", gameObject);

//             return visualDatas[0];
//         }

//         [System.Serializable]
//         public class VisualData
//         {
//             [SerializeField] RopeType ropeType;
//             public RopeType RopeType => ropeType;

//             [SerializeField] Mesh mesh;
//             public Mesh Mesh => mesh;

//             [SerializeField] Mesh cutMesh;
//             public Mesh CutMesh => cutMesh;
//         }
//     }
// }
