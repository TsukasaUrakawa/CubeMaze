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

        int topStartIndex = vertices.Count; //上面用の頂点が始まる番号

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

            // ローカル座標に変換
            leftTop = _floorMeshFilter.transform.InverseTransformPoint(leftTop);
            rightTop = _floorMeshFilter.transform.InverseTransformPoint(rightTop);

            // 頂点追加
            vertices.Add(leftTop);
            vertices.Add(rightTop);

            // UV
            float v = t * splineLength;

            uvs.Add(new Vector2(0.0f, v));
            uvs.Add(new Vector2(_floorWidth, v));
        }

        //上面の三角形を作る
        for (int i = 0; i < _resolution; i++)
        {

        }
    }
}
