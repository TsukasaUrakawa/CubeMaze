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
        MOTION_STATE motion = JslGetMotionState(usingHandle);
        //Debug.Log($"加速度１{imu.accelX}, {imu.accelY}, {imu.accelZ}");
        //Debug.Log($"加速度２{motion.accelX}, {motion.accelY}, {motion.accelZ}");
        //Debug.Log($"ジャイロ{imu.gyroX}, {imu.gyroY}, {imu.gyroZ}");
        Debug.Log($"クオータニオン{motion.quatW}, {motion.quatX}, {motion.quatY}, {motion.quatZ}");
        //Debug.Log($"重力方向{motion.gravX}, {motion.gravY}, {motion.gravZ}");
    }
}
