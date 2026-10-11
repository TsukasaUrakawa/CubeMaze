using UnityEngine;
using DG.Tweening;

public class TitleTextController : MonoBehaviour
{
    [SerializeField] private Material _titleTextMaterial;
    private float _endValue = 0;
    [SerializeField] private int _duration = 1;

    void Start()
    {
        _titleTextMaterial.SetFloat("_LightAngle", 2 * Mathf.PI);
        _titleTextMaterial.DOFloat(_endValue, "_LightAngle", _duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart);
    }
}
