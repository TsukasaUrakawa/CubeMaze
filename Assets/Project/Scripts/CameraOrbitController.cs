using Unity.Cinemachine;
using UnityEngine;
using static JSL;

public class CameraOrbitController : MonoBehaviour
{
    [SerializeField] private DeviceConnectManager _deviceConnectManager;
    [SerializeField] private GyroController _gyroController;

    private CinemachineOrbitalFollow _orbitalFollow;

    private int _previousButtons = 0;
    private int _buttonMaskL = 1 << ButtonMaskL;
    private int _buttonMaskR = 1 << ButtonMaskR;

    private bool _isRotating = false;
    public bool IsRotating
    {
        get
        {
            return _isRotating;
        }
    }
    private float _startAngle = 0f;
    private float _targetAngle = 0f;
    private float _rotationElapsedTime = 0f;
    [SerializeField, Range(1f, 3f)] private float _rotationDuration = 1f;

    private float _initialHorizontalAngle;

    void Awake()
    {
        _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        _initialHorizontalAngle = _orbitalFollow.HorizontalAxis.Value;
    }

    void Update()
    {
        if (_deviceConnectManager.CurrentConnectionState != DeviceConnectManager.ConnectionState.InUse)
        {
            return;
        }
        else
        {
            JOY_SHOCK_STATE state = JslGetSimpleState(_deviceConnectManager.ActiveDeviceHandle);
            if (_gyroController.IsViewing && !_gyroController.IsReturningToReference)
            {
                if ((state.buttons & _buttonMaskL) != 0 && (_previousButtons & _buttonMaskL) == 0)
                {
                    StartRotation(90f);
                }
                else if ((state.buttons & _buttonMaskR) != 0 && (_previousButtons & _buttonMaskR) == 0)
                {
                    StartRotation(-90f);
                }
            }
            _previousButtons = state.buttons;
            if (_isRotating)
            {
                _rotationElapsedTime += Time.deltaTime;
                float progress = _rotationElapsedTime / _rotationDuration;
                float currentAngle = Mathf.Lerp(_startAngle, _targetAngle, progress);
                _orbitalFollow.HorizontalAxis.Value = _orbitalFollow.HorizontalAxis.ClampValue(currentAngle);
                if (progress >= 1)
                {
                    _isRotating = false;
                }
            }
        }
    }

    private void StartRotation(float angle)
    {
        if (_isRotating)
        {
            return;
        }
        _startAngle = _orbitalFollow.HorizontalAxis.Value;
        _targetAngle = _startAngle + angle;
        _rotationElapsedTime = 0f;
        _isRotating = true;
    }

    public void ResetCameraForRespawn()
    {
        _isRotating = false;
        _rotationElapsedTime = 0f;
        _orbitalFollow.HorizontalAxis.Value = _initialHorizontalAngle;
    }
}
