using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class WallMeshGenerator
{
    private readonly SplineContainer _courseSpline;

    private readonly MeshFilter _leftWallMeshFilter;
    private readonly MeshFilter _rightWallMeshFilter;

    private readonly MeshCollider _leftWallMeshCollider;
    private readonly MeshCollider _rightWallMeshCollider;

    private readonly float _floorWidth;
    private readonly float _wallHeight;
    private readonly float _wallThickness;

    private readonly int _resolution;

    public WallMeshGenerator(
        SplineContainer courseSpline,
        MeshFilter leftWallMeshFilter,
        MeshFilter rightWallMeshFilter,
        MeshCollider leftWallMeshCollider,
        MeshCollider rightWallMeshCollider,
        float floorWidth,
        float wallHeight,
        float wallThickness,
        int resolution)
    {
        _courseSpline = courseSpline;

        _leftWallMeshFilter = leftWallMeshFilter;
        _rightWallMeshFilter = rightWallMeshFilter;

        _leftWallMeshCollider = leftWallMeshCollider;
        _rightWallMeshCollider = rightWallMeshCollider;

        _floorWidth = floorWidth;
        _wallHeight = wallHeight;
        _wallThickness = wallThickness;

        _resolution = resolution;
    }

    public void Generate()
    {
        if (_courseSpline == null || _leftWallMeshFilter == null || _rightWallMeshFilter == null || _leftWallMeshCollider == null || _rightWallMeshCollider == null)
        {
            Debug.LogError("CourseMeshGenerator: 壁生成に必要な参照が設定されていません");
            return;
        }

        List<Vector3> leftVertices = new();
        List<int> leftTriangles = new();

        List<Vector3> rightVertices = new();
        List<int> rightTriangles = new();

        float halfFloorWidth = _floorWidth * 0.5f;

        for (int i = 0; i <= _resolution; i++)
        {
            float t = (float)i / _resolution;

            Vector3 position = _courseSpline.EvaluatePosition(t);

            Vector3 tangent = _courseSpline.EvaluateTangent(t);
            tangent.Normalize();

            Vector3 up = _courseSpline.EvaluateUpVector(t);
            up.Normalize();

            Vector3 right = Vector3.Cross(up, tangent).normalized;

            //左壁
            Vector3 leftOuterBottom = position - right * halfFloorWidth;
            Vector3 leftInnerBottom = leftOuterBottom + right * _wallThickness;
            Vector3 leftOuterTop = leftOuterBottom + up * _wallHeight;
            Vector3 leftInnerTop = leftInnerBottom + up * _wallHeight;

            //右壁
            Vector3 rightOuterBottom = position + right * halfFloorWidth;
            Vector3 rightInnerBottom = rightOuterBottom - right * _wallThickness;
            Vector3 rightOuterTop = rightOuterBottom + up * _wallHeight;
            Vector3 rightInnerTop = rightInnerBottom + up * _wallHeight;

            leftOuterBottom = _leftWallMeshFilter.transform.InverseTransformPoint(leftOuterBottom);
            leftInnerBottom = _leftWallMeshFilter.transform.InverseTransformPoint(leftInnerBottom);
            leftOuterTop = _leftWallMeshFilter.transform.InverseTransformPoint(leftOuterTop);
            leftInnerTop = _leftWallMeshFilter.transform.InverseTransformPoint(leftInnerTop);

            rightOuterBottom = _rightWallMeshFilter.transform.InverseTransformPoint(rightOuterBottom);
            rightInnerBottom = _rightWallMeshFilter.transform.InverseTransformPoint(rightInnerBottom);
            rightOuterTop = _rightWallMeshFilter.transform.InverseTransformPoint(rightOuterTop);
            rightInnerTop = _rightWallMeshFilter.transform.InverseTransformPoint(rightInnerTop);

            leftVertices.Add(leftOuterBottom);
            leftVertices.Add(leftInnerBottom);
            leftVertices.Add(leftOuterTop);
            leftVertices.Add(leftInnerTop);

            rightVertices.Add(rightOuterBottom);
            rightVertices.Add(rightInnerBottom);
            rightVertices.Add(rightOuterTop);
            rightVertices.Add(rightInnerTop);
        }

        for (int j = 0; j < _resolution; j++)
        {
            int index = j * 4;

            // 左壁の内側面
            leftTriangles.Add(index + 1);
            leftTriangles.Add(index + 3);
            leftTriangles.Add(index + 5);

            leftTriangles.Add(index + 3);
            leftTriangles.Add(index + 7);
            leftTriangles.Add(index + 5);

            // 左壁の外側面
            leftTriangles.Add(index);
            leftTriangles.Add(index + 4);
            leftTriangles.Add(index + 2);

            leftTriangles.Add(index + 2);
            leftTriangles.Add(index + 4);
            leftTriangles.Add(index + 6);

            //左壁の上面
            leftTriangles.Add(index + 2);
            leftTriangles.Add(index + 6);
            leftTriangles.Add(index + 3);

            leftTriangles.Add(index + 3);
            leftTriangles.Add(index + 6);
            leftTriangles.Add(index + 7);

            // 右壁の内側面
            rightTriangles.Add(index + 1);
            rightTriangles.Add(index + 5);
            rightTriangles.Add(index + 3);

            rightTriangles.Add(index + 3);
            rightTriangles.Add(index + 5);
            rightTriangles.Add(index + 7);

            // 右壁の外側面
            rightTriangles.Add(index);
            rightTriangles.Add(index + 2);
            rightTriangles.Add(index + 4);

            rightTriangles.Add(index + 2);
            rightTriangles.Add(index + 6);
            rightTriangles.Add(index + 4);

            // 右壁の上面
            rightTriangles.Add(index + 2);
            rightTriangles.Add(index + 3);
            rightTriangles.Add(index + 6);

            rightTriangles.Add(index + 3);
            rightTriangles.Add(index + 7);
            rightTriangles.Add(index + 6);
        }

        // 左壁の前面
        leftTriangles.Add(0);
        leftTriangles.Add(2);
        leftTriangles.Add(1);

        leftTriangles.Add(1);
        leftTriangles.Add(2);
        leftTriangles.Add(3);

        // 右壁の前面
        rightTriangles.Add(0);
        rightTriangles.Add(1);
        rightTriangles.Add(2);

        rightTriangles.Add(1);
        rightTriangles.Add(3);
        rightTriangles.Add(2);

        int lastIndex = _resolution * 4;

        // 左壁の後面
        leftTriangles.Add(lastIndex);
        leftTriangles.Add(lastIndex + 1);
        leftTriangles.Add(lastIndex + 2);

        leftTriangles.Add(lastIndex + 1);
        leftTriangles.Add(lastIndex + 3);
        leftTriangles.Add(lastIndex + 2);

        // 右壁の後面
        rightTriangles.Add(lastIndex);
        rightTriangles.Add(lastIndex + 2);
        rightTriangles.Add(lastIndex + 1);

        rightTriangles.Add(lastIndex + 1);
        rightTriangles.Add(lastIndex + 2);
        rightTriangles.Add(lastIndex + 3);

        Mesh leftMesh = new Mesh();
        leftMesh.name = "GeneratedLeftWall";
        leftMesh.SetVertices(leftVertices);
        leftMesh.SetTriangles(leftTriangles, 0);
        leftMesh.RecalculateNormals();
        leftMesh.RecalculateBounds();

        _leftWallMeshFilter.sharedMesh = leftMesh;

        _leftWallMeshCollider.sharedMesh = null;
        _leftWallMeshCollider.sharedMesh = leftMesh;

        Mesh rightMesh = new Mesh();
        rightMesh.name = "GeneratedRightWall";
        rightMesh.SetVertices(rightVertices);
        rightMesh.SetTriangles(rightTriangles, 0);
        rightMesh.RecalculateNormals();
        rightMesh.RecalculateBounds();

        _rightWallMeshFilter.sharedMesh = rightMesh;

        _rightWallMeshCollider.sharedMesh = null;
        _rightWallMeshCollider.sharedMesh = rightMesh;
    }
}