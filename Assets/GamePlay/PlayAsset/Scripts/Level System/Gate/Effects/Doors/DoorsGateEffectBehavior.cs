// using UnityEngine;

// namespace Watermelon
// {
//     public sealed class DoorsGateEffectBehavior : GateEffectBehavior
//     {
//         private const float COOLDOWN_DURATION = 0.1f;

//         [SerializeField] float arrowYOffset = 0.1f;

//         [Space]
//         [SerializeField] DoorsVisualsData[] doorsVisualsDatas;

//         private DoorsGateVisualsBehavior visualsBehavior;

//         private bool isOpened;
//         public bool IsOpened => isOpened;

//         private Transform arrowTransform;
//         private Vector3 arrowPosition;

//         private float lastActivateTime;

//         public override void OnCreated(GateBehavior gateBehavior)
//         {
//             arrowTransform = gateBehavior.ArrowTransform;
//             arrowPosition = arrowTransform.position;
//             arrowTransform.position = arrowPosition + new Vector3(0, arrowYOffset, 0);

//             isOpened = data.IsOpened;

//             int doorsSize = gateBehavior.Data.UnifiedElements.Count;
//             DoorsVisualsData doorsVisualsData = GetVisualsData(doorsSize);
//             if (doorsVisualsData != null)
//             {
//                 GameObject visualsObject = Instantiate(doorsVisualsData.VisualsPrefab, transform);
//                 visualsObject.transform.localPosition = Vector3.zero;

//                 visualsBehavior = visualsObject.GetComponent<DoorsGateVisualsBehavior>();
//                 visualsBehavior.Init(this, doorsSize);
//             }
//             else
//             {
//                 DisableEffect();

//                 return;
//             }
//         }

//         public override void OnDisabled(GateBehavior gateBehavior)
//         {
//             arrowTransform.position = arrowPosition;
//         }

//         public override bool CanGoThroughGate(LevelBlockBehavior levelBlockBehavior)
//         {
//             if (!isOpened)
//                 return false;

//             return base.CanGoThroughGate(levelBlockBehavior);
//         }

//         public override void OnBlockCollectedGlobal(LevelBlockBehavior levelBlockBehavior)
//         {
//             if (Time.time < lastActivateTime) return;

//             lastActivateTime = Time.time + COOLDOWN_DURATION;

//             isOpened = !isOpened;
//             if (isOpened)
//             {
//                 visualsBehavior.Open();
//             }
//             else
//             {
//                 visualsBehavior.Close();
//             }
//         }

//         private void OnDestroy()
//         {
//             visualsBehavior?.Unload();
//         }

//         private DoorsVisualsData GetVisualsData(int gateSize)
//         {
//             DoorsVisualsData tempVisualData = null;
//             int closestDifference = int.MaxValue;

//             for (int i = 0; i < doorsVisualsDatas.Length; i++)
//             {
//                 int difference = Mathf.Abs(doorsVisualsDatas[i].GateSize - gateSize);
//                 if (difference < closestDifference)
//                 {
//                     closestDifference = difference;
//                     tempVisualData = doorsVisualsDatas[i];
//                 }
//             }

//             return tempVisualData;
//         }

//         [System.Serializable]
//         public class DoorsVisualsData
//         {
//             [SerializeField] int gateSize;
//             public int GateSize => gateSize;

//             [SerializeField] GameObject visualsPrefab;
//             public GameObject VisualsPrefab => visualsPrefab;
//         }
//     }
// }