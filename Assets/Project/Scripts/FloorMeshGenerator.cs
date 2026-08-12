using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class FloorMeshGenerator
{
    private readonly SplineContainer _courseSpline;
    private readonly MeshFilter _floorMeshFilter;
    private readonly MeshCollider _floorMeshCollider;

    private readonly float _floorWidth;
    private readonly float _floorThickness;
    private readonly int _resolution;

    public FloorMeshGenerator(SplineContainer courseSpline, MeshFilter floorMeshFilter, MeshCollider floorMeshCollider, float floorWidth, float floorThickness, int resolution)
    {
        _courseSpline = courseSpline;
        _floorMeshFilter = floorMeshFilter;
        _floorMeshCollider = floorMeshCollider;
        _floorWidth = floorWidth;
        _floorThickness = floorThickness;
        _resolution = resolution;
    }

    public void Generate()
    {
        if (_courseSpline == null || _floorMeshFilter == null || _floorMeshCollider == null)
        {
            Debug.LogError("CourseMeshGenerator: 必要な参照が設定されていません");
            return;
        }

        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        float halfFloorWidth = _floorWidth * 0.5f;
        float splineLength = _courseSpline.CalculateLength();

        int topStartIndex = vertices.Count;

        for (int i = 0; i <= _resolution; i++)
        {
            float t = (float)i / _resolution;

            //Spline上の位置
            Vector3 position = _courseSpline.EvaluatePosition(t);

            //Splineの進行方向
            Vector3 tangent = _courseSpline.EvaluateTangent(t);
            tangent.Normalize();

            //Splineの上方向
            Vector3 up = _courseSpline.EvaluateUpVector(t);
            up.Normalize();

            //Splineの右方向
            Vector3 right = Vector3.Cross(up, tangent).normalized;

            //床上面の左右
            Vector3 leftTop = position - right * halfFloorWidth;
            Vector3 rightTop = position + right * halfFloorWidth;

            //ローカル座標に変換
            leftTop = _floorMeshFilter.transform.InverseTransformPoint(leftTop);
            rightTop = _floorMeshFilter.transform.InverseTransformPoint(rightTop);
        }

        for (int i = 0; i < _resolution; i++)
        {
            int index = i * 4;

            // 上面の四角形
            triangles.Add(index);
            triangles.Add(index + 4);
            triangles.Add(index + 1);

            triangles.Add(index + 1);
            triangles.Add(index + 4);
            triangles.Add(index + 5);

            // 下面の四角形
            triangles.Add(index + 2);
            triangles.Add(index + 3);
            triangles.Add(index + 6);

            triangles.Add(index + 3);
            triangles.Add(index + 7);
            triangles.Add(index + 6);

            // 側面の四角形（左側）
            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 4);

            triangles.Add(index + 2);
            triangles.Add(index + 6);
            triangles.Add(index + 4);

            // 側面の四角形（右側）
            triangles.Add(index + 1);
            triangles.Add(index + 5);
            triangles.Add(index + 3);

            triangles.Add(index + 3);
            triangles.Add(index + 5);
            triangles.Add(index + 7);
        }

        // 側面の四角形（前側）
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);

        triangles.Add(1);
        triangles.Add(3);
        triangles.Add(2);

        // 側面の四角形（後側）
        int lastIndex = _resolution * 4;

        triangles.Add(lastIndex);
        triangles.Add(lastIndex + 2);
        triangles.Add(lastIndex + 1);

        triangles.Add(lastIndex + 1);
        triangles.Add(lastIndex + 2);
        triangles.Add(lastIndex + 3);

        Mesh mesh = new Mesh();
        mesh.name = "GeneratedFloor";

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        _floorMeshFilter.sharedMesh = mesh;

        _floorMeshCollider.sharedMesh = null;
        _floorMeshCollider.sharedMesh = mesh;
    }
}
