using System.Runtime.CompilerServices;
using UnityEngine;
using static JSL;

public class GyroTest : MonoBehaviour
{
    /// <summary>
    /// 使用中のデバイスの識別番号と接続状態を取得する
    /// </summary>
    [SerializeField] private DeviceConnectTest _deviceConnectTest;
    /// <summary>
    /// 現在の回転からコントローラーの姿勢へ補間するときの割合
    /// </summary>
    [SerializeField] private float _rotateSpeed = 0.3f;

    [SerializeField] private float _rotationDeadZoneDegrees = 0.05f;
    private Rigidbody _rigidbody;

    private Vector3 _calibrationReferenceUp = Vector3.zero;

    private Quaternion _calibrationReferenceRotation = Quaternion.identity;

    private Quaternion _mazeReferenceRotation = Quaternion.identity;

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
        if (_deviceConnectTest.HasActiveDevice)
        {
            MOTION_STATE motion = JslGetMotionState(_deviceConnectTest.ActiveDeviceHandle); // 使用中のデバイスの識別番号からモーションステートを取得
            _targetRotation = new Quaternion(motion.quatX, -motion.quatY, -motion.quatZ, motion.quatW);
        }
        else
        {
            return;
        }
    }

    private void FixedUpdate()
    {
        if (_deviceConnectTest.HasActiveDevice)
        {
            if (IsValid(_targetRotation))
            {
                _targetRotation.Normalize();

                if (!_isStartedCalibration)
                {
                    _calibrationElapsedTime = 0f;
                    JslResetContinuousCalibration(_deviceConnectTest.ActiveDeviceHandle);
                    JslStartContinuousCalibration(_deviceConnectTest.ActiveDeviceHandle);
                    _isStartedCalibration = true;
                }

                if (!_isCalibrationCompleted)
                {
                    _calibrationElapsedTime += Time.fixedDeltaTime;
                }

                if (_calibrationElapsedTime > 5f && !_isCalibrationCompleted)
                {
                    JslPauseContinuousCalibration(_deviceConnectTest.ActiveDeviceHandle);
                    _calibrationReferenceRotation = _targetRotation;
                    _calibrationReferenceUp = _calibrationReferenceRotation * Vector3.up;
                    _mazeReferenceRotation = _rigidbody.rotation;

                    float angle = Quaternion.Angle(this._rigidbody.rotation, _targetRotation);
                    Debug.Log(angle);
                    _isCalibrationCompleted = true;
                }

                if (!_isCalibrationCompleted)
                {
                    return;
                }
            }

            Quaternion relativeCalibrationRotation = Quaternion.Inverse(_calibrationReferenceRotation) * _targetRotation;
            Quaternion adjustedRotation = _mazeReferenceRotation * relativeCalibrationRotation;

            if (IsValid(adjustedRotation))
            {
                adjustedRotation.Normalize();

                if (Quaternion.Angle(_rigidbody.rotation, adjustedRotation) > _rotationDeadZoneDegrees)
                {
                    this._rigidbody.MoveRotation(Quaternion.Slerp(this._rigidbody.rotation, adjustedRotation, _rotateSpeed));
                }
            }
        }
        else
        {
            return;
        }
    }

    private bool IsValid(Quaternion quaternion)
    {
        if (float.IsNaN(quaternion.x) || float.IsNaN(quaternion.y) || float.IsNaN(quaternion.z) || float.IsNaN(quaternion.w) ||
           float.IsInfinity(quaternion.x) || float.IsInfinity(quaternion.y) || float.IsInfinity(quaternion.z) || float.IsInfinity(quaternion.w))
            return false;

        if (Quaternion.Dot(quaternion, quaternion) <= float.Epsilon)
            return false;

        return true;
    }
}






