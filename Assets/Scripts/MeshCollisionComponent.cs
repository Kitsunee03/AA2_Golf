using System.Collections.Generic;
using UnityEngine;

public enum SurfaceType { Grass, Ice, Sand }

[RequireComponent(typeof(MeshFilter))]
public class MeshCollisionComponent : MonoBehaviour
{
    private Vector3[] worldVertices;
    private int[] triangles;
    private Vector3[] localVertices;
    private MeshFilter meshFilter;

    [Header("Mesh parameters")]
    [Tooltip("Coeficiente de restitución: 0.2 = inelástico, 0.8 = elástico")]
    [SerializeField] private float restitution = 0.2f;
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Grass;

    [SerializeField] private GameObject iceLayer;
    [SerializeField] private GameObject sandLayer;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        triangles = mesh.triangles;
        localVertices = mesh.vertices;

        worldVertices = new Vector3[localVertices.Length];
        Transform meshTransform = meshFilter.transform;
        for (int i = 0; i < localVertices.Length; i++) { worldVertices[i] = meshTransform.TransformPoint(localVertices[i]); }
    }

    private void Start() { PhysicsManager.Instance.RegisterMeshCollider(this); }
    private void OnValidate() { UpdateSurfaceLayers(); }

    private void UpdateSurfaceLayers()
    {
        if (iceLayer != null) { iceLayer.SetActive(surfaceType == SurfaceType.Ice); }
        if (sandLayer != null) { sandLayer.SetActive(surfaceType == SurfaceType.Sand); }
    }

    public IEnumerable<(Vector3 a, Vector3 b, Vector3 c)> GetWorldTriangles()
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            yield return (
                worldVertices[triangles[i]],
                worldVertices[triangles[i + 1]],
                worldVertices[triangles[i + 2]]
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) { return; } // only in play mode
        if (worldVertices == null || triangles == null) { return; }

        Gizmos.color = Color.cyan;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            // draw triangles
            Vector3 a = worldVertices[triangles[i]];
            Vector3 b = worldVertices[triangles[i + 1]];
            Vector3 c = worldVertices[triangles[i + 2]];

            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, a);

            // draw triangle normal
            Vector3 center = (a + b + c) / 3f;
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(center, normal * 0.3f);

            Gizmos.color = Color.cyan;
        }
    }

    public SurfaceType Surface => surfaceType;
    public float Restitution => Mathf.Clamp01(restitution);
}