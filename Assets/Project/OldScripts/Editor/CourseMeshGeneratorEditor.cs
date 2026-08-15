#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// エディタ上でSplineが変更された際に、CourseMeshGeneratorのGenerateCourseメソッドを自動的に呼び出すクラス
/// </summary>
[InitializeOnLoad]
public static class CourseMeshGeneratorEditor
{
    static CourseMeshGeneratorEditor()
    {
        EditorSplineUtility.AfterSplineWasModified += OnSplineModified;
    }

    private static void OnSplineModified(Spline spline)
    {
        CourseMeshGenerator[] generators = Object.FindObjectsByType<CourseMeshGenerator>(FindObjectsSortMode.None);


        foreach (CourseMeshGenerator generator in generators)
        {
            if (generator.CourseSpline == null)
                continue;
            //変更さてらSplineにのみ適用する
            if (generator.CourseSpline.Spline == spline)
            {
                generator.GenerateCourse();
            }
        }
    }
        
}

#endif