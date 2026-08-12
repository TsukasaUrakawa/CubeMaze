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

    [SerializeField] private MeshCollider _floorMeshCollider;
    [SerializeField] private MeshCollider _leftWallMeshCollider;
    [SerializeField] private MeshCollider _rightWallMeshCollider;

    [Header("Course Settings")]
    [SerializeField] private float _floorWidth = 4.0f;
    [SerializeField] private float _floorThickness = 0.5f;
    [SerializeField] private float _wallHeight = 1.0f;
    [SerializeField] private float _wallThickness = 0.5f;

    //Spline‚Ì•ªŠ„”‚ğw’è‚·‚é•Ï”
    [SerializeField, Min(1)] private int _segmentPerUnit = 4;

    [ContextMenu("Generate Course")]
    public void GenerateCourse()
    {
        float splineLength = _courseSpline.CalculateLength();

        int resolution = Mathf.Max(1, Mathf.CeilToInt(splineLength * _segmentPerUnit));
        FloorMeshGenerator floorGenerator = new FloorMeshGenerator(_courseSpline, _floorMeshFilter, _floorMeshCollider, _floorWidth, _floorThickness, resolution);
        
        WallMeshGenerator wallGenerator = new WallMeshGenerator(_courseSpline, _leftWallMeshFilter, _rightWallMeshFilter, _leftWallMeshCollider, _rightWallMeshCollider, _floorWidth, _wallHeight, _wallThickness, resolution);

        floorGenerator.Generate();
        wallGenerator.Generate();
    }
}
