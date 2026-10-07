// using TMPro;
// using UnityEngine;

// namespace Watermelon
// {
//     public sealed class GateIceEffectBehavior : GateEffectBehavior
//     {
//         [SerializeField] TextMeshProUGUI turnsText;
//         [SerializeField] Material iceMaterial;

//         [Space]
//         [SerializeField] AudioClip turnSound;
//         [SerializeField] AudioClip disableSound;

//         [Space]
//         [SerializeField] ParticleSystem turnParticle;
//         [SerializeField] ParticleSystem disableParticle;

//         private Material storedMaterial;
//         private Material meshMaterial;
//         private int turnsLeft;

//         public override void OnCreated(GateBehavior gateBehavior)
//         {
//             if (!Application.isPlaying)
//             {
//                 meshMaterial = new Material(gateBehavior.GraphicsMeshRenderer.sharedMaterial);
//             }
//             else
//             {
//                 meshMaterial = gateBehavior.GraphicsMeshRenderer.material;
//             }

//             storedMaterial = meshMaterial;
//             gateBehavior.GraphicsMeshRenderer.material = iceMaterial;

//             transform.rotation = Quaternion.identity;

//             turnsLeft = data.IceTurnsAmount;
//             turnsText.text = turnsLeft.ToString();
//         }

//         public override void OnDisabled(GateBehavior gateBehavior)
//         {
//             gateBehavior.GraphicsMeshRenderer.material = storedMaterial;
//         }

//         public override void OnBlockCollectedGlobal(LevelBlockBehavior levelBlockBehavior)
//         {
//             turnsLeft--;

//             if (turnsLeft <= 0)
//             {
//                 if (disableSound != null)
//                     AudioController.PlaySound(disableSound);

//                 if (disableParticle != null)
//                 {
//                     disableParticle.transform.SetParent(null);
//                     disableParticle.PlayCase().Disabled += () =>
//                     {
//                         Destroy(disableParticle.gameObject);
//                     };
//                 }

//                 DisableEffect();
//             }
//             else
//             {
//                 if (turnSound != null)
//                     AudioController.PlaySound(turnSound);

//                 if (turnParticle != null)
//                     turnParticle.Play();

//                 turnsText.text = turnsLeft.ToString();
//             }
//         }

//         public override bool CanGoThroughGate(LevelBlockBehavior levelBlockBehavior)
//         {
//             return false;
//         }
//     }
// }
