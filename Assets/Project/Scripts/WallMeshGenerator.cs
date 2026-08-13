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

        // 左壁
        List<Vector3> leftInnerVertices = new();
        List<Vector3> leftOuterVertices = new();
        List<Vector3> leftTopVertices = new();

        List<Vector2> leftInnerUvs = new();
        List<Vector2> leftOuterUvs = new();
        List<Vector2> leftTopUvs = new();

        List<Vector3> leftVertices = new();
        List<int> leftTriangles = new();
        List<Vector2> leftUvs = new();

        // 右壁
        List<Vector3> rightInnerVertices = new();
        List<Vector3> rightOuterVertices = new();
        List<Vector3> rightTopVertices = new();

        List<Vector2> rightInnerUvs = new();
        List<Vector2> rightOuterUvs = new();
        List<Vector2> rightTopUvs = new();

        List<Vector3> rightVertices = new();
        List<int> rightTriangles = new();
        List<Vector2> rightUvs = new();

        float halfFloorWidth = _floorWidth * 0.5f;
        float splineLength = _courseSpline.CalculateLength();

        // 前面
        Vector3 leftFrontOuterBottom = Vector3.zero;
        Vector3 leftFrontInnerBottom = Vector3.zero;
        Vector3 leftFrontOuterTop = Vector3.zero;
        Vector3 leftFrontInnerTop = Vector3.zero;

        Vector3 rightFrontOuterBottom = Vector3.zero;
        Vector3 rightFrontInnerBottom = Vector3.zero;
        Vector3 rightFrontOuterTop = Vector3.zero;
        Vector3 rightFrontInnerTop = Vector3.zero;

        // 後面
        Vector3 leftBackOuterBottom = Vector3.zero;
        Vector3 leftBackInnerBottom = Vector3.zero;
        Vector3 leftBackOuterTop = Vector3.zero;
        Vector3 leftBackInnerTop = Vector3.zero;

        Vector3 rightBackOuterBottom = Vector3.zero;
        Vector3 rightBackInnerBottom = Vector3.zero;
        Vector3 rightBackOuterTop = Vector3.zero;
        Vector3 rightBackInnerTop = Vector3.zero;



        for (int i = 0; i <= _resolution; i++)
        {
            float t = (float)i / _resolution;

            Vector3 position = _courseSpline.EvaluatePosition(t);

            Vector3 tangent = _courseSpline.EvaluateTangent(t);
            tangent.Normalize();

            Vector3 up = _courseSpline.EvaluateUpVector(t);
            up.Normalize();

            Vector3 right = Vector3.Cross(up, tangent).normalized;

            // 左壁
            Vector3 leftOuterBottom = position - right * halfFloorWidth;
            Vector3 leftInnerBottom = leftOuterBottom + right * _wallThickness;
            Vector3 leftOuterTop = leftOuterBottom + up * _wallHeight;
            Vector3 leftInnerTop = leftInnerBottom + up * _wallHeight;

            // 右壁
            Vector3 rightOuterBottom = position + right * halfFloorWidth;
            Vector3 rightInnerBottom = rightOuterBottom - right * _wallThickness;
            Vector3 rightOuterTop = rightOuterBottom + up * _wallHeight;
            Vector3 rightInnerTop = rightInnerBottom + up * _wallHeight;

            // MeshFilterを基準にしたローカル座標に変換
            leftOuterBottom = _leftWallMeshFilter.transform.InverseTransformPoint(leftOuterBottom);
            leftInnerBottom = _leftWallMeshFilter.transform.InverseTransformPoint(leftInnerBottom);
            leftOuterTop = _leftWallMeshFilter.transform.InverseTransformPoint(leftOuterTop);
            leftInnerTop = _leftWallMeshFilter.transform.InverseTransformPoint(leftInnerTop);

            rightOuterBottom = _rightWallMeshFilter.transform.InverseTransformPoint(rightOuterBottom);
            rightInnerBottom = _rightWallMeshFilter.transform.InverseTransformPoint(rightInnerBottom);
            rightOuterTop = _rightWallMeshFilter.transform.InverseTransformPoint(rightOuterTop);
            rightInnerTop = _rightWallMeshFilter.transform.InverseTransformPoint(rightInnerTop);

            if (i == 0)
            {
                leftFrontOuterBottom = leftOuterBottom;
                leftFrontInnerBottom = leftInnerBottom;
                leftFrontOuterTop = leftOuterTop;
                leftFrontInnerTop = leftInnerTop;

                rightFrontOuterBottom = rightOuterBottom;
                rightFrontInnerBottom = rightInnerBottom;
                rightFrontOuterTop = rightOuterTop;
                rightFrontInnerTop = rightInnerTop;
            }

            if (i == _resolution)
            {
                leftBackOuterBottom = leftOuterBottom;
                leftBackInnerBottom = leftInnerBottom;
                leftBackOuterTop = leftOuterTop;
                leftBackInnerTop = leftInnerTop;

                rightBackOuterBottom = rightOuterBottom;
                rightBackInnerBottom = rightInnerBottom;
                rightBackOuterTop = rightOuterTop;
                rightBackInnerTop = rightInnerTop;
            }

            //左壁の内側面
            leftInnerVertices.Add(leftInnerBottom);
            leftInnerVertices.Add(leftInnerTop);

            //左壁の外側面
            leftOuterVertices.Add(leftOuterBottom);
            leftOuterVertices.Add(leftOuterTop);

            //左壁の上面
            leftTopVertices.Add(leftOuterTop);
            leftTopVertices.Add(leftInnerTop);

            //右壁の内側面
            rightInnerVertices.Add(rightInnerBottom);
            rightInnerVertices.Add(rightInnerTop);

            //右壁の外側面
            rightOuterVertices.Add(rightOuterBottom);
            rightOuterVertices.Add(rightOuterTop);

            //右壁の上面
            rightTopVertices.Add(rightInnerTop);
            rightTopVertices.Add(rightOuterTop);

            float u = t * splineLength;

            leftInnerUvs.Add(new Vector2(u, 0.0f));
            leftInnerUvs.Add(new Vector2(u, _wallHeight));

            leftOuterUvs.Add(new Vector2(u, 0.0f));
            leftOuterUvs.Add(new Vector2(u, _wallHeight));

            rightInnerUvs.Add(new Vector2(u, 0.0f));
            rightInnerUvs.Add(new Vector2(u, _wallHeight));

            rightOuterUvs.Add(new Vector2(u, 0.0f));
            rightOuterUvs.Add(new Vector2(u, _wallHeight));

            leftTopUvs.Add(new Vector2(u, 0.0f));
            leftTopUvs.Add(new Vector2(u, _wallThickness));

            rightTopUvs.Add(new Vector2(u, 0.0f));
            rightTopUvs.Add(new Vector2(u, _wallThickness));
        }

        int leftInnerStartIndex = leftVertices.Count;
        leftVertices.AddRange(leftInnerVertices);
        leftUvs.AddRange(leftInnerUvs);

        int leftOuterStartIndex = leftVertices.Count;
        leftVertices.AddRange(leftOuterVertices);
        leftUvs.AddRange(leftOuterUvs);

        int leftTopStartIndex = leftVertices.Count;
        leftVertices.AddRange(leftTopVertices);
        leftUvs.AddRange(leftTopUvs);

        int rightInnerStartIndex = rightVertices.Count;
        rightVertices.AddRange(rightInnerVertices);
        rightUvs.AddRange(rightInnerUvs);

        int rightOuterStartIndex = rightVertices.Count;
        rightVertices.AddRange(rightOuterVertices);
        rightUvs.AddRange(rightOuterUvs);

        int rightTopStartIndex = rightVertices.Count;
        rightVertices.AddRange(rightTopVertices);
        rightUvs.AddRange(rightTopUvs);

        int leftFrontStartIndex = leftVertices.Count;

        leftVertices.Add(leftFrontOuterBottom);
        leftVertices.Add(leftFrontInnerBottom);
        leftVertices.Add(leftFrontOuterTop);
        leftVertices.Add(leftFrontInnerTop);

        leftUvs.Add(new Vector2(0.0f, 0.0f));
        leftUvs.Add(new Vector2(_wallThickness, 0.0f));
        leftUvs.Add(new Vector2(0.0f, _wallHeight));
        leftUvs.Add(new Vector2(_wallThickness, _wallHeight));

        int leftBackStartIndex = leftVertices.Count;

        leftVertices.Add(leftBackOuterBottom);
        leftVertices.Add(leftBackInnerBottom);
        leftVertices.Add(leftBackOuterTop);
        leftVertices.Add(leftBackInnerTop);

        leftUvs.Add(new Vector2(0.0f, 0.0f));
        leftUvs.Add(new Vector2(_wallThickness, 0.0f));
        leftUvs.Add(new Vector2(0.0f, _wallHeight));
        leftUvs.Add(new Vector2(_wallThickness, _wallHeight));

        int rightFrontStartIndex = rightVertices.Count;

        rightVertices.Add(rightFrontOuterBottom);
        rightVertices.Add(rightFrontInnerBottom);
        rightVertices.Add(rightFrontOuterTop);
        rightVertices.Add(rightFrontInnerTop);

        rightUvs.Add(new Vector2(0.0f, 0.0f));
        rightUvs.Add(new Vector2(_wallThickness, 0.0f));
        rightUvs.Add(new Vector2(0.0f, _wallHeight));
        rightUvs.Add(new Vector2(_wallThickness, _wallHeight));

        int rightBackStartIndex = rightVertices.Count;

        rightVertices.Add(rightBackOuterBottom);
        rightVertices.Add(rightBackInnerBottom);
        rightVertices.Add(rightBackOuterTop);
        rightVertices.Add(rightBackInnerTop);

        rightUvs.Add(new Vector2(0.0f, 0.0f));
        rightUvs.Add(new Vector2(_wallThickness, 0.0f));
        rightUvs.Add(new Vector2(0.0f, _wallHeight));
        rightUvs.Add(new Vector2(_wallThickness, _wallHeight));

        //左壁の内側面
        for (int i = 0; i < _resolution; i++)
        {
            int index = leftInnerStartIndex + i * 2;

            leftTriangles.Add(index);
            leftTriangles.Add(index + 1);
            leftTriangles.Add(index + 2);

            leftTriangles.Add(index + 1);
            leftTriangles.Add(index + 3);
            leftTriangles.Add(index + 2);
        }

        //左壁の外側面
        for (int i = 0; i < _resolution; i++)
        {
            int index = leftOuterStartIndex + i * 2;

            leftTriangles.Add(index);
            leftTriangles.Add(index + 2);
            leftTriangles.Add(index + 1);

            leftTriangles.Add(index + 1);
            leftTriangles.Add(index + 2);
            leftTriangles.Add(index + 3);
        }

        //左壁の上面
        for (int i=0; i < _resolution; i++)
        {
            int index = leftTopStartIndex + i * 2;

            leftTriangles.Add(index);
            leftTriangles.Add(index + 2);
            leftTriangles.Add(index + 1);

            leftTriangles.Add(index + 1);
            leftTriangles.Add(index + 2);
            leftTriangles.Add(index + 3);
        }

        // 右壁の内側面
        for (int i = 0; i < _resolution; i++)
        {
            int index = rightInnerStartIndex + i * 2;

            rightTriangles.Add(index);
            rightTriangles.Add(index + 2);
            rightTriangles.Add(index + 1);

            rightTriangles.Add(index + 1);
            rightTriangles.Add(index + 2);
            rightTriangles.Add(index + 3);
        }

        // 右壁の外側面
        for (int i = 0; i < _resolution; i++)
        {
            int index = rightOuterStartIndex + i * 2;

            rightTriangles.Add(index);
            rightTriangles.Add(index + 1);
            rightTriangles.Add(index + 2);

            rightTriangles.Add(index + 1);
            rightTriangles.Add(index + 3);
            rightTriangles.Add(index + 2);
        }

        // 右壁の上面
        for (int i = 0; i < _resolution; i++)
        {
            int index = rightTopStartIndex + i * 2;

            rightTriangles.Add(index);
            rightTriangles.Add(index + 2);
            rightTriangles.Add(index + 1);

            rightTriangles.Add(index + 1);
            rightTriangles.Add(index + 2);
            rightTriangles.Add(index + 3);
        }

        leftTriangles.Add(leftFrontStartIndex);
        leftTriangles.Add(leftFrontStartIndex + 2);
        leftTriangles.Add(leftFrontStartIndex + 1);

        leftTriangles.Add(leftFrontStartIndex + 1);
        leftTriangles.Add(leftFrontStartIndex + 2);
        leftTriangles.Add(leftFrontStartIndex + 3);

        leftTriangles.Add(leftBackStartIndex);
        leftTriangles.Add(leftBackStartIndex + 1);
        leftTriangles.Add(leftBackStartIndex + 2);

        leftTriangles.Add(leftBackStartIndex + 1);
        leftTriangles.Add(leftBackStartIndex + 3);
        leftTriangles.Add(leftBackStartIndex + 2);

        rightTriangles.Add(rightFrontStartIndex);
        rightTriangles.Add(rightFrontStartIndex + 1);
        rightTriangles.Add(rightFrontStartIndex + 2);

        rightTriangles.Add(rightFrontStartIndex + 1);
        rightTriangles.Add(rightFrontStartIndex + 3);
        rightTriangles.Add(rightFrontStartIndex + 2);

        rightTriangles.Add(rightBackStartIndex);
        rightTriangles.Add(rightBackStartIndex + 2);
        rightTriangles.Add(rightBackStartIndex + 1);

        rightTriangles.Add(rightBackStartIndex + 1);
        rightTriangles.Add(rightBackStartIndex + 2);
        rightTriangles.Add(rightBackStartIndex + 3);

        Mesh leftMesh = new Mesh();
        leftMesh.name = "GeneratedLeftWall";
        leftMesh.SetVertices(leftVertices);
        leftMesh.SetTriangles(leftTriangles, 0);
        leftMesh.SetUVs(0, leftUvs);
        leftMesh.RecalculateNormals();
        leftMesh.RecalculateTangents();
        leftMesh.RecalculateBounds();

        _leftWallMeshFilter.sharedMesh = leftMesh;

        _leftWallMeshCollider.sharedMesh = null;
        _leftWallMeshCollider.sharedMesh = leftMesh;

        Mesh rightMesh = new Mesh();
        rightMesh.name = "GeneratedRightWall";
        rightMesh.SetVertices(rightVertices);
        rightMesh.SetTriangles(rightTriangles, 0);
        rightMesh.SetUVs(0, rightUvs);
        rightMesh.RecalculateNormals();
        rightMesh.RecalculateTangents();
        rightMesh.RecalculateBounds();

        _rightWallMeshFilter.sharedMesh = rightMesh;

        _rightWallMeshCollider.sharedMesh = null;
        _rightWallMeshCollider.sharedMesh = rightMesh;
    }
}