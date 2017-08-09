using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spewnity
{
     /// <summary>
     /// Attach this component to a GameObject with a particle system. When the particle system is 
     /// no longer producing particles and all particles are dead, it will destroy the GameObject.
     /// </summary>
    public class DestroyParticleSystem : MonoBehaviour
    {
		[Tooltip("Includes child particle systems in the check")]
		public bool withChildren = true;

        private ParticleSystem ps;

        void Awake()
        {
            ps = gameObject.GetComponent<ParticleSystem>();
			if(ps == null)
				throw new UnityException("DestroyParticleSystem must be attached a GameObject with a ParticleSystem.");
        }

        void Update()
        {
            if (!ps.IsAlive(withChildren))
                Destroy(gameObject);
        }
    }
}