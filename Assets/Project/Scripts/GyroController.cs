using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static JSL;

public class GyroController : MonoBehaviour
{
    [SerializeField] private DeviceConnectManager _deviceConnectManager;
    private Rigidbody _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        int usingHandle = _deviceConnectManager.SelectedHandle;

        IMU_STATE imu = JslGetIMUState(usingHandle);
        MOTION_STATE motion = JslGetMotionState(usingHandle);
    }
}
