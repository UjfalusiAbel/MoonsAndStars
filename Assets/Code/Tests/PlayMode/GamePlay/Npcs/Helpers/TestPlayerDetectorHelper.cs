using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.PlayMode.GamePlay.Npcs.Helpers
{
    public static class TestPlayerDetectorHelper
    {
        public static void ConfigureForTesting(this PlayerDetector detector, float range = 500f, float angle = 360f, bool useLineOfSight = false)
        {
            var rangeField = typeof(PlayerDetector).GetField("_detectionRange", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (rangeField != null)
                rangeField.SetValue(detector, range);
            
            var angleField = typeof(PlayerDetector).GetField("_detectionAngle", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (angleField != null)
                angleField.SetValue(detector, angle);
            
            var losField = typeof(PlayerDetector).GetField("_useLineOfSight", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (losField != null)
                losField.SetValue(detector, useLineOfSight);
            
            var intervalField = typeof(PlayerDetector).GetField("_detectionInterval", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (intervalField != null)
                intervalField.SetValue(detector, 0.01f);
            
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                var layerField = typeof(PlayerDetector).GetField("_playerLayer", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (layerField != null)
                {
                    LayerMask layerMask = 1 << playerLayer;
                    layerField.SetValue(detector, layerMask);
                }
            }
            else
            {
                var layerField = typeof(PlayerDetector).GetField("_playerLayer", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (layerField != null)
                {
                    LayerMask allLayers = ~0;
                    layerField.SetValue(detector, allLayers);
                }
            }
            
            var obstacleField = typeof(PlayerDetector).GetField("_obstacleMask", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (obstacleField != null)
            {
                LayerMask noObstacles = 0;
                obstacleField.SetValue(detector, noObstacles);
            }
        }
    }
}