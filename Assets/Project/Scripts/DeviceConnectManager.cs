using UnityEngine;
using static JSL;
using TMPro;

/// <summary>
/// デバイスの接続状態ごとの処理を管理するクラス
/// </summary>
public class DeviceConnectManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    private float _timer = 0.0f;
    /// <summary>
    /// デバイスの現在の状態を示すenum
    /// </summary>
    public enum ConnectState
    {
        Disconnected, Selecting, InUse
    }
    private ConnectState _currentConnectState = ConnectState.Disconnected;

    public ConnectState CurrentConnectState
    {
        get
        {
            return _currentConnectState;
        }
    }

    private int _deviceCount = 0; // デバイス数
    private int[] _handles; // デバイスの識別番号を格納する配列
    private int _selectedHandle = -1; // 選択されたデバイスの識別番号
    public int SelectedHandle
    {
        get
        {
            return _selectedHandle;
        }
    }

    private int[] _previousButtonsState; // それぞれのデバイスにおける、前フレームのボタンの状態を保存する配列

    private int _mask = 1 << ButtonMaskE; // SwitchコントローラーのAボタンに対応するマスク値

    private void Start()
    {
        SearchDevice();
    }

    private void Update()
    {
        switch (_currentConnectState)
        {
            case ConnectState.Disconnected:
                _timer += Time.deltaTime;
                if (_timer > 0.5f)
                {
                    SearchDevice();
                }
                break;
            case ConnectState.Selecting:
                SelectDevice();
                break;
            case ConnectState.InUse:
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
            _currentConnectState = ConnectState.Selecting;
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
        bool foundConnectingDevices = false;
        for (int i = 0; i < _handles.Length; i++)
        {
            if(JslStillConnected(_handles[i]))
            {
                foundConnectingDevices = true;
                JOY_SHOCK_STATE inputState = JslGetSimpleState(_handles[i]);
                int inputButtons = inputState.buttons; // デバイスのボタン情報のみを格納

                // Aボタンが押された瞬間を判定
                if ((_previousButtonsState[i] & _mask) == 0 && (inputButtons & _mask) == _mask)
                {
                    _textMeshPro.text = "";
                    _selectedHandle = _handles[i];
                    _currentConnectState = ConnectState.InUse;
                    return;
                }
                _previousButtonsState[i] = inputButtons;
            }
        }

        if(!foundConnectingDevices)
        {
            HandleDisconnection();
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
            HandleDisconnection();
        }
    }

    private void HandleDisconnection()
    {
        _currentConnectState = ConnectState.Disconnected;
        _textMeshPro.text = "デバイスが接続されていません";
        _timer = 0.0f;
        _selectedHandle = -1;
        SearchDevice();
    }

    private void OnDestroy()
    {
        JslDisconnectAndDisposeAll();
    }
}
