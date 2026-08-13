using TMPro;
using UnityEngine;
using static JSL;

public class CubeTiltController : MonoBehaviour
{
    [Header("回転設定")]
    [SerializeField] private float _maxTiltAngle = 20f; // 最大傾斜角
    [SerializeField] private float _rotationSpeed = 5f; // 回転速度
    [SerializeField] private float _filterStrength = 0.02f; // フィルタの強さ
    [SerializeField] private float _deadZone = 2f; // デッドゾーンの範囲
    [SerializeField] private float _maxRotationSpeed = 15f; // 最大回転速度

    [SerializeField] private float _rotationAcceleration = 20f;
    private float _currentRotationSpeed;

    private const int _maxDeviceCount = 16; // 最大接続デバイス数
    private int _deviceId = -1;

    private float _filteredPitch;
    private float _filteredRoll;

    private Quaternion _baseRotation = Quaternion.identity;
    private Quaternion _targetRotation = Quaternion.identity;

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _baseRotation = _rb.rotation;
        _targetRotation = _baseRotation;

        InitializeDevice();
    }

    private void Update()
    {
        if (_deviceId < 0)
        {
            return;
        }
        
        UpdateTilt();
    }

    private void FixedUpdate()
    {
        RotateCube();
    }

    /// <summary>
    /// JoyShockLibraryで接続されているデバイスを検索し、使用するデバイスを初期化する
    /// </summary>
    private void InitializeDevice()
    {
        // デバイスを検索
        JSL.JslConnectDevices();

        // デバイスIDを取得
        int[] handles = new int[_maxDeviceCount];
        // 取得したデバイス数を保存
        int count = JSL.JslGetConnectedDeviceHandles(handles, handles.Length);

        Debug.Log($"接続されたデバイス数 : {count}");

        if (count == 0)
        {
            Debug.LogError("デバイスが接続されていません。");
            return;
        }

        // 最初のデバイスを使用
        _deviceId = handles[0];

        Debug.Log($"使用するデバイスID : {_deviceId}");
    }

    private void UpdateTilt()
    {
        //　使用しているデバイスのIMU(Inertial Measurement Unit)情報を保存
        IMU_STATE imu = JSL.JslGetIMUState(_deviceId);

        // 加速度センサーの値からPitch(前後)とRoll(左右)の傾斜角を計算
        float pitch = Mathf.Atan2(imu.accelZ, imu.accelY) * Mathf.Rad2Deg;
        float roll = Mathf.Atan2(-imu.accelX, imu.accelY) * Mathf.Rad2Deg;

        // pitch と roll の値をフィルタリングして滑らかにする
        _filteredPitch = Mathf.Lerp(_filteredPitch, pitch, _filterStrength);
        _filteredRoll = Mathf.Lerp(_filteredRoll, roll, _filterStrength);

        // 最大傾斜角と最小傾斜角内に制限する
        float targetX = Mathf.Clamp(_filteredPitch, -_maxTiltAngle, _maxTiltAngle);
        float targetZ = Mathf.Clamp(-_filteredRoll, -_maxTiltAngle, _maxTiltAngle);

        // デッドゾーンの適用
        if (Mathf.Abs(targetX) < _deadZone)
            targetX = 0f;

        if (Mathf.Abs(targetZ) < _deadZone)
            targetZ = 0f;

        // 目標の回転角をQuaternionに変換
        Quaternion tiltRotation = Quaternion.Euler(targetX, 0f, targetZ);

        _targetRotation = _baseRotation * tiltRotation;
    }

    private void RotateCube()
    {
        float angleDifference = Quaternion.Angle(_rb.rotation, _targetRotation);
        float targetSpeed = angleDifference * _rotationSpeed;

        targetSpeed = Mathf.Min(targetSpeed, _maxRotationSpeed);

        _currentRotationSpeed = Mathf.MoveTowards(
            _currentRotationSpeed,
            targetSpeed,
            _rotationAcceleration * Time.fixedDeltaTime
            );

        Quaternion nextRotation = Quaternion.RotateTowards(
            _rb.rotation,
            _targetRotation,
            _currentRotationSpeed * Time.fixedDeltaTime
        );

        _rb.MoveRotation(nextRotation);
    }
}
