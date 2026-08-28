using UnityEngine;
using static JSL;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// デバイスの接続状態ごとの処理を管理するクラス
/// </summary>
public class DeviceConnectManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    private float _ElapsedTimer = 0.0f;
    /// <summary>
    /// デバイスの現在の接続状態を示すenum
    /// </summary>
    public enum ConnectionState
    {
        Disconnected, Selecting, InUse
    }
    private ConnectionState _currentConnectionState = ConnectionState.Disconnected;

    public ConnectionState CurrentConnectionState
    {
        get
        {
            return _currentConnectionState;
        }
    }

    private int _connectedDeviceCount = 0; // デバイス数
    private int[] _connectedDeviceHandles; // デバイスの識別番号を格納する配列
    private int _selectedDeviceHandle = -1; // 選択されたデバイスの識別番号
    public int SelectedDeviceHandle
    {
        get
        {
            return _selectedDeviceHandle;
        }
    }

    private Dictionary<int, int> _previousButtonStatesByHandle = new Dictionary<int, int>();

    private int _aButtonMask = 1 << ButtonMaskE; // SwitchコントローラーのAボタンに対応するマスク値

    private void Start()
    {
        SearchDevice();
    }

    private void Update()
    {
        switch (_currentConnectionState)
        {
            case ConnectionState.Disconnected:
                _ElapsedTimer += Time.deltaTime;
                if (_ElapsedTimer > 0.5f)
                {
                    SearchDevice();
                    if (_connectedDeviceCount >= 1 && ChangeState(ConnectionState.Selecting))
                    {
                        _textMeshPro.text = "Aボタンを押してください";
                    }
                    else if(_connectedDeviceCount == 0)
                    {
                        _ElapsedTimer = 0.0f;
                        _textMeshPro.text = "デバイスが接続されていません";
                    }
                }
                break;
            case ConnectionState.Selecting:
                SelectDevice();
                break;
            case ConnectionState.InUse:
                DetectDisconnected();
                break;
        }
    }

    private void SearchDevice()
    {
        _connectedDeviceCount = JslConnectDevices(); // 接続されているデバイスの数を保存
        if (_connectedDeviceCount >= 1)
        {
            int[] detectedDeviceHandles = new int[_connectedDeviceCount];
            JslGetConnectedDeviceHandles(detectedDeviceHandles, detectedDeviceHandles.Length); // 最新の接続中デバイスの識別番号を取得

            // 前回と最新の検索時の識別番号を比較
            foreach (int detectedDeviceHandle in detectedDeviceHandles)
            {
                if (_previousButtonStatesByHandle.ContainsKey(detectedDeviceHandle))
                {
                    continue;
                }

                else
                {
                    JOY_SHOCK_STATE currentInputState = JslGetSimpleState(detectedDeviceHandle);
                    int initialButtonState = currentInputState.buttons;

                    _previousButtonStatesByHandle.Add(detectedDeviceHandle, initialButtonState);
                }
            }
            _connectedDeviceHandles = detectedDeviceHandles;
        }
    }

    private void SelectDevice()
    {
        bool hasConnectedDevice = false;
        foreach(int connectedDeviceHandle in _connectedDeviceHandles)
        {
            if(JslStillConnected(connectedDeviceHandle))
            {
                hasConnectedDevice = true;

                if(_previousButtonStatesByHandle.ContainsKey(connectedDeviceHandle))
                {
                    int previousButtonState = _previousButtonStatesByHandle[connectedDeviceHandle];

                    JOY_SHOCK_STATE currentInputState = JslGetSimpleState(connectedDeviceHandle);
                    int currentButtonState = currentInputState.buttons;
                    // Aボタンが押された瞬間を判定
                    if ((previousButtonState & _aButtonMask) == 0 && (currentButtonState & _aButtonMask) == _aButtonMask)
                    {
                        ChangeState(ConnectionState.InUse);
                        _textMeshPro.text = "";
                        _selectedDeviceHandle = connectedDeviceHandle;
                        return;
                    }

                    _previousButtonStatesByHandle[connectedDeviceHandle] = currentButtonState;
                }
            }
        }

        if(!hasConnectedDevice)
        {
            HandleDisconnection();
        }
    }

    private void DetectDisconnected()
    {
        bool isSelectedDeviceConnected = JslStillConnected(_selectedDeviceHandle);
        if (isSelectedDeviceConnected)
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
        if(ChangeState(ConnectionState.Disconnected))
        {
            _textMeshPro.text = "デバイスが接続されていません";
            _ElapsedTimer = 0.0f;
            _selectedDeviceHandle = -1;
            SearchDevice();
        }
    }

    /// <summary>
    /// 状態を遷移するメソッド
    /// </summary>
    /// <param name="nextState">遷移先の接続状態</param>
    /// <returns>状態遷移に成功した場合はtrue、許可されていない遷移の場合はfalse</returns>
    private bool ChangeState(ConnectionState nextState)
    {
        switch(_currentConnectionState, nextState)
        {
            case (ConnectionState.Disconnected, ConnectionState.Selecting):
            case (ConnectionState.Selecting, ConnectionState.Disconnected):
            case (ConnectionState.Selecting, ConnectionState.InUse):
            case (ConnectionState.InUse, ConnectionState.Disconnected):
            case (ConnectionState.InUse, ConnectionState.Selecting):
                {
                    _currentConnectionState = nextState;
                    return true;
                }
            default:
                return false;
        }
    }

    private void OnDestroy()
    {
        JslDisconnectAndDisposeAll();
    }
}
