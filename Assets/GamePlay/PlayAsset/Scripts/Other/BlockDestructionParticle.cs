using UnityEngine;

namespace Watermelon
{
    public class BlockDestructionParticle
    {
        private static readonly int PARTICLE_HASH = "Block Destruction".GetHashCode();

        private readonly ParticleCase[] particleCases;

        public BlockDestructionParticle(LevelBlockBehavior blockBehavior, Material material, GateBehavior gateBehavior, GateDirection gateDirection)
        {
            int size = gateDirection.GetAlignedSize(blockBehavior.Figure);
            particleCases = new ParticleCase[size];
            for (int i = 0; i < size; i++)
            {
                Vector2Int pos = gateDirection.CalculateAlignedPosition(blockBehavior.MatrixPosition, blockBehavior.Figure, i);

                Vector3 particlePosition = new Vector3(pos.x, 0, pos.y);

                ParticleCase particleCase = ParticlesController.PlayParticle(PARTICLE_HASH);
                particleCase.SetPosition(particlePosition);
                particleCase.SetRotation(gateDirection.ParticleRotation);

                ParticleSystem particleSystem = particleCase.ParticleSystem;

                ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.material = material;

                particleCases[i] = particleCase;
            }
        }

        public void Stop()
        {
            foreach (ParticleCase particleCase in particleCases)
            {
                if (particleCase != null)
                {
                    particleCase.ParticleSystem.Stop();
                }
            }
        }
    }
}