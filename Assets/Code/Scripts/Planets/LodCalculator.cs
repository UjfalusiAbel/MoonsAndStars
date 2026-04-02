using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets
{
    public class LodCalculator : MonoBehaviour
    {
        [SerializeField] private List<float> m_recalculateDistances;
        [SerializeField] private float m_radius = 1000f;
        [SerializeField] private float m_updateTime = 0.2f;

        private List<PlanetMeshGenerator> m_consideredGenerators = new List<PlanetMeshGenerator>();

        private void Start()
        {
            StartCoroutine(CheckVisiblePlanets());
            StartCoroutine(CheckLodOnPlanets());
        }

        private IEnumerator CheckVisiblePlanets()
        {
            while (true)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, m_radius);

                m_consideredGenerators.Clear();

                foreach (var hit in hits)
                {
                    if (hit.transform.parent.CompareTag("Planet"))
                    {
                        PlanetMeshGenerator generator = hit.GetComponentInParent<PlanetMeshGenerator>();
                        if (generator != null && !m_consideredGenerators.Contains(generator))
                        {
                            m_consideredGenerators.Add(generator);
                        }
                    }
                }

                yield return new WaitForSeconds(m_updateTime);
            }
        }

        private IEnumerator CheckLodOnPlanets()
        {
            while (true)
            {
                foreach (var generator in m_consideredGenerators)
                {
                    float selectedDistance = m_radius;

                    foreach (var dist in m_recalculateDistances)
                    {
                        if (Vector3.Distance(transform.position, generator.transform.position) < dist)
                        {
                            selectedDistance = dist;
                            break;
                        }
                    }

                    generator.SetRecalculateDistance = selectedDistance;
                    generator.UpdateLod();
                }

                yield return new WaitForSeconds(m_updateTime);
            }
        }
    }
}