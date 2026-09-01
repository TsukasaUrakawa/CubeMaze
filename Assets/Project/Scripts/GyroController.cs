using UnityEngine;
using static JSL;

/// <summary>
/// 使用中のコントローラーから姿勢情報を取得し、Rigidbodyの回転に反映
/// </summary>
public class GyroController : MonoBehaviour
{
    /// <summary>
    /// 使用中のデバイスの識別番号と接続状態を取得する
    /// </summary>
    [SerializeField] private DeviceConnectManager _deviceConnectManager;
    /// <summary>
    /// 現在の回転からコントローラーの姿勢へ補間するときの割合
    /// </summary>
    [SerializeField] private float _rotateSpeed = 0.3f;

    [SerializeField] private float _rotationDeadZoneDegrees = 0.05f;
    private Rigidbody _rigidbody;
    /// <summary>
    /// コントローラーから取得した姿勢をUnity用に変換した目標回転
    /// </summary>
    private Quaternion _targetRotation = Quaternion.identity;
    private bool _isStartedCalibration = false;

    private bool _isCalibrationCompleted = false;

    private float _calibrationElapsedTime = 0f;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        switch (_deviceConnectManager.CurrentConnectionState)
        {
            case (DeviceConnectManager.ConnectionState.Calibrating):
            case (DeviceConnectManager.ConnectionState.InUse):
                MOTION_STATE motion = JslGetMotionState(_deviceConnectManager.ActiveDeviceHandle); // 使用中のデバイスの識別番号からモーションステートを取得

                _targetRotation = new Quaternion(motion.quatX, -motion.quatY, -motion.quatZ, motion.quatW);
                break;
            default:
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (_deviceConnectManager.CurrentConnectionState)
        {
            case (DeviceConnectManager.ConnectionState.Calibrating):
                if (IsValid(_targetRotation))
                {
                    _targetRotation.Normalize();

                    if (!_isStartedCalibration)
                    {
                        _calibrationElapsedTime = 0f;
                        JslResetContinuousCalibration(_deviceConnectManager.ActiveDeviceHandle);
                        JslStartContinuousCalibration(_deviceConnectManager.ActiveDeviceHandle);
                        _isStartedCalibration = true;
                    }

                    if (!_isCalibrationCompleted)
                    {
                        _calibrationElapsedTime += Time.fixedDeltaTime;
                    }

                    if (_calibrationElapsedTime > 5f && !_isCalibrationCompleted)
                    {
                        JslPauseContinuousCalibration(_deviceConnectManager.ActiveDeviceHandle);
                        _isCalibrationCompleted = true;
                        _deviceConnectManager.CompleteCalibration();
                    }

                    if (!_isCalibrationCompleted)
                    {
                        return;
                    }
                }
                break;
            case (DeviceConnectManager.ConnectionState.InUse):
                if (IsValid(_targetRotation))
                {
                    _targetRotation.Normalize();

                    float angleDifference = Quaternion.Angle(_rigidbody.rotation, _targetRotation);
                    if (angleDifference > _rotationDeadZoneDegrees)
                    {
                        Quaternion result = Quaternion.Slerp(this._rigidbody.rotation, _targetRotation, _rotateSpeed);
                        this._rigidbody.MoveRotation(result);
                    }
                }
                break;
            default:
                _isStartedCalibration = false;
                _isCalibrationCompleted = false;
                _calibrationElapsedTime = 0f;
                break;
        }
    }

    private bool IsValid(Quaternion quaternion)
    {
        if (float.IsNaN(quaternion.x) || float.IsNaN(quaternion.y) || float.IsNaN(quaternion.z) || float.IsNaN(quaternion.w) ||
           float.IsInfinity(quaternion.x) || float.IsInfinity(quaternion.y) || float.IsInfinity(quaternion.z) || float.IsInfinity(quaternion.w))
        {
            return false;
        }

        float quaternionLength = Quaternion.Dot(quaternion, quaternion); // クォータニオンの長さの二乗を計算

        if (quaternionLength <= float.Epsilon)
        {
            return false;
        }

        return true;
    }
}
