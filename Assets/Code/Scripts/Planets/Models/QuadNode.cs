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
        private MeshCollider m_collider;
        private QuadNode[] m_children;
        public Vector2 GetMinCoords => m_minCoords;
        public Vector2 GetMaxCoords => m_maxCoords;
        public int GetLevel => m_level;
        public MeshFilter GetMeshFilter => m_meshFilter;
        public MeshCollider GetMeshCollider => m_collider;
        public bool IsLeaf => m_children == null;
        public QuadNode[] GetChildren => m_children;
        public QuadNode(Vector2 minCoords, Vector2 maxCoords, int level, Transform meshParent, Material nodeMaterial)
        {
            m_minCoords = minCoords;
            m_maxCoords = maxCoords;
            m_level = level;
            m_meshObject = new GameObject("QuadNode");
            m_meshFilter = m_meshObject.AddComponent<MeshFilter>();
            var renderer = m_meshObject.AddComponent<MeshRenderer>();
            m_collider = m_meshObject.AddComponent<MeshCollider>();
            m_collider.convex = false;
            renderer.material = nodeMaterial;
            m_meshObject.transform.parent = meshParent;
            m_meshObject.transform.localPosition = Vector3.zero;
        }

        public void SetChildren(QuadNode[] children)
        {
            m_children = children;
            m_meshFilter.mesh = null;
        }

        public void DestroyChildren()
        {
            if (m_children == null)
            {
                return;
            }

            foreach (var child in m_children)
            {
                GameObject.Destroy(child.m_meshObject);
            }

            m_children = null;
        }
    }
}