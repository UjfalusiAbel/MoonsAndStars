using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Player
{
    public class FrustumCuller : MonoBehaviour
    {
        [SerializeField] private Camera m_targetCamera;
        [SerializeField] private bool m_enableFrustumCulling = true;
        [SerializeField] private float m_cullingCheckInterval = 0.05f;

        private Plane[] m_frustumPlanes;
        private float m_timer;
        private Vector3 m_lastCameraPosition;
        private Quaternion m_lastCameraRotation;

        public Camera TargetCamera => m_targetCamera;
        public bool IsFrustumCullingEnabled => m_enableFrustumCulling;
        public Plane[] FrustumPlanes => m_frustumPlanes;

        private void Start()
        {
            if (m_targetCamera == null)
            {
                m_targetCamera = Camera.main;
            }

            m_frustumPlanes = new Plane[6];
            UpdateFrustumPlanes();
        }

        private void Update()
        {
            if (!m_enableFrustumCulling)
            {
                return;
            }

            if (m_targetCamera == null) return;

            bool cameraMoved = m_lastCameraPosition != m_targetCamera.transform.position ||
                              m_lastCameraRotation != m_targetCamera.transform.rotation;

            m_timer += Time.deltaTime;

            if (cameraMoved || m_timer >= m_cullingCheckInterval)
            {
                UpdateFrustumPlanes();
                m_timer = 0f;
                m_lastCameraPosition = m_targetCamera.transform.position;
                m_lastCameraRotation = m_targetCamera.transform.rotation;
            }
        }

        private void UpdateFrustumPlanes()
        {
            if (m_targetCamera == null)
            {
                return;
            }

            GeometryUtility.CalculateFrustumPlanes(m_targetCamera, m_frustumPlanes);
        }

        public bool IsSphereInFrustum(Vector3 center, float radius)
        {
            if (!m_enableFrustumCulling || m_frustumPlanes == null)
            {
                return true;
            }

            for (int i = 0; i < 6; i++)
            {
                float distanceToPlane = m_frustumPlanes[i].GetDistanceToPoint(center);
                if (distanceToPlane < -radius)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsBoundsInFrustum(Bounds bounds)
        {
            if (!m_enableFrustumCulling || m_frustumPlanes == null)
            {
                return true;
            }

            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            for (int i = 0; i < 6; i++)
            {
                Plane plane = m_frustumPlanes[i];
                Vector3 normal = plane.normal;
                float distance = plane.distance;

                float r = extents.x * Mathf.Abs(normal.x) +
                          extents.y * Mathf.Abs(normal.y) +
                          extents.z * Mathf.Abs(normal.z);

                float dot = Vector3.Dot(normal, center);
                if (dot + r < -distance)
                {
                    return false;
                }
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (m_targetCamera == null) return;

            Gizmos.color = Color.yellow;
            Vector3[] nearCorners = new Vector3[4];
            Vector3[] farCorners = new Vector3[4];

            m_targetCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), m_targetCamera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearCorners);
            m_targetCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), m_targetCamera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, farCorners);

            for (int i = 0; i < 4; i++)
            {
                nearCorners[i] = m_targetCamera.transform.TransformVector(nearCorners[i]);
                farCorners[i] = m_targetCamera.transform.TransformVector(farCorners[i]);

                int next = (i + 1) % 4;
                Gizmos.DrawLine(nearCorners[i], nearCorners[next]);
                Gizmos.DrawLine(farCorners[i], farCorners[next]);
                Gizmos.DrawLine(nearCorners[i], farCorners[i]);
            }
        }
    }
}