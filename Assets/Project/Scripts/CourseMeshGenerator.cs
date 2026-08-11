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
    [SerializeField] private float _floorWidth = 4.0f;
    [SerializeField] private float _floorThickness = 0.5f;
    [SerializeField] private float _wallHeight = 1.0f;
    [SerializeField] private float _wallThickness = 0.5f;

    //Splineの分割数を指定する変数
    [SerializeField, Min(1)] private int resolution = 20;
    
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

        float halfFloorWidth = _floorWidth * 0.5f;

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

            Vector3 leftTop = position - right * halfFloorWidth;
            Vector3 rightTop = position + right * halfFloorWidth;

            Vector3 leftBottom = leftTop - up * _floorThickness;
            Vector3 rightBottom = rightTop - up * _floorThickness;

            // ワールド座標からローカル座標に変換
            leftTop = _floorMeshFilter.transform.InverseTransformPoint(leftTop);
            rightTop = _floorMeshFilter.transform.InverseTransformPoint(rightTop);
            leftBottom = _floorMeshFilter.transform.InverseTransformPoint(leftBottom);
            rightBottom = _floorMeshFilter.transform.InverseTransformPoint(rightBottom);

            vertices.Add(leftTop);
            vertices.Add(rightTop);
            vertices.Add(leftBottom);
            vertices.Add(rightBottom);
        }

        for (int i = 0; i < resolution; i++)
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
        int lastIndex = resolution * 4;

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

        float halfFloorWidth = _floorWidth * 0.5f;

        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;

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

        for (int j = 0; j < resolution; j++)
        {
            int index = j * 4;

            // 左壁の内側面
            leftTriangles.Add(index+1);
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

        int lastIndex = resolution * 4;

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

        Mesh rightMesh = new Mesh();
        rightMesh.name = "GeneratedRightWall";
        rightMesh.SetVertices(rightVertices);
        rightMesh.SetTriangles(rightTriangles, 0);
        rightMesh.RecalculateNormals();
        rightMesh.RecalculateBounds();

        _rightWallMeshFilter.sharedMesh = rightMesh;
    }
}
