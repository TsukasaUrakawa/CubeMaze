using UnityEngine;
using static JSL;
using TMPro;

public class DeviceConnectManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;

    public enum ConnectState
    {
        disconnected, selecting, inUse
    }
    private ConnectState _connectState = ConnectState.disconnected;
    private int[] _handles;
    private int[] _previousButtonsState;
    private int _selectedHandle = -1;
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
            _previousButtonsState = new int[deviceCount];
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
            for (int i = 0; i < _handles.Length; i++)
            {
                JOY_SHOCK_STATE inputState = JslGetSimpleState(_handles[i]);
                int inputButtons = inputState.buttons;
                int mask = 1 << ButtonMaskE;
                if ((inputButtons & mask) == mask)
                {
                    _textMeshPro.text = "スタート！";
                    _selectedHandle = _handles[i];
                    _connectState = ConnectState.inUse;
                }
            }
        }
    }
}
