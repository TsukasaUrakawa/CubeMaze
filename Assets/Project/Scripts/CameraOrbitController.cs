using Unity.Cinemachine;
using UnityEngine;
using static JSL;

public class CameraOrbitController : MonoBehaviour
{
    [SerializeField] DeviceConnectTest _deviceConnectTest;

    [SerializeField] private float _rStickDeadZone = 0.025f;

    private CinemachineOrbitalFollow _orbitalFollow;

    [SerializeField] private float _rotateSpeed = 5f;

    void Awake()
    {
        _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        if (!_deviceConnectTest.HasActiveDevice)
        {
            return;
        }
        else
        {
            JOY_SHOCK_STATE state = JslGetSimpleState(_deviceConnectTest.ActiveDeviceHandle);
            Vector2 rStick = new Vector2(state.stickRX, state.stickRY);
            float magnitude = rStick.magnitude;
            if (magnitude > _rStickDeadZone)
            {
                _orbitalFollow.HorizontalAxis.Value = _orbitalFollow.HorizontalAxis.ClampValue(_orbitalFollow.HorizontalAxis.Value + -rStick.x * _rotateSpeed * Time.deltaTime);
            }
        }
    }
}
