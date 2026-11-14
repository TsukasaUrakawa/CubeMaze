using UnityEngine;
using DG.Tweening;

/// <summary>
/// タイトル文字のマテリアルの光源角度を繰り返し変化させる。
/// </summary>
public class TitleTextController : MonoBehaviour
{
    /// <summary>
    /// TextMeshProの_LightAngleを持つタイトル用マテリアル。
    /// </summary>
    [SerializeField] private Material _titleTextMaterial;
    /// <summary>
    /// 各周回の終了角度（ラジアン）。
    /// </summary>
    private float _endValue = 0;
    /// <summary>
    /// 光源角度が一周するまでの時間（秒）。
    /// </summary>
    [SerializeField] private int _duration = 1;

    /// <summary>
    /// 光源角度を2πから0へ補間し、中間で速くなる変化を繰り返す。
    /// </summary>
    void Start()
    {
        _titleTextMaterial.SetFloat("_LightAngle", 2 * Mathf.PI);
        // 2πと0は同じ向き。Restartで角度を戻しながら、一方向の周回を繰り返す。
        _titleTextMaterial.DOFloat(_endValue, "_LightAngle", _duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart);
    }
}
