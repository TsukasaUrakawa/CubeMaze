using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CourseMeshGenerator : MonoBehaviour
{
    [Header("Spline")]
    [SerializeField] private SplineContainer _courseSpline;

    [Header("Geometory")]
    [SerializeField] private MeshFilter _floorMeshFilter;

    [Header("Course Settings")]
    [SerializeField] private float _floorWidth = 3.0f;

    //Splineの分割数を指定する変数
    [SerializeField] private int resolution = 20;
    
    private void Start()
    {
        GenerateFloor();
    }
    [ContextMenu("Generate Floor")]
    private void GenerateFloor()
    {
        if (_courseSpline == null || _floorMeshFilter == null)
        {
            Debug.LogError("CourseMeshGenerator: 必要な参照が設定されていません");
            return;
        }

        List<Vector3> vertices = new();
        List<int> triangles = new();

        float halfWidth = _floorWidth * 0.5f;

        for(int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;

            // Splineの座標
            Vector3 position = _courseSpline.EvaluatePosition(t);

            // Splineの進行方向
            Vector3 tangent = _courseSpline.EvaluateTangent(t);
            tangent.Normalize();

            // Splineの上方向
            Vector3 up = _courseSpline.EvaluateUpVector(t);
            up.Normalize();

            // Splineの進行方向から見て右方向
            Vector3 right = Vector3.Cross(up, tangent).normalized;

            Vector3 leftPosition = position - right * halfWidth;
            Vector3 rightPosition = position + right * halfWidth;

            // ワールド座標からローカル座標に変換
            leftPosition = _floorMeshFilter.transform.InverseTransformPoint(leftPosition);
            rightPosition = _floorMeshFilter.transform.InverseTransformPoint(rightPosition);

            vertices.Add(leftPosition);
            vertices.Add(rightPosition);
        }

        for (int i = 0; i < resolution; i++)
        {
            int index = i * 2;

            // 2つの三角形を作成して四角形を形成
            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 1);

            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        Mesh mesh = new Mesh();
        mesh.name = "GeneratedFloor";

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        _floorMeshFilter.sharedMesh = mesh;
    }
}
