// using System;
// using System.Collections.Generic;
// using UnityEngine;

// namespace Watermelon
// {
//     public abstract class BlockEffectBehavior : MonoBehaviour
//     {
//         protected LevelBlockBehavior linkedBlock;

//         protected int orderID;
//         public int OrderID => orderID;

//         protected bool isActive;
//         public bool IsActive => isActive;

//         protected BlockEffectData effectData;
//         public BlockEffectData EffectData => effectData;

//         protected BlockEffectType type;
//         public BlockEffectType Type => type;

//         public virtual int EffectSortingOrder => 0;

//         public void SetOrder(int orderID)
//         {
//             this.orderID = orderID;
//         }

//         /// <summary>
//         /// Called when the effect is created and attached to a block.
//         /// Use this to initialize visuals, timers, or state.
//         /// </summary>
//         public virtual void OnCreated(LevelBlockBehavior blockBehavior) { }

//         /// <summary>
//         /// Called when the effect is disabled or removed from a block.
//         /// Use this to clean up visuals, unregister from managers, or reset state.
//         /// </summary>
//         public virtual void OnDisabled(LevelBlockBehavior blockBehavior) { }

//         /// <summary>
//         /// Called when the game starts.
//         /// Use this to start timers or animations that should only run during gameplay.
//         /// </summary>
//         public virtual void OnGameStarted() { }

//         /// <summary>
//         /// Called when the game ends.
//         /// </summary>
//         public virtual void OnGameEnded() { }

//         /// <summary>
//         /// Called when player revived and returned to the level.
//         /// </summary>
//         public virtual void OnRevived() { }

//         /// <summary>
//         /// Called when the block is collected (e.g., removed from the level).
//         /// Use this to trigger cleanup, effects, or disable the effect.
//         /// </summary>
//         public virtual void OnBlockCollected() { }

//         public virtual void OnBlockCollectedGlobal(LevelBlockBehavior levelBlockBehavior) { }

//         /// <summary>
//         /// Called when the block attempts to enter a gate.
//         /// Return true to allow passage, or false to block it.
//         /// Can be used to trigger logic on gate entry.
//         /// </summary>
//         public virtual bool OnGateEntered(GateBehavior gateBehavior, GateDirection gateDirection)
//         {
//             return gateBehavior.CanGoThroughGate(linkedBlock);
//         }

//         /// <summary>
//         /// Called when any block enters a gate, including this one.
//         /// Use this to decrement counters, play sounds, or trigger particles.
//         /// </summary>
//         public virtual void OnBlockEnteredGate(LevelBlockBehavior levelBlockBehavior, GateBehavior gateBehavior) { }

//         /// <summary>
//         /// Allows the effect to override the block's movement matrix.
//         /// Use this to restrict or modify movement directions.
//         /// </summary>
//         public virtual void OverrideMovementMatrix(ref int[,] movementMatrix) { }

//         /// <summary>
//         /// Determines whether the block with this attached effect can be clicked.
//         /// If this method returns <c>false</c>, the block will not respond to click events and will instead play a shake animation.
//         /// Override this method in derived classes to implement custom clickability logic for specific effects.
//         /// </summary>
//         public virtual bool IsClickable()
//         {
//             return true;
//         }

//         /// <summary>
//         /// Determines whether the block should move as part of a group.
//         /// Return true for effects that link multiple blocks together.
//         /// </summary>
//         public virtual bool MoveMultiplyObjects()
//         {
//             return false;
//         }

//         /// <summary>
//         /// Returns a list of blocks linked by this effect.
//         /// Used for group movement or combined effects.
//         /// </summary>
//         public virtual List<LevelBlockBehavior> GetLinkedBlocks()
//         {
//             return new List<LevelBlockBehavior> { linkedBlock };
//         }

//         /// <summary>
//         /// Determines whether the block can pass through a specific gate.
//         /// Override to implement custom gate-passing logic.
//         /// </summary>
//         public virtual bool CanGoThroughGate(GateBehavior gateBehavior)
//         {
//             return gateBehavior.Data.LevelElementData.BlockColor == linkedBlock.GetActiveBlockColor();
//         }

//         /// <summary>
//         /// Returns the block color to use for logic and visuals.
//         /// Override to provide a custom color (e.g., for layered or overridden effects).
//         /// </summary>
//         public virtual BlockColor GetOverridedBlockColor()
//         {
//             return linkedBlock.ColorData.Type;
//         }

//         /// <summary>
//         /// Called when a block is destructed (removed or destroyed) in the level.
//         /// Removes the specified block from the ActiveBlocks collection,
//         /// ensuring it is no longer tracked as part of the active level state.
//         /// </summary>
//         /// <param name="block">The block that was destructed.</param>
//         public virtual void OnBlockDestructed(LevelBlockBehavior levelBlockBehavior)
//         {

//         }

//         public virtual void OnNewEffectAddedToBlock(BlockEffectBehavior effect)
//         {

//         }

//         public virtual bool CanBeReapplied()
//         {
//             return true;
//         }

//         public virtual void OnBlockSplit()
//         {

//         }

//         public virtual BlockEffectData GetCurrentEffectData()
//         {
//             return effectData;
//         }

//         public void DisableEffect()
//         {
//             isActive = false;

//             gameObject.SetActive(false);

//             linkedBlock?.OnEffectDisabled(this);
//         }

//         public BlockEffectBehavior ApplyEffect(LevelBlockBehavior blockBehavior, BlockEffectData effectData)
//         {
//             GameObject effectObject = Instantiate(gameObject);
//             effectObject.transform.SetParent(blockBehavior.transform);
//             effectObject.transform.ResetLocal();

//             BlockEffectBehavior effectBehavior = effectObject.GetComponent<BlockEffectBehavior>();
//             effectBehavior.linkedBlock = blockBehavior;
//             effectBehavior.type = effectData.Type;
//             effectBehavior.effectData = effectData;
//             effectBehavior.isActive = true;

//             blockBehavior.ApplyEffect(effectBehavior);

//             return effectBehavior;
//         }
//     }
// }
