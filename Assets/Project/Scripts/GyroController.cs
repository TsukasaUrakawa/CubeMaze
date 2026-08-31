using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using static JSL;

public class GyroController : MonoBehaviour
{
    [SerializeField] private DeviceConnectManager _deviceConnectManager;
    [SerializeField] private float _rotateSpeed = 0.5f;
    private Rigidbody _rigidbody;

    private Quaternion _currentRotation = Quaternion.identity;
    private Quaternion _targetRotation = Quaternion.identity;
    [SerializeField] private float _allowableValue = 0.000001f;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(_deviceConnectManager.CurrentConnectionState == DeviceConnectManager.ConnectionState.InUse)
        {
            int usingHandle = _deviceConnectManager.InUseDeviceHandle;
            MOTION_STATE motion = JslGetMotionState(usingHandle);

            _targetRotation = new Quaternion(motion.quatX, motion.quatY, motion.quatZ, motion.quatW);

            if (IsValid(_targetRotation))
            {
                _targetRotation.Normalize();
                this._rigidbody.rotation = Quaternion.Slerp(_currentRotation, _targetRotation, _rotateSpeed);
                _currentRotation = this._rigidbody.rotation;
            }
        }
    }

    private bool IsValid(Quaternion quaternion)
    {
        if(float.IsNaN(quaternion.x) || float.IsNaN(quaternion.y) || float.IsNaN(quaternion.z) || float.IsNaN(quaternion.w) ||
           float.IsInfinity(quaternion.x) || float.IsInfinity(quaternion.y) || float.IsInfinity(quaternion.z) || float.IsInfinity(quaternion.w))
        {
            return false;
        }

        float quaternionLength = Quaternion.Dot(quaternion, quaternion);

        if(quaternionLength <= _allowableValue)
        {
            return false;
        }

        return true;
    }
}
