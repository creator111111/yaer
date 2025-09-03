using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Effect
{
    public class ParticleComponent : MonoBehaviour, IParticleComponent
    {
        [SerializeField] private ParticleSystem ps;

        public bool play;

        private void Awake()
        {
            Find();
        }

        private void Update()
        {
        }

        private void OnValidate()
        {
            Find();
        }

        public GameObject GameObject => gameObject;

        public void Play(int times)
        {
        }

        public void Play()
        {
            ps.Play();
        }


        private void Find()
        {
            ps = GetComponent<ParticleSystem>();
        }

        public void Stop()
        {
            ps.Stop();
        }
    }
}