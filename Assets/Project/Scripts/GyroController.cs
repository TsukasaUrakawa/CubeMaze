using UnityEngine;
using static JSL;

public class GyroController : MonoBehaviour
{
    [SerializeField] private DeviceConnectManager _deviceConnectManager;
    void Start()
    {
        
    }

    void Update()
    {
        int usingHandle = _deviceConnectManager.SelectedHandle;
        IMU_STATE imu = JslGetIMUState(usingHandle);
        JslGetAndFlushAccumulatedGyro(usingHandle, ref imu.gyroX, ref imu.gyroY, ref imu.gyroZ);
    }
}
