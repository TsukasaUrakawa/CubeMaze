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

        // 各部分における頂点のList
        List<Vector3> topVertices = new();
        List<Vector3> bottomVertices = new();
        List<Vector3> leftSideVertices = new();
        List<Vector3> rightSideVertices = new();
        List<Vector3> vertices = new();

        List<int> triangles = new(); // 面を作るのに必要な三角形

        // 各部分におけるUVのList
        List<Vector2> topUvs = new();
        List<Vector2> bottomUvs = new();
        List<Vector2> leftSideUvs = new();
        List<Vector2> rightSideUvs = new();
        List<Vector2> uvs = new();

        float halfFloorWidth = _floorWidth * 0.5f;
        float splineLength = _courseSpline.CalculateLength();

        // 前面用の頂点
        Vector3 frontLeftTop = Vector3.zero;
        Vector3 frontRightTop = Vector3.zero;
        Vector3 frontLeftBottom = Vector3.zero;
        Vector3 frontRightBottom = Vector3.zero;

        // 後面用の頂点
        Vector3 backLeftTop = Vector3.zero;
        Vector3 backRightTop = Vector3.zero;
        Vector3 backLeftBottom = Vector3.zero;
        Vector3 backRightBottom = Vector3.zero;

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

            // Splineの右方向(進行方向から見て右)
            Vector3 right = Vector3.Cross(up, tangent).normalized;

            // 床上面の左右
            Vector3 leftTop = position - right * halfFloorWidth;
            Vector3 rightTop = position + right * halfFloorWidth;

            //床下面の左右
            Vector3 leftBottom = leftTop - up * _floorThickness;
            Vector3 rightBottom = rightTop - up * _floorThickness;

            // MeshFilter基準のローカル座標に変換
            leftTop = _floorMeshFilter.transform.InverseTransformPoint(leftTop);
            rightTop = _floorMeshFilter.transform.InverseTransformPoint(rightTop);
            leftBottom = _floorMeshFilter.transform.InverseTransformPoint(leftBottom);
            rightBottom = _floorMeshFilter.transform.InverseTransformPoint(rightBottom);

            // 前面の頂点
            if (i == 0)
            {
                frontLeftTop = leftTop;
                frontRightTop = rightTop;
                frontLeftBottom = leftBottom;
                frontRightBottom = rightBottom;
            }

            // 後面の頂点
            if (i == _resolution)
            {
                backLeftTop = leftTop;
                backRightTop = rightTop;
                backLeftBottom = leftBottom;
                backRightBottom = rightBottom;
            }

            // 各面専用の頂点Listに追加
            topVertices.Add(leftTop);
            topVertices.Add(rightTop);

            bottomVertices.Add(leftBottom);
            bottomVertices.Add(rightBottom);

            leftSideVertices.Add(leftTop);
            leftSideVertices.Add(leftBottom);

            rightSideVertices.Add(rightTop);
            rightSideVertices.Add(rightBottom);

            // UV
            float v = t * splineLength;

            topUvs.Add(new Vector2(0.0f, v));
            topUvs.Add(new Vector2(_floorWidth, v));

            bottomUvs.Add(new Vector2(0.0f, v));
            bottomUvs.Add(new Vector2(_floorWidth, v));

            leftSideUvs.Add(new Vector2(v, _floorThickness));
            leftSideUvs.Add(new Vector2(v, 0.0f));

            rightSideUvs.Add(new Vector2(v, _floorThickness));
            rightSideUvs.Add(new Vector2(v, 0.0f));
        }

        // 各部分のListに保存した頂点、UVをそれぞれ1つのListに保存
        int topStartIndex = vertices.Count;
        vertices.AddRange(topVertices);
        uvs.AddRange(topUvs);

        int bottomStartIndex = vertices.Count;
        vertices.AddRange(bottomVertices);
        uvs.AddRange(bottomUvs);

        int leftSideStartIndex = vertices.Count;
        vertices.AddRange(leftSideVertices);
        uvs.AddRange(leftSideUvs);

        int rightSideStartIndex = vertices.Count;
        vertices.AddRange(rightSideVertices);
        uvs.AddRange(rightSideUvs);

        int frontStartIndex = vertices.Count;
        vertices.Add(frontLeftTop);
        vertices.Add(frontRightTop);
        vertices.Add(frontLeftBottom);
        vertices.Add(frontRightBottom);
        uvs.Add(new Vector2(0.0f, _floorThickness));
        uvs.Add(new Vector2(_floorWidth, _floorThickness));
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(_floorWidth, 0.0f));

        int backStartIndex = vertices.Count;
        vertices.Add(backLeftTop);
        vertices.Add(backRightTop);
        vertices.Add(backLeftBottom);
        vertices.Add(backRightBottom);
        uvs.Add(new Vector2(0.0f, _floorThickness));
        uvs.Add(new Vector2(_floorWidth, _floorThickness));
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(_floorWidth, 0.0f));

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

        // 左側面の三角形
        for (int i = 0; i < _resolution; i++)
        {
            int index = leftSideStartIndex + i * 2;

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            triangles.Add(index + 1);
            triangles.Add(index + 3);
            triangles.Add(index + 2);
        }

        //右側面の三角形
        for (int i = 0;i < _resolution; i++)
        {
            int index = rightSideStartIndex + i * 2;

            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 1);

            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        // 前面の三角形
        triangles.Add(frontStartIndex);
        triangles.Add(frontStartIndex + 1);
        triangles.Add(frontStartIndex + 2);

        triangles.Add(frontStartIndex + 1);
        triangles.Add(frontStartIndex + 3);
        triangles.Add(frontStartIndex + 2);

        // 後面の三角形
        triangles.Add(backStartIndex);
        triangles.Add(backStartIndex + 2);
        triangles.Add(backStartIndex + 1);

        triangles.Add(backStartIndex + 1);
        triangles.Add(backStartIndex + 2);
        triangles.Add(backStartIndex + 3);

        Mesh mesh = new Mesh();
        mesh.name = "GeneratedFloor";

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        _floorMeshFilter.sharedMesh = mesh;

        _floorMeshCollider.sharedMesh = null;
        _floorMeshCollider.sharedMesh = mesh;
    }
}
