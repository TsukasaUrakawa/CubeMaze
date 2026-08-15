using UnityEngine;
using static JSL;
using TMPro;
using UnityEngine.InputSystem.LowLevel;

public class DeviceConnectManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;

    public enum ConnectState
    {
        disconnected, selecting, inUse
    }
    private ConnectState _connectState = ConnectState.disconnected;
    private int[] _handles;
    private void Start()
    {
        //接続されているデバイスの数を保存
        int deviceCount = JslConnectDevices();
        if (deviceCount == 0)
        {
            _textMeshPro.text = "デバイスが接続されていません";
        }
        if (deviceCount >= 1)
        {
            _connectState = ConnectState.selecting;
            _handles = new int[deviceCount];
            JslGetConnectedDeviceHandles(_handles, _handles.Length);
            _textMeshPro.text = "ボタンを押してください";
        }
    }

    private void Update()
    {
        SelectDevice();
    }

    private void SelectDevice()
    {
        if (_connectState == ConnectState.selecting)
        {
            foreach (int handle in _handles)
            {
                JOY_SHOCK_STATE inputState = JslGetSimpleState(handle);
            }
        }
    }
}
