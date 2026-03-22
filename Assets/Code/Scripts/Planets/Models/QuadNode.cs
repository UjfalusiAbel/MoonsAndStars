using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.Models
{
    public class QuadNode
    {
        private Vector2 m_minCoords;
        private Vector2 m_maxCoords;
        private int m_level;
        private GameObject m_meshObject;
        private MeshFilter m_meshFilter;
        private QuadNode[] m_children;
        public Vector2 GetMinCoords => m_minCoords;
        public Vector2 GetMaxCoords => m_maxCoords;
        public int GetLevel => m_level;
        public MeshFilter GetMeshFilter => m_meshFilter;
        public bool IsLeaf => m_children == null;
        public QuadNode(Vector2 minCoords, Vector2 maxCoords, int level, Transform meshParent)
        {
            m_minCoords = minCoords;
            m_maxCoords = maxCoords;
            m_level = level;
            m_meshObject = new GameObject("QuadNode");
            m_meshFilter = m_meshObject.AddComponent<MeshFilter>();
            m_meshObject.AddComponent<MeshRenderer>();
        }
    }
}