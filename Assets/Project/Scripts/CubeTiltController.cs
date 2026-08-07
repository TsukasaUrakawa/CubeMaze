using UnityEngine;
using static JSL;

public class CubeTiltController : MonoBehaviour
{
    [Header("回転設定")]
    [SerializeField] private float maxTiltAngle = 20f; // 最大傾斜角
    [SerializeField] private float rotationSpeed = 5f; // 回転速度
    [SerializeField] private float filterStrength = 0.02f; // フィルタの強さ
    [SerializeField] private float deadZone = 2f; // デッドゾーンの範囲

    private const int MaxDeviceCount = 16; // 最大接続デバイス数
    private int deviceId = -1;

    private float filteredPitch;
    private float filteredRoll;

    private Quaternion targetRotation = Quaternion.identity;

    private void Start()
    {

        InitializeDevice();
    }

    private void Update()
    {
        if (deviceId < 0)
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
        int[] handles = new int[MaxDeviceCount];
        // 取得したデバイス数を保存
        int count = JSL.JslGetConnectedDeviceHandles(handles, handles.Length);

        Debug.Log($"接続されたデバイス数 : {count}");

        if (count == 0)
        {
            Debug.LogError("デバイスが接続されていません。");
            return;
        }

        // 最初のデバイスを使用
        deviceId = handles[0];

        Debug.Log($"使用するデバイスID : {deviceId}");
    }

    private void UpdateTilt()
    {
        //　使用しているデバイスのIMU(Inertial Measurement Unit)情報を保存
        IMU_STATE imu = JSL.JslGetIMUState(deviceId);

        // 加速度センサーの値からPitch(前後)とRoll(左右)の傾斜角を計算
        float pitch = Mathf.Atan2(imu.accelZ, imu.accelY) * Mathf.Rad2Deg;
        float roll = Mathf.Atan2(-imu.accelX, imu.accelY) * Mathf.Rad2Deg;

        // pitch と roll の値をフィルタリングして滑らかにする
        filteredPitch = Mathf.Lerp(filteredPitch, pitch, filterStrength);
        filteredRoll = Mathf.Lerp(filteredRoll, roll, filterStrength);

        // 最大傾斜角と最小傾斜角内に制限する
        float targetX = Mathf.Clamp(filteredPitch, -maxTiltAngle, maxTiltAngle);
        float targetZ = Mathf.Clamp(-filteredRoll, -maxTiltAngle, maxTiltAngle);

        // デッドゾーンの適用
        if (Mathf.Abs(targetX) < deadZone)
            targetX = 0f;

        if (Mathf.Abs(targetZ) < deadZone)
            targetZ = 0f;

        // 目標の回転角をQuaternionに変換
        targetRotation = Quaternion.Euler(targetX, 0f, targetZ);
    }

    private void RotateCube()
    {
        float angleDifference = Quaternion.Angle(transform.localRotation, targetRotation);
        float speed = angleDifference * rotationSpeed;

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            speed * Time.fixedDeltaTime);
    }
}
