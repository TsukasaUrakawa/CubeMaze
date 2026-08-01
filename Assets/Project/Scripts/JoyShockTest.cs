using UnityEngine;
using static JSL;

public class JoyShockTest : MonoBehaviour
{
    private int deviceId = -1;

    void Start()
    {
        int count = JSL.JslConnectDevices();

        Debug.Log($"接続されたデバイス数 : {count}");

        if (count > 0)
        {
            deviceId = 0;
        }
    }

    void Update()
    {
        if (deviceId < 0)
            return;

        IMU_STATE imu = JSL.JslGetIMUState(deviceId);

        Debug.Log(
            $"Gyro : X={imu.gyroX:F2}  Y={imu.gyroY:F2}  Z={imu.gyroZ:F2}");
    }
}