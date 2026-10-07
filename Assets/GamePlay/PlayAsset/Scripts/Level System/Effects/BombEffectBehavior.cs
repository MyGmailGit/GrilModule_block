// using System;
// using TMPro;
// using UnityEngine;

// namespace Watermelon
// {
//     public sealed class BombEffectBehavior : BlockEffectBehavior
//     {
//         [SerializeField] TextMeshProUGUI timerText;

//         [Space]
//         [SerializeField] ParticleSystem explosionEffect;
//         [SerializeField] AudioClip explosionSound;

//         [Space]
//         [SerializeField] float offsetY = 0.1f;

//         private GameplayTimer timer;

//         public override int EffectSortingOrder => 2;

//         public override void OnCreated(LevelBlockBehavior blockBehavior)
//         {
//             Bounds bounds = blockBehavior.Figure.GetHorizontalCenterBounds();

//             transform.position = blockBehavior.transform.position + bounds.center + new Vector3(0, offsetY * orderID, 0);

//             timer = new GameplayTimer();
//             timer.SetMaxTime(effectData.bombDuration);
//             timer.OnTimeSpanChanged += UpdateTimerText;
//             timer.OnTimerFinished += OnTimerFinished;

//             timerText.text = effectData.bombDuration.ToString();

//             if(GameController.IsGameActivated)
//                 timer.Start();
//         }

//         public override void OnDisabled(LevelBlockBehavior blockBehavior)
//         {
//             timer.Pause();
//             timer = null;
//         }

//         public override bool OnGateEntered(GateBehavior gateBehavior, GateDirection gateDirection)
//         {
//             timer.Pause();

//             return base.OnGateEntered(gateBehavior, gateDirection);
//         }

//         private void OnTimerFinished()
//         {
//             explosionEffect.transform.SetParent(null);

//             ParticleCase particleCase = explosionEffect.PlayCase();
//             particleCase.Disabled += () =>
//             {
//                 Destroy(explosionEffect.gameObject);
//             };

//             AudioController.PlaySound(explosionSound);

//             Haptic.Play(Haptic.HAPTIC_HARD);

//             GameController.GameOver(true, 0.35f);

//             DisableEffect();
//         }

//         private void UpdateTimerText(TimeSpan timespan)
//         {
//             timerText.text = $"{timespan.Seconds + timespan.Minutes * 60}";
//         }

//         public override void OnGameStarted()
//         {
//             timer?.Start();
//         }

//         public override void OnGameEnded()
//         {
//             timer?.Pause();
//         }

//         public override void OnRevived()
//         {
//             timer?.Resume();
//         }

//         private void Update()
//         {
//             timer.Update();
//         }

//         public override bool CanBeReapplied()
//         {
//             return false;
//         }

//         public override BlockEffectData GetCurrentEffectData()
//         {
//             return new BlockEffectData(effectData) { bombDuration = timer.CurrentTimeSpan.Seconds };
//         }
//     }
// }
