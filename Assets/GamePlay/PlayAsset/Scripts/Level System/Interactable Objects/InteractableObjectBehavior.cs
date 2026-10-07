// using UnityEngine;

// namespace Watermelon
// {
//     public class InteractableObjectBehavior : MonoBehaviour
//     {
//         protected Vector2Int position;
//         public Vector2Int Position => position;

//         protected InteractableObjectData data;
//         public InteractableObjectData Data => data;

//         public void Init(InteractableObjectData data, Vector2Int position)
//         {
//             this.position = position;
//             this.data = data;

//             OnCreated();
//         }

//         public virtual void OnCreated() { }

//         public virtual void OnBlockPicked(LevelBlockBehavior levelBlockBehavior) { }
//         public virtual void OnBlockReleased(LevelBlockBehavior levelBlockBehavior) { }

//         public virtual bool IsMovable(LevelBlockBehavior levelBlockBehavior) => false;
//     }
// }
