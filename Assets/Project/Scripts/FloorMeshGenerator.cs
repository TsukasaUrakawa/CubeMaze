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
            Debug.LogError("FloorMeshGenerator: 必要な参照が設定されていません");
            return;
        }

        List<Vector3> topVertices = new();
        List<Vector3> bottomVertices = new();
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> topUvs = new();
        List<Vector2> bottomUvs = new();
        List<Vector2> uvs = new();

        float halfFloorWidth = _floorWidth * 0.5f;
        float splineLength = _courseSpline.CalculateLength();

        for (int i = 0; i <= _resolution; i++)
        {
            float t = (float)i / _resolution;

            // Spline上の位置
            Vector3 position = _courseSpline.EvaluatePosition(t);

            // Splineの進行方向
            Vector3 tangent = _courseSpline.EvaluateTangent(t);
            tangent.Normalize();

            // Splineの上方向
            Vector3 up = _courseSpline.EvaluateUpVector(t);
            up.Normalize();

            // Splineの右方向
            Vector3 right = Vector3.Cross(up, tangent).normalized;

            // 床上面の左右
            Vector3 leftTop = position - right * halfFloorWidth;
            Vector3 rightTop = position + right * halfFloorWidth;

            //床下面の左右
            Vector3 leftBottom = leftTop - up * _floorThickness;
            Vector3 rightBottom = rightTop - up * _floorThickness;

            // ローカル座標に変換
            leftTop = _floorMeshFilter.transform.InverseTransformPoint(leftTop);
            rightTop = _floorMeshFilter.transform.InverseTransformPoint(rightTop);
            leftBottom = _floorMeshFilter.transform.InverseTransformPoint(leftBottom);
            rightBottom = _floorMeshFilter.transform.InverseTransformPoint(rightBottom);

            // 頂点追加
            topVertices.Add(leftTop);
            topVertices.Add(rightTop);

            bottomVertices.Add(leftBottom);
            bottomVertices.Add(rightBottom);

            // UV
            float v = t * splineLength;

            topUvs.Add(new Vector2(0.0f, v));
            topUvs.Add(new Vector2(_floorWidth, v));

            bottomUvs.Add(new Vector2(0.0f, v));
            bottomUvs.Add(new Vector2(_floorWidth, v));
        }

        int topStartIndex = vertices.Count;
        vertices.AddRange(topVertices);
        uvs.AddRange(topUvs);

        int bottomStartIndex = vertices.Count;
        vertices.AddRange(bottomVertices);
        uvs.AddRange(bottomUvs);

        //上面の三角形
        for (int i = 0; i < _resolution; i++)
        {
            int index = topStartIndex + i * 2;

            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 1);

            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        //下面の三角形
        for (int i = 0; i < _resolution; i++)
        {
            int index = bottomStartIndex + i * 2;

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            triangles.Add(index + 1);
            triangles.Add(index + 3);
            triangles.Add(index + 2);
        }

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
