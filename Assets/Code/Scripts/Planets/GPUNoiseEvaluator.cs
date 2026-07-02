using System.Collections.Generic;
using UnityEngine;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects;

namespace MoonsAndStars.Assets.Code.Scripts.Planets
{
    public class GPUNoiseEvaluator
    {
        private ComputeShader _computeShader;
        private ComputeBuffer _configBuffer;
        private ComputeBuffer _resultBuffer;
        
        private struct NoiseConfig
        {
            public Vector3 pos;
            public int noiseType;
            public int octaves;
            public float persistence;
            public float lacunarity;
            public float strength;
            public float ridgeOffset;
            public float ridgeSharpness;
            public float scale;
            public int enabled;
        }
        
        private struct NoiseOutput
        {
            public float val;
        }
        
        public GPUNoiseEvaluator()
        {
            _computeShader = Resources.Load<ComputeShader>("GPUNoise");
        }
        
        public bool IsAvailable()
        {
            return _computeShader != null && SystemInfo.supportsComputeShaders;
        }
        
        public float[] EvaluateBatch(Vector3[] points, List<NoiseConfiguration> configurations)
        {
            if (!IsAvailable() || configurations == null || configurations.Count == 0)
            {
                return EvaluateBatchCPU(points, configurations);
            }
            
            int total = points.Length * configurations.Count;
            NoiseConfig[] configs = new NoiseConfig[total];
            
            int idx = 0;
            for (int i = 0; i < points.Length; i++)
            {
                foreach (var cfg in configurations)
                {
                    if (cfg == null || cfg.filter == null || !cfg.details.isEnabled)
                    {
                        configs[idx] = new NoiseConfig { enabled = 0 };
                        idx++;
                        continue;
                    }
                    
                    configs[idx] = new NoiseConfig
                    {
                        pos = points[i],
                        noiseType = GetNoiseType(cfg.filter),
                        octaves = cfg.details.numberOfLayers,
                        persistence = cfg.details.persistance,
                        lacunarity = cfg.details.roughness,
                        strength = cfg.details.strength,
                        ridgeOffset = GetRidgeOffset(cfg.filter),
                        ridgeSharpness = GetRidgeSharpness(cfg.filter),
                        scale = cfg.details.baseRoughness,
                        enabled = 1
                    };
                    idx++;
                }
            }
            
            int stride = sizeof(float) * 3 + sizeof(int) * 3 + sizeof(float) * 8;
            _configBuffer = new ComputeBuffer(total, stride);
            _resultBuffer = new ComputeBuffer(total, sizeof(float));
            
            _configBuffer.SetData(configs);
            _resultBuffer.SetData(new NoiseOutput[total]);
            
            int kernel = _computeShader.FindKernel("EvaluateNoiseBatch");
            _computeShader.SetBuffer(kernel, "_Configs", _configBuffer);
            _computeShader.SetBuffer(kernel, "_Results", _resultBuffer);
            _computeShader.SetInt("_BatchCount", total);
            
            int groups = Mathf.CeilToInt(total / 64.0f);
            _computeShader.Dispatch(kernel, groups, 1, 1);
            
            NoiseOutput[] results = new NoiseOutput[total];
            _resultBuffer.GetData(results);
            
            _configBuffer.Release();
            _resultBuffer.Release();
            
            float[] heights = new float[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                float sum = 0;
                for (int j = 0; j < configurations.Count; j++)
                {
                    sum += results[i * configurations.Count + j].val;
                }
                heights[i] = sum;
            }
            
            return heights;
        }
        
        private float[] EvaluateBatchCPU(Vector3[] points, List<NoiseConfiguration> configs)
        {
            float[] heights = new float[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                float sum = 0;
                foreach (var c in configs)
                {
                    if (c != null && c.filter != null && c.details.isEnabled)
                    {
                        sum += c.filter.EvaluatePoint(points[i], c.details);
                    }
                }
                heights[i] = sum;
            }
            return heights;
        }
        
        private int GetNoiseType(NoiseFilter f)
        {
            string n = f.GetType().Name;
            if (n.Contains("Simple")) return 0;
            if (n.Contains("Ridged")) return 2;
            return 1; // Default to FBM
        }
        
        private float GetRidgeOffset(NoiseFilter f)
        {
            if (f is RidgedNoiseFilter r) return r.ridgeOffset;
            return 1f;
        }
        
        private float GetRidgeSharpness(NoiseFilter f)
        {
            if (f is RidgedNoiseFilter r) return r.ridgeSharpness;
            return 1f;
        }
    }
}