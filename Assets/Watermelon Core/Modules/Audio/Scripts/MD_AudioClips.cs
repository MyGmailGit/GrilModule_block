using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Audio Clips", menuName = "Data/Core/Audio Clips")]
    public class AudioClips : ScriptableObject
    {
        [BoxGroup("UI", "UI")]
        public AudioClip buttonSound;

        [BoxGroup("Gameplay", "Gameplay")]
        public AudioClip blockPick;

        [BoxGroup("Gameplay")]
        public AudioClip blockDestroy;

        [BoxGroup("Gameplay")]
        public AudioClip win;

        [BoxGroup("Gameplay")]
        public AudioClip completePopup;

        [BoxGroup("Gameplay")]
        public AudioClip lose;

        [BoxGroup("Gameplay")]
        public AudioClip revive;

        [BoxGroup("Gameplay")]
        public AudioClip actionDone;

        [BoxGroup("Gameplay")]
        public AudioClip bottleClose;

        [BoxGroup("Gameplay")]
        public AudioClip finish;

        [BoxGroup("Gameplay")]
        public AudioClip particle1;

        [BoxGroup("Gameplay")]
        public AudioClip particle2;

        [BoxGroup("Gameplay")]
        public AudioClip spark01;
        [BoxGroup("Gameplay")]
        public AudioClip Fly;

        [BoxGroup("Gameplay")]
        public AudioClip alert;

        [BoxGroup("Gameplay")]
        public AudioClip appear;

        [BoxGroup("Gameplay")]
        public AudioClip slide;

        [BoxGroup("Gameplay")]
        public AudioClip ballDrop;

        [BoxGroup("Gameplay")]
        public AudioClip startLevelLabel;


        [BoxGroup("Girl")]
        public AudioClip[] girls;


    }
}

// -----------------
// Audio Controller v 0.4
// -----------------
