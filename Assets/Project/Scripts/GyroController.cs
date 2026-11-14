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
    /// <summary>
    /// 観察を適用した時の水平なカメラ方向から、迷路操作の基準を決める。
    /// </summary>
    [SerializeField] private Transform _cameraTransform;

    /// <summary>
    /// 一回の物理更新で傾きを目標へ近づける補間割合。角速度ではない。
    /// </summary>
    [SerializeField] private float _rotateSpeed = 0.3f;

    /// <summary>
    /// 補間済みの傾きと目標傾きの差がこの角度以下なら更新しない（度）。
    /// </summary>
    [SerializeField] private float _rotationDeadZoneDegrees = 0.05f;

    /// <summary>
    /// 復帰中の操作適用を止めるための参照。復帰の進行はRespawnが決める。
    /// </summary>
    [SerializeField] private Respawn _respawn;
    /// <summary>
    /// 迷路全体の姿勢を物理更新で変更するRigidbody。
    /// </summary>
    private Rigidbody _rigidbody;
    /// <summary>
    /// コントローラーから取得した現在の姿勢をUnity用に変換した目標回転
    /// </summary>
    private Quaternion _targetRotation = Quaternion.identity;
    /// <summary>
    /// 現在のキャリブレーション開始からの経過時間（秒）。
    /// </summary>
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
    /// <summary>
    /// 90度回転または基準姿勢へ戻す補間の開始姿勢。
    /// </summary>
    private Quaternion _stepStartRotation = Quaternion.identity;
    /// <summary>
    /// 90度回転または基準姿勢へ戻す補間の到達姿勢。
    /// </summary>
    private Quaternion _stepTargetRotation = Quaternion.identity;

    /// <summary>
    /// カメラを基準に決めた、操作方向
    /// </summary>
    private Quaternion _controllReferenceRotation = Quaternion.identity;
    /// <summary>
    /// 現在の段階回転に費やした時間（秒）。
    /// </summary>
    private float _stepRotationElapsedTime = 0f;
    /// <summary>
    /// 90度回転および基準姿勢へ戻す補間にかける時間（秒）。
    /// </summary>
    [SerializeField] private float _stepRotationDuration = 1f;

    /// <summary>
    /// 前回取得した押下ビット。現在値と比較し、押し始めだけを検出する。
    /// </summary>
    private int _previousButtons = 0;
    // JSLのボタン番号を、一つのビットだけが立つ判定用マスクへ変換する。
    private int _buttonMaskUp = 1 << ButtonMaskUp;
    private int _buttonMaskDown = 1 << ButtonMaskDown;
    private int _buttonMaskLeft = 1 << ButtonMaskLeft;
    private int _buttonMaskRight = 1 << ButtonMaskRight;
    private int _buttonMaskX = 1 << ButtonMaskN;
    private int _buttonMaskY = 1 << ButtonMaskW;

    /// <summary>
    /// 初回測定の基準姿勢の記録。現在の復帰処理では読み出していない。
    /// </summary>
    private Quaternion _initialCalibrationRotation = Quaternion.identity;
    /// <summary>
    /// 初回測定完了時に保存する、復帰用の迷路のワールド姿勢。
    /// </summary>
    private Quaternion _initialMazeRotation = Quaternion.identity;

    /// <summary>
    /// 初回の迷路姿勢を保存済みか。復帰可能かの判定と上書き防止に使う。
    /// </summary>
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
        Suspended
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

    private int _sampleDeviceHandle = -1;
    private bool _hasCurrentSample = false;

    /// <summary>
    /// 同じオブジェクトの迷路用Rigidbodyを取得する。
    /// </summary>
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 使用機器の姿勢・ボタンを取得し、復帰中でなければ操作モードを切り替える。
    /// </summary>
    void Update()
    {
        switch (_deviceConnectManager.CurrentConnectionState)
        {
            case (DeviceConnectManager.ConnectionState.Calibrating):
            case (DeviceConnectManager.ConnectionState.InUse):
                MOTION_STATE motion = JslGetMotionState(_deviceConnectManager.ActiveDeviceHandle); // 使用中のデバイスの識別番号からモーションステートを取得

                _targetRotation = new Quaternion(motion.quatX, -motion.quatY, -motion.quatZ, motion.quatW); // コントローラーのクオータニオンを保存
                JOY_SHOCK_STATE state = JslGetSimpleState(_deviceConnectManager.ActiveDeviceHandle);
                // 復帰中も姿勢取得と押下履歴の更新は続けるが、操作要求は適用しない。
                if (_currentControllState != ControllState.StepRotating && _deviceConnectManager.CurrentConnectionState == DeviceConnectManager.ConnectionState.InUse && !_respawn.IsRespawning)
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
                            // カメラの上下の傾きを除き、観察後の左右・前後操作を水平な視点方向へ合わせる。
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

    /// <summary>
    /// 測定を進め、使用中は操作モードに応じて迷路の姿勢を物理更新へ反映する。
    /// </summary>
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
                        // 初回の測定結果だけを復帰用に保存し、再測定で復帰先の基準を上書きしない。
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
                    // この停止はInUseだけに適用し、再接続後のキャリブレーションは進められるようにする。
                    if (_respawn.IsRespawning)
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
                            // 基準からの相対姿勢を求め、機器を持ち直した姿勢をゼロとして扱う。
                            Quaternion relativeRotation = Quaternion.Inverse(_calibrationReferenceRotation) * _targetRotation;
                            Vector3 twistAxis = Vector3.up;
                            Vector3 r = new Vector3(relativeRotation.x, relativeRotation.y, relativeRotation.z);
                            // Y軸まわりのねじれ成分twistを分離し、残る傾きswingを迷路へ使う。
                            Vector3 p = Vector3.Project(r, twistAxis);
                            Quaternion twist = new Quaternion(p.x, p.y, p.z, relativeRotation.w);
                            Quaternion swing = Quaternion.identity;
                            // ねじれの長さがほぼゼロなら正規化を避け、相対姿勢全体を傾きとして扱う。
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

                        // 操作基準で傾きの軸をワールドへ移し、迷路の基準姿勢へ重ねる。積の順序で結果が変わる。
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

    /// <summary>
    /// 操作基準に合わせた軸で、迷路の基準姿勢を段階的に回す準備をする。
    /// </summary>
    /// <param name="axis">操作基準内での回転軸。</param>
    /// <param name="angle">回転する角度（度）。</param>
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

    /// <summary>
    /// 初回の姿勢が保存済みなら、迷路・操作基準・カメラを復帰用に初期化する。
    /// </summary>
    /// <returns>初期化を実行できた場合はtrue。初回姿勢が未保存ならfalse。</returns>
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
            _currentControllState = ControllState.Suspended;
            _stepRotationElapsedTime = 0f;
            _rigidbody.rotation = _initialMazeRotation;
            return true;
        }
    }

    /// <summary>
    /// 復帰中・機器使用中・有効な姿勢の条件を確認し、現在の姿勢を操作基準にする。
    /// </summary>
    /// <returns>操作再開の準備ができた場合はtrue。ボールの再開と復帰完了は呼び出し元が行う。</returns>
    public bool TryResumeAfterRespawn()
    {
        // 再測定で操作モードが変わっても、復帰進行はRespawnの状態で判定する。
        if (_respawn.IsRespawning && _deviceConnectManager.CurrentConnectionState == DeviceConnectManager.ConnectionState.InUse && IsValid(_targetRotation))
        {
            _calibrationReferenceRotation = _targetRotation.normalized;
            _smoothGyroRotation = Quaternion.identity;
            _currentControllState = ControllState.Normal;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 成分が有限で、長さがほぼゼロではないQuaternionかを調べる。
    /// </summary>
    /// <param name="quaternion">検査する姿勢。</param>
    /// <returns>数値として利用可能ならtrue。接続状態や入力の鮮度は検査しない。</returns>
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
