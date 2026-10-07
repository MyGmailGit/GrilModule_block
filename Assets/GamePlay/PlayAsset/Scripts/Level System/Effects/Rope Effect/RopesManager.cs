// using System.Collections.Generic;

// namespace Watermelon
// {
//     public static class RopesManager
//     {
//         private static List<RopeEffectBehavior> registeredElements;

//         public static void Init()
//         {
//             registeredElements = new List<RopeEffectBehavior>();
//         }

//         public static void RegisterElement(RopeEffectBehavior element)
//         {
//             registeredElements.Add(element);
//         }

//         public static void UnregisterElement(RopeEffectBehavior element)
//         {
//             registeredElements.Remove(element);
//         }

//         public static RopeBehavior GetRopeBehavior(BlockColor color)
//         {
//             foreach(RopeEffectBehavior element in registeredElements)
//             {
//                 foreach(RopeBehavior rope in element.Ropes)
//                 {
//                     if (!rope.IsCut && rope.RopeColor == color)
//                     {
//                         return rope;
//                     }
//                 }
//             }

//             return null;
//         }
//     }
// }
