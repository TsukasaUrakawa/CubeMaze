using UnityEngine;
using static JSL;
using TMPro;
using System.Collections;

public class DeviceConnectManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    private float _timer = 0.0f;
    /// <summary>
    /// デバイスの現在の状態を示すenum
    /// </summary>
    enum ConnectState
    {
        disconnected, selecting, inUse
    }
    private ConnectState _connectState = ConnectState.disconnected;

    private int _deviceCount = 0;
    private int[] _handles; // デバイスの識別番号を格納する配列
    private int _selectedHandle = -1; // 選択されたデバイスの識別番号

    private int[] _previousButtonsState; // それぞれのデバイスにおける、前回のボタンの状態を保存する配列

    private int _mask = 1 << ButtonMaskE; // SwitchコントローラーのAボタンに対応するマスク値

    private void Start()
    {
        SearchDevice();
    }

    private void Update()
    {
        switch (_connectState)
        {
            case ConnectState.disconnected:
                _timer += Time.deltaTime;
                if (_timer > 0.5f)
                {
                    SearchDevice();
                }
                break;
            case ConnectState.selecting:
                SelectDevice();
                break;
            case ConnectState.inUse:
                DetectDisconnected();
                break;
        }
    }

    private void SearchDevice()
    {
        _deviceCount = JslConnectDevices(); // 接続されているデバイスの数を保存
        if (_deviceCount >= 1)
        {
            _handles = new int[_deviceCount];
            _previousButtonsState = new int[_deviceCount];
            JslGetConnectedDeviceHandles(_handles, _handles.Length); // 接続中デバイスの識別番号を_handlesに格納
            _connectState = ConnectState.selecting; // 選択中に移行
            _textMeshPro.text = "Aボタンを押してください";
        }
        else
        {
            _timer = 0.0f;
            _textMeshPro.text = "デバイスが接続されていません";
        }
    }

    private void SelectDevice()
    {
        for (int i = 0; i < _handles.Length; i++)
        {
            JOY_SHOCK_STATE inputState = JslGetSimpleState(_handles[i]);
            int inputButtons = inputState.buttons; // デバイスのボタン情報のみを格納

            // Aボタンが押された瞬間を判定
            if ((_previousButtonsState[i] & _mask) == 0 && (inputButtons & _mask) == _mask)
                {
                    _textMeshPro.text = "";
                    _selectedHandle = _handles[i];
                    _connectState = ConnectState.inUse;
                    return;
                }
                _previousButtonsState[i] = inputButtons;
            }
    }

    private void DetectDisconnected()
    {
        bool stillConnected = JslStillConnected(_selectedHandle);
        if (stillConnected)
        {
            return;
        }
        else
        {
            _textMeshPro.text = "デバイスが接続されていません";
            _connectState = ConnectState.disconnected;
            _timer = 0.0f;
            _selectedHandle = -1;
        }
    }
}
