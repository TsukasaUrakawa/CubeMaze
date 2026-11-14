using UnityEngine;

/// <summary>
/// 画面上のボール位置と奥行きを、迷路の透過用マテリアルへ渡す。
/// </summary>
public class MazeCutOutController : MonoBehaviour
{
    /// <summary>
    /// ボールのワールド位置を画面内の位置と奥行きへ変換するカメラ。
    /// </summary>
    [SerializeField] Camera _mainCamera;
    /// <summary>
    /// 透過する穴の中心として追跡するボール。
    /// </summary>
    [SerializeField] Transform _ballTransform;
    /// <summary>
    /// _HoleCenterと_BallDepthを更新する対象マテリアル。
    /// </summary>
    [SerializeField] Material _mazeCutOutMaterial;
    /// <summary>
    /// ボールのカメラ奥行きから引く透過判定用の補正距離。
    /// </summary>
    [SerializeField, Range(1.0f, 3.0f)] private float _adjustDifference = 1.5f;

    /// <summary>
    /// 通常のUpdate後にボールの画面位置を求め、穴の中心と奥行きを更新する。
    /// </summary>
    private void LateUpdate()
    {
        // x・yは画面内の正規化位置、zはカメラからの奥行きとして別々の用途に渡す。
        Vector3 ballViewportPosition = _mainCamera.WorldToViewportPoint(_ballTransform.position);
        _mazeCutOutMaterial.SetVector("_HoleCenter", new Vector4(ballViewportPosition.x, ballViewportPosition.y, 0, 0));
        _mazeCutOutMaterial.SetFloat("_BallDepth", ballViewportPosition.z - _adjustDifference);
    }
}
