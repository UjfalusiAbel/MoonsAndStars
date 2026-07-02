using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets
{
    public class GravityWell : MonoBehaviour
    {
        [Header("Gravity Settings")]
        [SerializeField] private float m_gravityStrength = 9.85f;
        [SerializeField] private float m_maxGravityRadius = 500f;
        [SerializeField] private AnimationCurve m_gravityFalloff = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] private bool m_useRealisticGravity = true;
        
        [Header("Debug")]
        [SerializeField] private bool m_showGizmos = true;
        [SerializeField] private Color m_gizmoColor = new Color(0, 1, 0, 0.3f);
        
        private float m_planetRadius;
        private Transform m_planetTransform;
        
        public float GravityStrength => m_gravityStrength;
        public float MaxGravityRadius => m_maxGravityRadius;
        public float PlanetRadius => m_planetRadius;
        public Transform PlanetTransform => m_planetTransform;
        
        private void Start()
        {
            m_planetTransform = transform;
            
            var generator = GetComponent<PlanetMeshGenerator>();
            if (generator != null && generator.GetMeshData != null)
            {
                m_planetRadius = generator.GetMeshData.PlanetSize;
            }
            else
            {
                m_planetRadius = transform.localScale.x * 5f;
            }
            
            // Ensure max radius is at least planet radius
            if (m_maxGravityRadius < m_planetRadius * 2)
            {
                m_maxGravityRadius = m_planetRadius * 3;
            }
        }
        
        public Vector3 GetGravityForce(Vector3 position, Vector3 velocity)
        {
            Vector3 directionToCenter = (m_planetTransform.position - position);
            float distance = directionToCenter.magnitude;
            
            // Outside gravity radius
            if (distance > m_maxGravityRadius)
            {
                return Vector3.zero;
            }
            
            // Inside planet - push out
            if (distance < m_planetRadius)
            {
                return directionToCenter.normalized * m_gravityStrength * 2f;
            }
            
            float t = (distance - m_planetRadius) / (m_maxGravityRadius - m_planetRadius);
            float falloff = m_gravityFalloff.Evaluate(t);
            
            float gravityMagnitude;
            if (m_useRealisticGravity)
            {
                // Inverse square law
                gravityMagnitude = m_gravityStrength / (distance * distance) * (m_planetRadius * m_planetRadius);
            }
            else
            {
                gravityMagnitude = m_gravityStrength * falloff;
            }
            
            return directionToCenter.normalized * gravityMagnitude;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!m_showGizmos) return;
            
            Gizmos.color = m_gizmoColor;
            Gizmos.DrawWireSphere(transform.position, m_maxGravityRadius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_planetRadius);
        }
    }
}