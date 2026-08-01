using UnityEngine;
using static JSL;

public class JoyShockTest : MonoBehaviour
{
    [SerializeField] private Transform cubeRoot;
    [SerializeField] private float gyroWeight = 0.98f;

    private float filteredPitch;
    private float filteredRoll;

    private int deviceId = -1;

    private void Start()
    {
        int count = JSL.JslConnectDevices();

        Debug.Log($"接続されたデバイス数 : {count}");

        if (count > 0)
        {
            deviceId = 0;
        }
    }

    private void Update()
    {
        if (deviceId < 0)
            return;

        IMU_STATE imu = JSL.JslGetIMUState(deviceId);

        float pitch = Mathf.Atan2(
            imu.accelZ,
            imu.accelY
        ) * Mathf.Rad2Deg;

        float roll = Mathf.Atan2(
            -imu.accelX,
            imu.accelY
        ) * Mathf.Rad2Deg;

        // 初回だけ現在角度で初期化
        if (filteredPitch == 0f && filteredRoll == 0f)
        {
            filteredPitch = pitch;
            filteredRoll = roll;
        }

        // 加速度だけで平滑化（まずはここまで）
        filteredPitch = Mathf.Lerp(
            filteredPitch,
            pitch,
            (1f - gyroWeight));

        filteredRoll = Mathf.Lerp(
            filteredRoll,
            roll,
            (1f - gyroWeight));

        float clampedPitch = Mathf.Clamp(filteredPitch, -20f, 20f);
        float clampedRoll = Mathf.Clamp(filteredRoll, -20f, 20f);
        // デッドゾーン
        if (Mathf.Abs(clampedPitch) < 2f)
            clampedPitch = 0f;

        if (Mathf.Abs(clampedRoll) < 2f)
            clampedRoll = 0f;

        Quaternion targetRotation = Quaternion.Euler(
            clampedPitch,
            0f,
            -clampedRoll
        );

        cubeRoot.localRotation =
            Quaternion.Lerp(
                cubeRoot.localRotation,
                targetRotation,
                8f * Time.deltaTime);
    }
}