using Unity.Cinemachine;
using UnityEngine;
using static JSL;

/// <summary>
/// 観察モードのL・R入力でカメラを段階的に旋回し、復帰時には初期角度へ戻す。
/// </summary>
public class CameraOrbitController : MonoBehaviour
{
    /// <summary>
    /// 入力を取得する機器と接続状態の参照先。
    /// </summary>
    [SerializeField] private DeviceConnectManager _deviceConnectManager;
    /// <summary>
    /// 現在の迷路操作が観察モードかを確認する相手。
    /// </summary>
    [SerializeField] private GyroController _gyroController;

    /// <summary>
    /// カメラの水平周回角度を変更するCinemachineのコンポーネント。
    /// </summary>
    private CinemachineOrbitalFollow _orbitalFollow;

    /// <summary>
    /// 前回の押下ビット。L・Rを押し始めた瞬間を判定する。
    /// </summary>
    private int _previousButtons = 0;
    // ボタン番号をビットマスクへ変換し、押し始めを前回値との差で判定する。
    private int _buttonMaskL = 1 << ButtonMaskL;
    private int _buttonMaskR = 1 << ButtonMaskR;

    /// <summary>
    /// 開始済みのカメラ旋回を補間しているか。
    /// </summary>
    private bool _isRotating = false;
    /// <summary>
    /// 旋回途中かを公開し、迷路側の観察適用の受付に使用する。
    /// </summary>
    public bool IsRotating
    {
        get
        {
            return _isRotating;
        }
    }
    /// <summary>
    /// 今回の旋回開始角度（度）。
    /// </summary>
    private float _startAngle = 0f;
    /// <summary>
    /// 今回の旋回到達角度（度）。
    /// </summary>
    private float _targetAngle = 0f;
    /// <summary>
    /// 今回の旋回に費やした時間（秒）。
    /// </summary>
    private float _rotationElapsedTime = 0f;
    /// <summary>
    /// 一回のカメラ旋回にかける時間（秒）。
    /// </summary>
    [SerializeField, Range(1f, 3f)] private float _rotationDuration = 1f;

    /// <summary>
    /// 復帰時に戻す、起動時の水平周回角度。
    /// </summary>
    private float _initialHorizontalAngle;

    /// <summary>
    /// 周回コンポーネントを取得し、復帰用の水平角度を保存する。
    /// </summary>
    void Awake()
    {
        _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        _initialHorizontalAngle = _orbitalFollow.HorizontalAxis.Value;
    }

    /// <summary>
    /// 使用中の機器から観察入力を読み、開始済みの旋回を時間補間する。
    /// </summary>
    void Update()
    {
        if (_deviceConnectManager.CurrentConnectionState != DeviceConnectManager.ConnectionState.InUse)
        {
            return;
        }
        else
        {
            JOY_SHOCK_STATE state = JslGetSimpleState(_deviceConnectManager.ActiveDeviceHandle);
            if (_gyroController.CurrentControllState == GyroController.ControllState.Viewing)
            {
                if ((state.buttons & _buttonMaskL) != 0 && (_previousButtons & _buttonMaskL) == 0)
                {
                    StartRotation(90f);
                }
                else if ((state.buttons & _buttonMaskR) != 0 && (_previousButtons & _buttonMaskR) == 0)
                {
                    StartRotation(-90f);
                }
            }
            _previousButtons = state.buttons;
            if (_isRotating)
            {
                _rotationElapsedTime += Time.deltaTime;
                float progress = _rotationElapsedTime / _rotationDuration;
                // 周回角度を補間し、Cinemachineの軸が扱う範囲へ収める。
                float currentAngle = Mathf.Lerp(_startAngle, _targetAngle, progress);
                _orbitalFollow.HorizontalAxis.Value = _orbitalFollow.HorizontalAxis.ClampValue(currentAngle);
                if (progress >= 1)
                {
                    _isRotating = false;
                }
            }
        }
    }

    /// <summary>
    /// 旋回中の追加要求を拒否し、新しい旋回の開始角度と目標を保存する。
    /// </summary>
    /// <param name="angle">現在角度から加える角度（度）。符号で方向を指定する。</param>
    private void StartRotation(float angle)
    {
        if (_isRotating)
        {
            return;
        }
        _startAngle = _orbitalFollow.HorizontalAxis.Value;
        _targetAngle = _startAngle + angle;
        _rotationElapsedTime = 0f;
        _isRotating = true;
    }

    /// <summary>
    /// 進行中の旋回を中断し、水平角度を起動時の値へ戻す。
    /// </summary>
    public void ResetCameraForRespawn()
    {
        _isRotating = false;
        _rotationElapsedTime = 0f;
        _orbitalFollow.HorizontalAxis.Value = _initialHorizontalAngle;
    }
}
