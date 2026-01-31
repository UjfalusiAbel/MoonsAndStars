using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets
{
    public class LodCalculator : MonoBehaviour
    {
        [SerializeField] private List<float> m_recalculateDistances;
        [SerializeField] private float m_radius;
        [SerializeField] private float m_updateTime;
        List<PlanetMeshGenerator> m_consideredGenerators = new List<PlanetMeshGenerator>();
        private Coroutine m_planetVisibilityCheckerRoutine;
        private Coroutine m_planetLodCheckerRoutine;

        public void Start()
        {
            m_planetVisibilityCheckerRoutine = StartCoroutine(CheckVisiblePlanets());
            m_planetLodCheckerRoutine = StartCoroutine(CheckLodOnPlanets());
        }

        public void OnDestroy()
        {
            if (m_planetVisibilityCheckerRoutine != null)
            {
                StopCoroutine(m_planetVisibilityCheckerRoutine);
                StopCoroutine(m_planetLodCheckerRoutine);
            }
        }

        private IEnumerator CheckVisiblePlanets()
        {
            while (Application.isPlaying)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, m_radius);
                foreach (Collider hit in hits)
                {
                    if (hit.gameObject.tag == "Planet" && hit.TryGetComponent<PlanetMeshGenerator>(out var generator))
                    {
                        m_consideredGenerators.Add(generator);
                    }
                }

                yield return new WaitForSeconds(m_updateTime);
            }
        }

        private IEnumerator CheckLodOnPlanets()
        {
            while (Application.isPlaying)
            {
                foreach (var generator in m_consideredGenerators)
                {
                    int n = m_recalculateDistances.Count;
                    bool wasDistanceSet = false;
                    for (int i = 0; i < n; i++)
                    {
                        if (Vector3.Distance(transform.position, generator.transform.position) < m_recalculateDistances[n - i - 1])
                        {
                            generator.SetRecalculateDistance = m_recalculateDistances[n - i - 1];
                            wasDistanceSet = true;
                        }

                        if (wasDistanceSet)
                        {
                            break;
                        }
                        else
                        {
                            generator.SetRecalculateDistance = m_radius;
                        }
                    }
                }

                yield return new WaitForSeconds(m_updateTime);
            }
        }
    }
}