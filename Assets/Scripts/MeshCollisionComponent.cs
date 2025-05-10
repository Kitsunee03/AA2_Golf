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

    private Vector3 center;
    private float boundingRadius;

    [Header("Mesh parameters")]
    [Tooltip("Coeficiente de restitución: 0.2 = inelástico, 0.8 = elástico")]
    [SerializeField] private float restitution = 0.2f;
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Grass;

    [SerializeField] private GameObject iceLayer;
    [SerializeField] private GameObject sandLayer;

    [Tooltip("if not hole, leave empty")]
    [SerializeField] private List<int> holeTriangles = new();

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        triangles = mesh.triangles;
        localVertices = mesh.vertices;

        worldVertices = new Vector3[localVertices.Length];
        Transform meshTransform = meshFilter.transform;
        for (int i = 0; i < localVertices.Length; i++) { worldVertices[i] = meshTransform.TransformPoint(localVertices[i]); }

        // center calculation
        center = Vector3.zero;
        for (int i = 0; i < worldVertices.Length; i++) { center += worldVertices[i]; }
        center /= worldVertices.Length;

        // farthest vertex from center
        boundingRadius = 0f;
        for (int i = 0; i < worldVertices.Length; i++)
        {
            float dist = Vector3.Distance(center, worldVertices[i]);
            if (dist > boundingRadius) { boundingRadius = dist; }
        }
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

        #if UNITY_EDITOR
        GUIStyle style = new();
        style.normal.textColor = Color.red;
        #endif

        for (int i = 0; i < triangles.Length; i += 3)
        {
            // draw triangles
            Gizmos.color = Color.cyan;
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

            #if UNITY_EDITOR
            // draw triangle number
            UnityEditor.Handles.Label(center, (i / 3).ToString(), style);
            #endif
        }
    }

    #region Accessors
    public SurfaceType Surface => surfaceType;
    public float Restitution => Mathf.Clamp01(restitution);
    public Vector3 Center => center;
    public float BoundingRadius => boundingRadius;
    public bool IsHoleTriangle(int p_triangleIndex) { return holeTriangles.Contains(p_triangleIndex); }
    #endregion
}