using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static JSL;

/// <summary>
/// 使用中のコントローラーから姿勢情報を取得し、Rigidbodyの回転に反映
/// </summary>
public class GyroController : MonoBehaviour
{
    /// <summary>
    /// デバイス接続クラスの参照
    /// </summary>
    [SerializeField] private DeviceConnectManager _deviceConnectManager;
    /// <summary>
    /// カメラ旋回クラスの参照
    /// </summary>
    [SerializeField] private CameraOrbitController _cameraOrbitController;
    [SerializeField] private Transform _cameraTransform;

    /// <summary>
    /// 現在の回転からコントローラーの姿勢へ補間するときの割合
    /// </summary>
    [SerializeField] private float _rotateSpeed = 0.3f;

    [SerializeField] private float _rotationDeadZoneDegrees = 0.05f;
    private Rigidbody _rigidbody;
    /// <summary>
    /// コントローラーから取得した現在の姿勢をUnity用に変換した目標回転
    /// </summary>
    private Quaternion _targetRotation = Quaternion.identity;
    private float _calibrationElapsedTime = 0f;

    /// <summary>
    /// コントローラーの傾きをゼロとする姿勢
    /// </summary>
    private Quaternion _calibrationReferenceRotation = Quaternion.identity;
    /// <summary>
    /// ジャイロの傾きを加える前の迷路の基準姿勢
    /// </summary>
    private Quaternion _mazeReferenceRotation = Quaternion.identity;
    /// <summary>
    /// 急激な変化を滑らかにした、ジャイロによる傾き
    /// </summary>
    private Quaternion _smoothGyroRotation = Quaternion.identity;
    private Quaternion _stepStartRotation = Quaternion.identity;
    private Quaternion _stepTargetRotation = Quaternion.identity;

    /// <summary>
    /// カメラを基準に決めた、操作方向
    /// </summary>
    private Quaternion _controllReferenceRotation = Quaternion.identity;
    private float _stepRotationElapsedTime = 0f;
    [SerializeField] private float _stepRotationDuration = 1f;

    private int _previousButtons = 0;
    private int _buttonMaskUp = 1 << ButtonMaskUp;
    private int _buttonMaskDown = 1 << ButtonMaskDown;
    private int _buttonMaskLeft = 1 << ButtonMaskLeft;
    private int _buttonMaskRight = 1 << ButtonMaskRight;
    private int _buttonMaskX = 1 << ButtonMaskN;
    private int _buttonMaskY = 1 << ButtonMaskW;

    private Quaternion _initialCalibrationRotation = Quaternion.identity;
    private Quaternion _initialMazeRotation = Quaternion.identity;

    private bool _isSavedInitialRotation = false;

    /// <summary>
    /// 迷路の操作状態を表すenum
    /// </summary>
    public enum ControllState
    {
        /// <summary>
        /// ジャイロと十字ボタンによる通常操作
        /// </summary>
        Normal,
        /// <summary>
        /// ジャイロのキャリブレーションを実行し、基準姿勢を決定
        /// </summary>
        MesuringReference,
        /// <summary>
        /// 十字ボタンで90度回転実行する
        /// </summary>
        StepRotating,
        /// <summary>
        /// 迷路の傾きを基準姿勢に戻す
        /// </summary>
        ReturningToReference,
        /// <summary>
        /// カメラでボールの周り観察する
        /// </summary>
        Viewing,
        /// <summary>
        /// リスポーン直後の操作を一時停止する
        /// </summary>
        WaitingForRespawn
    }

    /// <summary>
    /// 現在の迷路の操作状態
    /// </summary>
    private ControllState _currentControllState = ControllState.Normal;
    /// <summary>
    /// 現在の迷路の操作状態プロパティ
    /// </summary>
    public ControllState CurrentControllState
    {
        get
        {
            return _currentControllState;
        }
    }

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

                _targetRotation = new Quaternion(motion.quatX, -motion.quatY, -motion.quatZ, motion.quatW); // コントローラーのクオータニオンを保存
                JOY_SHOCK_STATE state = JslGetSimpleState(_deviceConnectManager.ActiveDeviceHandle);
                if (_currentControllState != ControllState.StepRotating && _deviceConnectManager.CurrentConnectionState == DeviceConnectManager.ConnectionState.InUse)
                {
                    if (_currentControllState == ControllState.Normal)
                    {
                        if ((state.buttons & _buttonMaskX) != 0 && (_previousButtons & _buttonMaskX) == 0)
                        {
                            _stepStartRotation = _rigidbody.rotation;
                            _stepTargetRotation = _mazeReferenceRotation;
                            _stepRotationElapsedTime = 0f;
                            _currentControllState = ControllState.ReturningToReference;
                        }
                        else
                        {
                            if ((state.buttons & _buttonMaskUp) != 0 && (_previousButtons & _buttonMaskUp) == 0)
                            {
                                RotateMazeReference(Vector3.right, 90f);
                            }
                            if ((state.buttons & _buttonMaskDown) != 0 && (_previousButtons & _buttonMaskDown) == 0)
                            {
                                RotateMazeReference(Vector3.right, -90f);
                            }
                            if ((state.buttons & _buttonMaskLeft) != 0 && (_previousButtons & _buttonMaskLeft) == 0)
                            {
                                RotateMazeReference(Vector3.forward, 90f);
                            }
                            if ((state.buttons & _buttonMaskRight) != 0 && (_previousButtons & _buttonMaskRight) == 0)
                            {
                                RotateMazeReference(Vector3.forward, -90f);
                            }
                        }
                    }
                    else
                    {
                        if (_currentControllState == ControllState.Viewing && !_cameraOrbitController.IsRotating && (state.buttons & _buttonMaskY) != 0 && (_previousButtons & _buttonMaskY) == 0)
                        {
                            Vector3 horizontalForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up);
                            if (horizontalForward.sqrMagnitude > 0.000001f && IsValid(_targetRotation))
                            {
                                _controllReferenceRotation = Quaternion.LookRotation(horizontalForward, Vector3.up);
                                _targetRotation.Normalize();
                                _calibrationReferenceRotation = _targetRotation;
                                _smoothGyroRotation = Quaternion.identity;
                                _currentControllState = ControllState.Normal;
                            }
                        }
                    }
                }
                _previousButtons = state.buttons;
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

                    if (_currentControllState != ControllState.MesuringReference)
                    {
                        _calibrationElapsedTime = 0f;
                        JslResetContinuousCalibration(_deviceConnectManager.ActiveDeviceHandle);
                        JslStartContinuousCalibration(_deviceConnectManager.ActiveDeviceHandle);
                        _currentControllState = ControllState.MesuringReference;
                    }

                    _calibrationElapsedTime += Time.fixedDeltaTime;

                    if (_calibrationElapsedTime > 5f)
                    {
                        JslPauseContinuousCalibration(_deviceConnectManager.ActiveDeviceHandle);
                        _calibrationReferenceRotation = _targetRotation;
                        _mazeReferenceRotation = _rigidbody.rotation;
                        _smoothGyroRotation = Quaternion.identity;
                        _currentControllState = ControllState.Normal;
                        _deviceConnectManager.CompleteCalibration();
                        if (!_isSavedInitialRotation)
                        {
                            _initialCalibrationRotation = _calibrationReferenceRotation;
                            _initialMazeRotation = _mazeReferenceRotation;
                            _isSavedInitialRotation = true;
                        }
                    }
                }
                break;
            case (DeviceConnectManager.ConnectionState.InUse):
                {
                    if (_currentControllState == ControllState.WaitingForRespawn)
                    {
                        return;
                    }
                    if (_currentControllState == ControllState.ReturningToReference)
                    {
                        _stepRotationElapsedTime += Time.fixedDeltaTime;
                        float progress = _stepRotationElapsedTime / _stepRotationDuration;
                        _rigidbody.MoveRotation(Quaternion.Slerp(_stepStartRotation, _stepTargetRotation, progress));
                        if (progress >= 1)
                        {
                            _smoothGyroRotation = Quaternion.identity;
                            _currentControllState = ControllState.Viewing;
                        }
                        return;
                    }
                    if (IsValid(_targetRotation))
                    {
                        _targetRotation.Normalize();
                        if (_currentControllState == ControllState.StepRotating)
                        {
                            _stepRotationElapsedTime += Time.fixedDeltaTime;
                            float progress = _stepRotationElapsedTime / _stepRotationDuration;
                            _mazeReferenceRotation = Quaternion.Slerp(_stepStartRotation, _stepTargetRotation, progress);
                            if (progress >= 1)
                            {
                                _mazeReferenceRotation = _stepTargetRotation;
                                _calibrationReferenceRotation = _targetRotation;
                                _currentControllState = ControllState.Normal;
                            }
                        }
                        else if (_currentControllState == ControllState.Normal)
                        {
                            Quaternion relativeRotation = Quaternion.Inverse(_calibrationReferenceRotation) * _targetRotation;
                            Vector3 twistAxis = Vector3.up;
                            Vector3 r = new Vector3(relativeRotation.x, relativeRotation.y, relativeRotation.z);
                            Vector3 p = Vector3.Project(r, twistAxis);
                            Quaternion twist = new Quaternion(p.x, p.y, p.z, relativeRotation.w);
                            Quaternion swing = Quaternion.identity;
                            if (Quaternion.Dot(twist, twist) < float.Epsilon)
                            {
                                twist = Quaternion.identity;
                                swing = relativeRotation;
                            }
                            else
                            {
                                twist.Normalize();
                                swing = relativeRotation * Quaternion.Inverse(twist);
                            }
                            if (Quaternion.Angle(_smoothGyroRotation, swing) > _rotationDeadZoneDegrees)
                            {
                                _smoothGyroRotation = Quaternion.Slerp(_smoothGyroRotation, swing, _rotateSpeed);
                            }
                        }

                        Quaternion worldGyroRotation = _controllReferenceRotation * _smoothGyroRotation * Quaternion.Inverse(_controllReferenceRotation);
                        Quaternion adjustedRotation = worldGyroRotation * _mazeReferenceRotation;
                        if (IsValid(adjustedRotation))
                        {
                            this._rigidbody.MoveRotation(adjustedRotation);
                        }
                    }
                }
                break;
            default:
                _currentControllState = ControllState.Normal;
                _calibrationElapsedTime = 0f;
                _stepRotationElapsedTime = 0f;
                break;
        }
    }

    private void RotateMazeReference(Vector3 axis, float angle)
    {
        if (_currentControllState == ControllState.StepRotating)
        {
            return;
        }
        Vector3 worldAxis = _controllReferenceRotation * axis;
        _stepStartRotation = _rigidbody.rotation;
        _stepTargetRotation = Quaternion.AngleAxis(angle, worldAxis) * _mazeReferenceRotation;
        _smoothGyroRotation = Quaternion.identity;
        _stepRotationElapsedTime = 0f;
        _currentControllState = ControllState.StepRotating;
    }

    public bool ResetForRespawn()
    {
        if (!_isSavedInitialRotation)
        {
            return false;
        }
        else
        {
            _calibrationReferenceRotation = _targetRotation;
            _mazeReferenceRotation = _initialMazeRotation;
            _cameraOrbitController.ResetCameraForRespawn();
            _smoothGyroRotation = Quaternion.identity;
            _controllReferenceRotation = Quaternion.identity;
            _currentControllState = ControllState.WaitingForRespawn;
            _stepRotationElapsedTime = 0f;
            _rigidbody.rotation = _initialMazeRotation;
            return true;
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
