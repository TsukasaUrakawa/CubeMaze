using UnityEngine;
using static JSL;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// デバイスの接続状態ごとの処理を管理する
/// </summary>
public class DeviceConnectManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    private float _searchDevicesElapsedTimer = 0.0f;
    /// <summary>
    /// デバイスの現在の接続状態を示す
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

    private void Update()
    {
        switch (_currentConnectionState)
        {
            case ConnectionState.Disconnected:
                _searchDevicesElapsedTimer += Time.deltaTime;
                if (_searchDevicesElapsedTimer > 0.5f)
                {
                    SearchDevice();
                    _searchDevicesElapsedTimer = 0.0f;
                    if (_connectedDeviceCount >= 1 && ChangeState(ConnectionState.Selecting))
                    {
                        _textMeshPro.text = "Aボタンを押してください";
                    }
                    else if(_connectedDeviceCount == 0)
                    {
                        _textMeshPro.text = "デバイスが接続されていません";
                    }
                }
                break;
            case ConnectionState.Selecting:
                _searchDevicesElapsedTimer += Time.deltaTime;
                if (_searchDevicesElapsedTimer > 0.5f)
                {
                    SearchDevice();
                    _searchDevicesElapsedTimer = 0.0f;
                    if (_connectedDeviceCount >= 1)
                    {
                        _textMeshPro.text = "Aボタンを押してください";
                    }
                    else
                    {
                        _textMeshPro.text = "デバイスが接続されていません";
                    }
                }
                SelectDevice();
                break;
            case ConnectionState.InUse:
                DetectDisconnected();
                break;
        }
    }

    /// <summary>
    /// 接続されているデバイスを検索する
    /// </summary>
    private void SearchDevice()
    {
        _connectedDeviceCount = JslConnectDevices(); // 認識したデバイスの数を保存
        if (_connectedDeviceCount >= 1)
        {
            int[] detectedDeviceHandles = new int[_connectedDeviceCount];
            JslGetConnectedDeviceHandles(detectedDeviceHandles, detectedDeviceHandles.Length); // 認識したデバイスの識別番号を取得

            IEnumerable<int> onlyRegisteredDictionaryHandles = _previousButtonStatesByHandle.Keys.Except(detectedDeviceHandles);
            int[] disconnectedHandles = onlyRegisteredDictionaryHandles.ToArray();
            foreach (int disconnectedHandle in disconnectedHandles)
            {
                _previousButtonStatesByHandle.Remove(disconnectedHandle);
            }

            // 前回と今回の検索時で識別番号を比較
            foreach (int detectedDeviceHandle in detectedDeviceHandles)
            {
                if (_previousButtonStatesByHandle.ContainsKey(detectedDeviceHandle))
                {
                    continue;
                }

                // 前回までの検索時にないデバイスは追加する
                else
                {
                    JOY_SHOCK_STATE currentInputState = JslGetSimpleState(detectedDeviceHandle);
                    int initialButtonState = currentInputState.buttons;

                    _previousButtonStatesByHandle.Add(detectedDeviceHandle, initialButtonState);
                }
            }
            _connectedDeviceHandles = detectedDeviceHandles; // 認識済みデバイスの識別番号を接続済みデバイスの識別番号として保存
        }
        else
        {
            int[] emptyArray = Array.Empty<int>();
            _connectedDeviceHandles = emptyArray;
            _previousButtonStatesByHandle.Clear();
        }
    }

    /// <summary>
    /// 接続されている複数のデバイスの中から使用するデバイスを選択する
    /// </summary>
    private void SelectDevice()
    {
        bool hasConnectedDevice = false; // 接続し続けているか判定する

        foreach (int connectedDeviceHandle in _connectedDeviceHandles)
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

    /// <summary>
    /// デバイスの切断を検知する
    /// </summary>
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

    /// <summary>
    /// 切断時の処理をする
    /// </summary>
    private void HandleDisconnection()
    {
        if(ChangeState(ConnectionState.Disconnected))
        {
            _textMeshPro.text = "デバイスが接続されていません";
            _searchDevicesElapsedTimer = 0.0f;
            _previousButtonStatesByHandle.Clear();
            _selectedDeviceHandle = -1;
        }
    }

    /// <summary>
    /// 状態を遷移する
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

    /// <summary>
    /// ゲーム終了時にリソースを解放する
    /// </summary>
    private void OnDestroy()
    {
        JslDisconnectAndDisposeAll();
    }
}
