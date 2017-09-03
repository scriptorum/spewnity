using System.Collections;
using System.Collections.Generic;
using Spewnity.ParticleMonitorInternal;
using UnityEngine;
using UnityEngine.Events;

// TODO FIX: Some births aren't being caught. Seems to be an issue with the ParticleSystem. Bug reported.
//  https://issuetracker.unity3d.com/issues/particlesystem-birth-subemitter-doesnt-always-fire-when-using-random-between-two-constants-lifetime
namespace Spewnity
{
    /// <summary>
    /// ParticleSystem support functions. Can self-destruct a ParticleSystem GameObject when the system
    /// has finished. Can give callbacks on lifecycle of system and individual particles.
    /// <para>
    /// Note that the particleBorn and particleDied events will create subemitters that are needed to
    /// detect particle births and deaths. These will show up as two GameObjects underneath this 
    /// component's GameObject, named subemitter-XXX. Also note that these emitters are only created
    /// if listeners have been registered by the time Start() is called.
    /// </para>
    /// </summary>
    public class ParticleMonitor : MonoBehaviour
    {
        [Header("Self Destruction")]
        [Tooltip("When checked, will destroy this GameObject when the attached ParticleSystem has finished its job")]
        public bool destroyWhenFinished = false;

        [Header("Events")]
        public ParticleSystemEvent systemStarted;
        public ParticleSystemEvent systemFinished;
        public ParticleEvent particleBorn;
        public ParticleEvent particleDied;

        private ParticleSystem ps;
        private ParticleSystem psBirthTester;
        private ParticleSystem psDeathTester;
        private bool alive = false;

        void Awake()
        {
            ps = gameObject.GetComponent<ParticleSystem>();
            if (ps == null)
                throw new UnityException("DestroyParticleSystem must be attached a GameObject with a ParticleSystem.");
        }

        void Start()
        {
            particleBorn.ThrowIfNull();
            if (particleBorn.GetPersistentEventCount() > 0)
                AddSubEmitter(ref psBirthTester, ParticleSystemSubEmitterType.Birth, (v) => { particleBorn.Invoke(v); });

            particleDied.ThrowIfNull();
            if (particleDied.GetPersistentEventCount() > 0)
                AddSubEmitter(ref psDeathTester, ParticleSystemSubEmitterType.Death, (v) => { particleDied.Invoke(v); });
        }

        private void AddSubEmitter(ref ParticleSystem subPS, ParticleSystemSubEmitterType type,
            System.Action<Vector3> callback)
        {
            // Create SubEmitter object
            GameObject go = new GameObject("subemitter-" + type.ToString());
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localEulerAngles = Vector3.zero;

            // Configure SubEmitter particle system
            subPS = go.AddComponent<ParticleSystem>();
            subPS.Stop();
            // go.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Particles/Additive"));
            ParticleSystem.MainModule main = subPS.main;
            main.duration = 10f;
            main.startLifetime = 10f;
            main.startSize = 0f;
            main.startSpeed = 0f;
            main.maxParticles = ps.main.maxParticles;
            main.loop = true;
            main.playOnAwake = true;
            ParticleSystem.EmissionModule emission = subPS.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 1, 1, 1, 0)
            });
            ParticleSystem.ShapeModule shape = subPS.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = Vector3.zero;
            subPS.Play();

            // Add SubEmitter callback
            SubEmitterHandler handler = go.AddComponent<SubEmitterHandler>();
            handler.callback = callback;

            // Attach subemitter to to main emitter
            ParticleSystem.SubEmittersModule subEmitters = ps.subEmitters;
            subEmitters.AddSubEmitter(subPS, type, ParticleSystemSubEmitterProperties.InheritNothing);
            subEmitters.enabled = true;
        }

        void Update()
        {
            if (alive)
            {
                if (!ps.IsAlive())
                {
                    systemFinished.Invoke(ps);
                    if (destroyWhenFinished)
                        Destroy(gameObject);
                    else alive = false;
                }
            }
            else
            {
                if (ps.IsAlive())
                {
                    systemStarted.Invoke(ps);
                    alive = true;
                }
            }
        }
    }

    [System.Serializable]
    public class ParticleEvent : UnityEvent<Vector3> { }

    [System.Serializable]
    public class ParticleSystemEvent : UnityEvent<ParticleSystem> { }
}

namespace Spewnity.ParticleMonitorInternal
{
    public class SubEmitterHandler : MonoBehaviour
    {
        public System.Action<Vector3> callback;
        private ParticleSystem ps;
        private ParticleSystem.Particle[] particles;

        void Awake()
        {
            ps = gameObject.GetComponent<ParticleSystem>();
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }

        void LateUpdate()
        {
            if (ps.particleCount > 0)
            {
                int count = ps.GetParticles(particles);
                Debug.Assert(count == ps.particleCount);
                for (int i = 0; i < count; i++)
                    callback(particles[i].position);
                ps.Clear();
            }
        }
    }
}