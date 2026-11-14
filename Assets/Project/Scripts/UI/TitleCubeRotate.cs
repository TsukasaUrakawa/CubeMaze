using UnityEngine;

/// <summary>
/// タイトルの迷路模型を、ワールドのY軸まわりに一定速度で回す。
/// </summary>
public class TitleCubeRotate : MonoBehaviour
{
    /// <summary>
    /// 回転させる自身のTransform。
    /// </summary>
    private Transform _titleCubeTransform;
    /// <summary>
    /// 一秒あたりの回転角度（度）。
    /// </summary>
    [SerializeField] private float _rotateSpeed = 72f;

    /// <summary>
    /// 回転対象のTransformを保存する。
    /// </summary>
    void Awake()
    {
        _titleCubeTransform = this.transform;
    }
    /// <summary>
    /// 経過時間に比例した角度を加え、フレーム数に左右されにくい回転にする。
    /// </summary>
    void Update()
    {
        _titleCubeTransform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime, Space.World);
    }
}
