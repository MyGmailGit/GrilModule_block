// using System;
// using UnityEngine;

// namespace Watermelon
// {
//     public abstract class GateEffectBehavior : MonoBehaviour
//     {
//         protected GateBehavior linkedGate;

//         protected int orderID;
//         public int OrderID => orderID;

//         protected bool isActive;
//         public bool IsActive => isActive;

//         protected GateEffectData data;
//         public GateEffectData Data => data;

//         protected GateEffectType type;
//         public GateEffectType Type => type;

//         public void SetOrder(int orderID)
//         {
//             this.orderID = orderID;
//         }

//         /// <summary>
//         /// Called when the effect is created and attached to a gate.
//         /// Use this to initialize visuals, set up state, or register with managers.
//         /// </summary>
//         public virtual void OnCreated(GateBehavior gateBehavior) { }

//         /// <summary>
//         /// Called when the effect is disabled or removed from a gate.
//         /// Use this to clean up visuals, reset transforms, or unregister from managers.
//         /// </summary>
//         public virtual void OnDisabled(GateBehavior gateBehavior) { }

//         /// <summary>
//         /// Called when a block enters the gate.
//         /// Use this to update state, trigger animations, or modify gate logic.
//         /// </summary>
//         public virtual void OnBlockEntered(LevelBlockBehavior pickedBlock) { }

//         /// <summary>
//         /// Determines whether a block can pass through this gate.
//         /// Override to implement custom gate-passing logic (e.g., locked doors, color checks).
//         /// </summary>
//         /// <param name="levelBlockBehavior">The block attempting to pass through the gate.</param>
//         /// <returns>True if the block can pass, false otherwise.</returns>
//         public virtual bool CanGoThroughGate(LevelBlockBehavior levelBlockBehavior)
//         {
//             return linkedGate.Data.LevelElementData.BlockColor == levelBlockBehavior.GetActiveBlockColor();
//         }

//         /// <summary>
//         /// Called when a block is destructed (removed or destroyed) in the level.
//         /// Removes the specified block from the <c>ActiveBlocks</c> collection,
//         /// ensuring it is no longer tracked as part of the active level state.
//         /// </summary>
//         /// <param name="block">The block that was destructed.</param>
//         public virtual void OnBlockDestructed(LevelBlockBehavior destructedBlock) { }

//         public virtual void OnBlockCollectedGlobal(LevelBlockBehavior levelBlockBehavior) { }

//         public void DisableEffect()
//         {
//             isActive = false;

//             gameObject.SetActive(false);

//             linkedGate?.OnEffectDisabled(this);
//         }

//         public GateEffectBehavior ApplyEffect(GateBehavior gateBehavior, GateEffectType type, GateEffectData effectData)
//         {
//             GameObject effectObject = Instantiate(gameObject);
//             effectObject.transform.SetParent(gateBehavior.transform);
//             effectObject.transform.ResetLocal();

//             GateEffectBehavior effectBehavior = effectObject.GetComponent<GateEffectBehavior>();
//             effectBehavior.linkedGate = gateBehavior;
//             effectBehavior.type = type;
//             effectBehavior.data = effectData;
//             effectBehavior.isActive = true;

//             gateBehavior.ApplyEffect(effectBehavior);

//             return effectBehavior;
//         }

//         public void LinkToNewGate(GateBehavior gateBehavior)
//         {
//             this.linkedGate = gateBehavior;
//             this.isActive = true;

//             gateBehavior.ApplyEffect(this);
//         }
//     }
// }