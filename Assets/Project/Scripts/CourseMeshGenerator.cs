using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CourseMeshGenerator : MonoBehaviour
{
    [Header("Spline")]
    [SerializeField] private SplineContainer _courseSpline;
    public SplineContainer CourseSpline => _courseSpline;

    [Header("Geometry")]
    [SerializeField] private MeshFilter _floorMeshFilter;
    [SerializeField] private MeshFilter _leftWallMeshFilter;
    [SerializeField] private MeshFilter _rightWallMeshFilter;

    [Header("Course Settings")]
    [SerializeField] private float _floorWidth = 3.0f;
    [SerializeField] private float _wallHeight = 1.0f;

    //Splineの分割数を指定する変数
    [SerializeField] private int resolution = 20;
    
    [ContextMenu("Generate Course")]
    public void GenerateCourse()
    {
        GenerateFloor();
        GenerateWalls();
    }
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

    private void GenerateWalls()
    {
        if (_courseSpline == null || _leftWallMeshFilter == null || _rightWallMeshFilter == null)
        {
            Debug.LogError("CourseMeshGenerator: 壁生成に必要な参照が設定されていません");
            return;
        }

        List<Vector3> leftVertices = new();
        List<int> leftTriangles = new();

        List<Vector3> rightVertices= new();
        List<int> rightTriangles = new();

        float halfWidth = _floorWidth * 0.5f;

        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;

            Vector3 position = _courseSpline.EvaluatePosition(t);

            Vector3 tangent = _courseSpline.EvaluateTangent(t);
            tangent.Normalize();

            Vector3 up = _courseSpline.EvaluateUpVector(t);
            up.Normalize();

            Vector3 right = Vector3.Cross(up, tangent).normalized;

            Vector3 leftBottom = position - right * halfWidth;
            Vector3 leftTop = leftBottom + up * _wallHeight;

            Vector3 rightBottom = position + right * halfWidth;
            Vector3 rightTop = rightBottom + up * _wallHeight;

            leftBottom = _leftWallMeshFilter.transform.InverseTransformPoint(leftBottom);
            leftTop = _leftWallMeshFilter.transform.InverseTransformPoint(leftTop);

            rightBottom = _rightWallMeshFilter.transform.InverseTransformPoint(rightBottom);
            rightTop = _rightWallMeshFilter.transform.InverseTransformPoint(rightTop);

            leftVertices.Add(leftBottom);
            leftVertices.Add(leftTop);

            rightVertices.Add(rightBottom);
            rightVertices.Add(rightTop);
        }

        for (int j = 0; j < resolution; j++)
        {
            int index = j * 2;
            // 左壁の三角形
            leftTriangles.Add(index);
            leftTriangles.Add(index + 2);
            leftTriangles.Add(index + 1);

            leftTriangles.Add(index + 1);
            leftTriangles.Add(index + 2);
            leftTriangles.Add(index + 3);

            // 右壁の三角形
            rightTriangles.Add(index);
            rightTriangles.Add(index + 1);
            rightTriangles.Add(index + 2);

            rightTriangles.Add(index + 1);
            rightTriangles.Add(index + 3);
            rightTriangles.Add(index + 2);
        }

        Mesh leftMesh = new Mesh();
        leftMesh.name = "GeneratedLeftWall";
        leftMesh.SetVertices(leftVertices);
        leftMesh.SetTriangles(leftTriangles, 0);
        leftMesh.RecalculateNormals();
        leftMesh.RecalculateBounds();

        _leftWallMeshFilter.sharedMesh = leftMesh;

        Mesh rightMesh = new Mesh();
        rightMesh.name = "GeneratedRightWall";
        rightMesh.SetVertices(rightVertices);
        rightMesh.SetTriangles(rightTriangles, 0);
        rightMesh.RecalculateNormals();
        rightMesh.RecalculateBounds();

        _rightWallMeshFilter.sharedMesh = rightMesh;
    }
}
