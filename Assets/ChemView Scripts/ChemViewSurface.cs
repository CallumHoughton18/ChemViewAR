using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChemViewSurface : MonoBehaviour {

    DetectedPlane TrackedPlane;
    MeshCollider MeshCollider;
    Mesh Mesh;
    List<Vector3> Points = new List<Vector3>();
    List<Vector3> PreviousFramePoints = new List<Vector3>();

    private void Awake()
    {
        MeshCollider = gameObject.AddComponent<MeshCollider>();
        Mesh = new Mesh();
        MeshCollider.sharedMesh = Mesh;

        Vector3 oneCentimeterUp = Vector3.up * 0.01f;
        transform.Translate(oneCentimeterUp, Space.Self);
    }

    public void SetTrackedPlane(DetectedPlane plane)
    {
        TrackedPlane = plane;
        Update();
    }

    void Update()
    {
        if (TrackedPlane == null)
        {
            return;
        }
        else if (TrackedPlane.SubsumedBy != null)
        {
            Destroy(gameObject);
            return;
        }
        else if (Session.Status != SessionStatus.Tracking)
        {
            MeshCollider.enabled = false;
            return;
        }

        MeshCollider.enabled = true;

        UpdateMeshIfNeeded();
    }

    void UpdateMeshIfNeeded()
    {
        TrackedPlane.GetBoundaryPolygon(Points);

        if (AreVertexListsEqual(PreviousFramePoints, Points))
        {
            return;
        }

        int[] indices = TriangulatorXZ.Triangulate(Points);

        Mesh.Clear();
        Mesh.SetVertices(Points);
        Mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        Mesh.RecalculateBounds();

        MeshCollider.sharedMesh = null;
        MeshCollider.sharedMesh = Mesh;
    }

    bool AreVertexListsEqual(List<Vector3> firstList, List<Vector3> secondList)
    {
        if (firstList.Count != secondList.Count)
        {
            return false;
        }

        for (int i = 0; i < firstList.Count; i++)
        {
            if (firstList[i] != secondList[i])
            {
                return false;
            }
        }

        return true;
    }
}
