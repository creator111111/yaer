using System.Collections;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEffects
{
    [RequireComponent(typeof(Animator))]
    public class RandomStartBlink : MonoBehaviour
    {
        private Animator m_Animator;

        private float nextTime;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            nextTime = Random.Range(3, 7);
        }

        private void Update()
        {
            nextTime -= Time.deltaTime;
            if (nextTime < 0)
            {
                nextTime = Random.Range(3, 7);
                m_Animator.SetTrigger("Blink");
            }
        }
    }
}