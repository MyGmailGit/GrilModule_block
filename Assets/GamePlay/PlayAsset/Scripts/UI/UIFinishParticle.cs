using System.Collections;
using UnityEngine;

namespace Watermelon
{
    public class UIFinishParticle : UIPage
    {
        [SerializeField] ParticleSystem particleOne;
        [SerializeField] ParticleSystem particleTwo;

        public override void Init()
        {
        }

        public override void PlayHideAnimation()
        {
            particleOne.Stop();
            particleTwo.Stop();
            particleOne.Clear();
            particleTwo.Clear();
        }

        public override void PlayShowAnimation()
        {
            PlayFinishParticle();
        }

        public void PlayFinishParticle()
        {
            StartCoroutine(PlayFinishParticles());
        }
        IEnumerator PlayFinishParticles()
        {
            particleOne.Clear();
            particleTwo.Clear();
            particleOne.Play();
            yield return new WaitForSeconds(0.7f);
            particleTwo.Play();
        }

    }
}
