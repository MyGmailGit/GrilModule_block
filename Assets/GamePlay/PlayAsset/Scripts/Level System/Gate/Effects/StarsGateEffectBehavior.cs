// using System.Collections.Generic;
// using UnityEngine;

// namespace Watermelon
// {
//     public sealed class StarsGateEffectBehavior : GateEffectBehavior
//     {
//         [SerializeField] GameObject visualsPrefab;

//         public override void OnCreated(GateBehavior gateBehavior)
//         {
//             List<BorderData> unifiedElements = linkedGate.Data.UnifiedElements;
//             if(!unifiedElements.IsNullOrEmpty())
//             {
//                 foreach(BorderData element in unifiedElements)
//                 {
//                     Vector2Int position = element.Position;
//                     Vector3 offset = element.Offset;

//                     GameObject visualObject = Instantiate(visualsPrefab, transform);
//                     visualObject.transform.position = new Vector3(position.x + offset.x, GameConsts.BLOCK_HEIGHT, position.y + offset.z);
//                 }
//             }
//         }
//     }
// }