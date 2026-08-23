using UnityEngine;
using static JSL;

public class GyroController : MonoBehaviour
{
    [SerializeField] private DeviceConnectManager _deviceConnectManager;
    [SerializeField] private float _rotateSpeed = 0.5f;
    private Rigidbody _rigidbody;

    private Quaternion _currentRotation = Quaternion.identity;
    private Quaternion _targetRotation = Quaternion.identity;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(_deviceConnectManager._connectState == DeviceConnectManager.ConnectState.inUse)
        {
            int usingHandle = _deviceConnectManager.SelectedHandle;
            MOTION_STATE motion = JslGetMotionState(usingHandle);

            _targetRotation = new Quaternion(motion.quatX, motion.quatY, motion.quatZ, motion.quatW);

            if(IsValid(_targetRotation))
            {
                this._rigidbody.rotation = Quaternion.Slerp(_currentRotation, _targetRotation, _rotateSpeed);
            }
            _currentRotation = this._rigidbody.rotation;
        }
    }

    private bool IsValid(Quaternion quaternion)
    {
        float quaternionLength = quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w;
        return !float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z) && !float.IsNaN(quaternion.w) &&
               !float.IsInfinity(quaternion.x) && !float.IsInfinity(quaternion.y) && !float.IsInfinity(quaternion.z) && !float.IsInfinity(quaternion.w);
    }
}
