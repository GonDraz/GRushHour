using UnityEngine;

namespace GonDraz.Effects
{
    public class BaseParticleEffect : BaseEffect
    {
        [Header("Particle Settings")] [SerializeField]
        protected ParticleSystem particle;

        protected override void OnSetup()
        {
            base.OnSetup();
            if (!particle)
                particle = GetComponent<ParticleSystem>();

            if (particle)
            {
                particle.Stop();
                var main = particle.main;
                duration = Mathf.Max(main.duration + main.startLifetime.constantMax);
            }
        }

        public override void OnGetFromPool()
        {
            base.OnGetFromPool();

            if (particle)
            {
                particle.Clear();
                particle.Stop();
            }
        }

        protected override void OnPlay()
        {
            if (particle)
            {
                particle.Play();

                if (autoReturnToPool) Invoke(nameof(CompleteEffect), duration);
            }
        }

        protected override void OnStop()
        {
            CancelInvoke(nameof(CompleteEffect));

            if (particle) particle.Stop();
        }
    }
}