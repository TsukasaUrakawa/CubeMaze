using UnityEngine;
using static JSL;

public class DeviceConnectTest : MonoBehaviour
{
    int _detectedDeviceCount = 0;

    int _activeDeviceHandle = -1;

    public int ActiveDeviceHandle
    {
        get
        {
            return _activeDeviceHandle;
        }
    }


    public bool HasActiveDevice
    {
        get
        {
            return _activeDeviceHandle != -1 && JslStillConnected(_activeDeviceHandle);
        }
    }

    void Start()
    {
        _detectedDeviceCount = JslConnectDevices();
        if (_detectedDeviceCount > 0)
        {

            int[] deviceHandles = new int[_detectedDeviceCount];
            JslGetConnectedDeviceHandles(deviceHandles, _detectedDeviceCount);
            _activeDeviceHandle = deviceHandles[0];
        }
    }

    private void OnDestroy()
    {
        JslDisconnectAndDisposeAll();
    }
}
