using System.Collections;
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

    private Quaternion _calibrationReferenceRotation = Quaternion.identity;

    private Quaternion _mazeReferenceRotation = Quaternion.identity;


    /// <summary>
    /// コントローラーから取得した姿勢をUnity用に変換した目標回転
    /// </summary>
    private Quaternion _targetRotation = Quaternion.identity;
    private bool _isStartedCalibration = false;

    private bool _isCalibrationCompleted = false;

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
        Vector3 twistAxis = Vector3.up;

        Quaternion swing = Quaternion.identity;

        Quaternion twist = Quaternion.identity;

        if (_deviceConnectTest.HasActiveDevice)
        {
            if (IsValid(_targetRotation))
            {
                _targetRotation.Normalize();

                if (!_isStartedCalibration) 
                {
                    StartCoroutine(CalibrationCoroutine());
                }

                if (!_isCalibrationCompleted)
                {
                    return;
                }
                else
                {
                    Quaternion relativeRotation = Quaternion.Inverse(_calibrationReferenceRotation) * _targetRotation;
                    Vector3 r = new Vector3(relativeRotation.x, relativeRotation.y, relativeRotation.z);
                    Vector3 p = Vector3.Project(r, twistAxis);
                    twist = new Quaternion(p.x, p.y, p.z, relativeRotation.w);

                    if (p.sqrMagnitude < float.Epsilon)
                    {
                        twist = Quaternion.identity;
                        swing = relativeRotation;
                    }

                    else
                    {
                        twist.Normalize();
                        swing = relativeRotation * Quaternion.Inverse(twist);
                    }
                }
            }
            else
            {
                return;
            }


            Quaternion adjustedRotation = _mazeReferenceRotation * swing;

            if (IsValid(adjustedRotation))
            {
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

    IEnumerator CalibrationCoroutine()
    {
        if (!_isStartedCalibration)
        {
            JslResetContinuousCalibration(_deviceConnectTest.ActiveDeviceHandle);
            JslStartContinuousCalibration(_deviceConnectTest.ActiveDeviceHandle);
            _isStartedCalibration = true;
        }

        if (!_isCalibrationCompleted)
        {
            yield return new WaitForSeconds(5f);
        }

        if (!_deviceConnectTest.HasActiveDevice)
        {
            yield break;
        }

        if (!_isCalibrationCompleted)
        {
            JslPauseContinuousCalibration(_deviceConnectTest.ActiveDeviceHandle);

            _isCalibrationCompleted = true;

            _calibrationReferenceRotation = _targetRotation;
            _mazeReferenceRotation = _rigidbody.rotation;
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






