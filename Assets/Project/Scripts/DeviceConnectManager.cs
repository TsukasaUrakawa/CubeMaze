using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static JSL;

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
        Preparing, Searching, Selecting, InUse
    }
    private ConnectionState _currentConnectionState = ConnectionState.Preparing;

    public ConnectionState CurrentConnectionState
    {
        get
        {
            return _currentConnectionState;
        }
    }

    /// <summary>
    /// ConnectionStateがPreparingの時の理由を示す
    /// </summary>
    public enum PreparingState
    {
        InitialConnect, FoundNoDevices, DisconnectionInSelect, DisconnectionInUse
    }

    private PreparingState _currentPreparingState = PreparingState.InitialConnect;

    public PreparingState CurrentPreparingState
    {
        get
        {
            return _currentPreparingState;
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

    public event Action<IReadOnlyList<int>> SelectionCandidatesPrepared;

    private void Update()
    {
        switch (_currentConnectionState)
        {
            case ConnectionState.Preparing:
                ShowMessage();
                break;
            case ConnectionState.Searching:
                _searchDevicesElapsedTimer += Time.deltaTime;
                if (_searchDevicesElapsedTimer > 0.5f)
                {
                    SearchDevice();
                    _searchDevicesElapsedTimer = 0.0f;
                    if (_connectedDeviceCount >= 1 && ChangeConnectionState(ConnectionState.Selecting))
                    {
                        // 選択候補の準備が完了したことのイベント通知
                        SelectionCandidatesPrepared?.Invoke(_connectedDeviceHandles);
                        _textMeshPro.text = "";
                    }
                    else if (_connectedDeviceCount == 0)
                    {
                        ChangePreparingState(PreparingState.FoundNoDevices);
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
            _connectedDeviceHandles = detectedDeviceHandles; // 認識済みデバイスの識別番号を接続済みデバイスの識別番号として保存
        }
        else
        {
            int[] emptyArray = Array.Empty<int>();
            _connectedDeviceHandles = emptyArray;
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
            // 接続確認
            if (JslStillConnected(connectedDeviceHandle))
            {
                hasConnectedDevice = true;
            }
        }

        if (!hasConnectedDevice)
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
        switch (_currentConnectionState)
        {
            case (ConnectionState.Selecting):
                {
                    ChangePreparingState(PreparingState.DisconnectionInSelect);
                }
                break;
            case (ConnectionState.InUse):
                {
                    ChangePreparingState(PreparingState.DisconnectionInUse);
                }
                break;
        }
        _searchDevicesElapsedTimer = 0.0f;
        _selectedDeviceHandle = -1;
    }

    /// <summary>
    /// 状態を遷移する
    /// </summary>
    /// <param name="nextState">遷移先の接続状態</param>
    /// <returns>状態遷移に成功した場合はtrue、許可されていない遷移の場合はfalse</returns>
    private bool ChangeConnectionState(ConnectionState nextState)
    {
        switch (_currentConnectionState, nextState)
        {
            case (ConnectionState.Preparing, ConnectionState.Searching):
            case (ConnectionState.Searching, ConnectionState.Selecting):
            case (ConnectionState.Searching, ConnectionState.Preparing):
            case (ConnectionState.Selecting, ConnectionState.InUse):
            case (ConnectionState.Selecting, ConnectionState.Preparing):
            case (ConnectionState.InUse, ConnectionState.Preparing):
                {
                    _currentConnectionState = nextState;
                    return true;
                }
            default: return false;
        }
    }

    private void ChangePreparingState(PreparingState changeReason)
    {
        switch (_currentConnectionState, changeReason)
        {
            case (ConnectionState.Searching, PreparingState.FoundNoDevices):
            case (ConnectionState.Selecting, PreparingState.DisconnectionInSelect):
            case (ConnectionState.InUse, PreparingState.DisconnectionInUse):
                {
                    if (ChangeConnectionState(ConnectionState.Preparing))
                    {
                        _currentPreparingState = changeReason;
                        ShowMessage();
                    }
                    break;
                }
            default: break;
        }
    }

    private void ShowMessage()
    {
        switch (_currentPreparingState)
        {
            case (PreparingState.InitialConnect):
                _textMeshPro.text = "使用するデバイスを接続してください";
                break;
            case (PreparingState.FoundNoDevices):
                _textMeshPro.text = "デバイスが見つかりませんでした";
                break;
            case (PreparingState.DisconnectionInSelect):
                _textMeshPro.text = "選択リストのデバイスが全て切断されました";
                break;
            case (PreparingState.DisconnectionInUse):
                _textMeshPro.text = "使用中のデバイスが切断されました";
                break;
        }
    }

    /// <summary>
    /// デバイスの選択ボタンを押したときに実行する
    /// </summary>
    /// <param name="decidedDeviceHandle">選択したデバイスの識別番号</param>
    public void DecideUsingDevice(int decidedDeviceHandle)
    {
        if (_currentConnectionState != ConnectionState.Selecting || !_connectedDeviceHandles.Contains(decidedDeviceHandle) || !JslStillConnected(decidedDeviceHandle))
        {
            return;
        }
        _selectedDeviceHandle = decidedDeviceHandle;
        ChangeConnectionState(ConnectionState.InUse);
    }

    /// <summary>
    /// 検索ボタンを押したときに実行する
    /// </summary>
    public void RequireSearch()
    {
        if (_currentConnectionState == ConnectionState.Preparing)
        {
            ChangeConnectionState(ConnectionState.Searching);
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
