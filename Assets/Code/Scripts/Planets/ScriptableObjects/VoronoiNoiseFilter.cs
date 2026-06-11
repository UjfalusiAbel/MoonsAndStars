using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Planets/NoiseFilters/Voronoi Noise Filter")]
    public class VoronoiNoiseFilter : NoiseFilter
    {
        [Range(1, 16)]
        public int numPoints = 8;

        [Range(0f, 2f)]
        public float cellSize = 1f;

        [Range(0f, 1f)]
        public float distancePower = 1f;

        public enum VoronoiReturnType
        {
            F1,
            F2,
            F2MinusF1,
            F1TimesF2
        }

        public VoronoiReturnType returnType = VoronoiReturnType.F1;

        public override float EvaluatePoint(Vector3 point, NoiseDetails details)
        {
            Vector3 scaledPoint = point * cellSize;

            float minDist1 = float.MaxValue;
            float minDist2 = float.MaxValue;

            int cellX = Mathf.FloorToInt(scaledPoint.x);
            int cellY = Mathf.FloorToInt(scaledPoint.y);
            int cellZ = Mathf.FloorToInt(scaledPoint.z);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        int checkX = cellX + x;
                        int checkY = cellY + y;
                        int checkZ = cellZ + z;

                        Vector3 pointPos = new Vector3(checkX, checkY, checkZ);

                        int hash = GetHash(checkX, checkY, checkZ);
                        Vector3 featurePoint = pointPos + new Vector3(
                            (hash & 0xFF) / 255f,
                            ((hash >> 8) & 0xFF) / 255f,
                            ((hash >> 16) & 0xFF) / 255f
                        ) - Vector3.one * 0.5f;

                        Vector3 diff = featurePoint - scaledPoint;
                        float dist = diff.magnitude;

                        if (dist < minDist1)
                        {
                            minDist2 = minDist1;
                            minDist1 = dist;
                        }
                        else if (dist < minDist2)
                        {
                            minDist2 = dist;
                        }
                    }
                }
            }

            float result;
            switch (returnType)
            {
                case VoronoiReturnType.F2:
                    result = minDist2;
                    break;
                case VoronoiReturnType.F2MinusF1:
                    result = minDist2 - minDist1;
                    break;
                case VoronoiReturnType.F1TimesF2:
                    result = minDist1 * minDist2;
                    break;
                default:
                    result = minDist1;
                    break;
            }

            result = Mathf.Pow(result, distancePower);
            result = Mathf.Clamp01(result * 2f - 1f);

            return result * details.strength;
        }

        private int GetHash(int x, int y, int z)
        {
            int hash = (x * 73856093) ^ (y * 19349663) ^ (z * 83492791);
            return Mathf.Abs(hash);
        }
    }
}