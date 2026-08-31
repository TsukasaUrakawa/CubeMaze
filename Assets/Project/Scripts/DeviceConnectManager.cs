using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static JSL;

/// <summary>
/// デバイスの接続状態ごとの処理を管理
/// </summary>
public class DeviceConnectManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _messageText;

    /// <summary>
    /// デバイスの現在の接続状態
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// デバイスの接続を待っている状態
        /// </summary>
        Preparing,
        /// <summary>
        /// 接続済みのデバイスを検索している状態
        /// </summary>
        Searching,
        /// <summary>
        /// 接続済みのデバイスから一つ選択する状態
        /// </summary>
        Selecting,
        /// <summary>
        /// 接続済みのデバイスを使用している状態
        /// </summary>
        InUse
    }

    /// <summary>
    /// 現在の接続状態
    /// </summary>
    private ConnectionState _currentConnectionState = ConnectionState.Preparing;

    public ConnectionState CurrentConnectionState
    {
        get
        {
            return _currentConnectionState;
        }
    }

    /// <summary>
    /// ConnectionStateがPreparingになった理由
    /// </summary>
    public enum PreparingReason
    {
        /// <summary>
        /// 初回接続状態
        /// </summary>
        InitialStartup,
        /// <summary>
        /// 接続済みのデバイスを検索したが見つからなかった状態
        /// </summary>
        FoundNoDevices,
        /// <summary>
        /// 接続済みのデバイスを選択中に切断された状態
        /// </summary>
        AllSelectionCandidatesDisconnected,
        /// <summary>
        /// 接続済みのデバイスを使用中に切断された状態
        /// </summary>
        InUseDeviceDisconnected
    }

    /// <summary>
    /// 現在のPreparingになっている理由
    /// </summary>
    private PreparingReason _currentPreparingReason = PreparingReason.InitialStartup;

    public PreparingReason CurrentPreparingReason
    {
        get
        {
            return _currentPreparingReason;
        }
    }

    private int _detectedDeviceCount = 0; // 接続済みのデバイス数
    private int[] _selectionCandidateHandles; // 接続済みのデバイスの識別番号を格納
    private int _inUseDeviceHandle = -1; // 選択されたデバイスの識別番号
    public int InUseDeviceHandle
    {
        get
        {
            return _inUseDeviceHandle;
        }
    }

    /// <summary>
    /// 選択候補一覧の変化を他のクラスに通知
    /// </summary>
    public event Action<IReadOnlyList<int>> SelectionCandidatesChanged;

    private void Update()
    {
        switch (_currentConnectionState)
        {
            case ConnectionState.Preparing:
                ShowPreparingMessage();
                break;
            case ConnectionState.Searching:
                SearchDevices();
                if (_detectedDeviceCount >= 1 && ChangeConnectionState(ConnectionState.Selecting))
                {
                    SelectionCandidatesChanged?.Invoke(_selectionCandidateHandles); // 現在の選択候補一覧を渡す
                    _messageText.text = "";
                }
                else if (_detectedDeviceCount == 0)
                {
                    TransitionToPreparing(PreparingReason.FoundNoDevices);
                }
                break;
            case ConnectionState.Selecting:
                CheckSelectionCandidateConnections();
                break;
            case ConnectionState.InUse:
                CheckInUseDeviceConnection();
                break;
        }
    }

    /// <summary>
    /// 接続されているデバイスを検索する
    /// </summary>
    private void SearchDevices()
    {
        _detectedDeviceCount = JslConnectDevices(); // 認識した接続済みのデバイス数を保存
        if (_detectedDeviceCount >= 1)
        {
            int[] detectedDeviceHandles = new int[_detectedDeviceCount];
            JslGetConnectedDeviceHandles(detectedDeviceHandles, detectedDeviceHandles.Length); // 認識した接続済みのデバイスの識別番号を取得
            _selectionCandidateHandles = detectedDeviceHandles; // 認識した接続済みのデバイスの識別番号を保存
        }
        else
        {
            int[] emptyIntArray = Array.Empty<int>();
            _selectionCandidateHandles = emptyIntArray; // 選択候補の識別番号をリセット
        }
    }

    /// <summary>
    /// 選択候補の接続確認
    /// </summary>
    private void CheckSelectionCandidateConnections()
    {
        bool hasConnectedSelectionCandidate = false;　// 接続し続けているか判定

        foreach (int selectionCandidateHandle in _selectionCandidateHandles)
        {
            // 継続した接続確認
            if (JslStillConnected(selectionCandidateHandle))
            {
                hasConnectedSelectionCandidate = true;
            }
        }

        if (!hasConnectedSelectionCandidate)
        {
            HandleDisconnection();
        }
    }

    /// <summary>
    /// 使用中のデバイスの接続確認
    /// </summary>
    private void CheckInUseDeviceConnection()
    {
        bool isInUseDeviceConnected = JslStillConnected(_inUseDeviceHandle);
        if (isInUseDeviceConnected)
        {
            return;
        }
        else
        {
            HandleDisconnection();
        }
    }

    /// <summary>
    /// 切断時の処理
    /// </summary>
    private void HandleDisconnection()
    {
        switch (_currentConnectionState)
        {
            case (ConnectionState.Selecting):
                {
                    TransitionToPreparing(PreparingReason.AllSelectionCandidatesDisconnected);
                }
                break;
            case (ConnectionState.InUse):
                {
                    TransitionToPreparing(PreparingReason.InUseDeviceDisconnected);
                    _inUseDeviceHandle = -1;
                }
                break;
        }
    }

    /// <summary>
    /// 状態を遷移
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

    /// <summary>
    /// Preparing状態の理由ごとの処理
    /// </summary>
    /// <param name="preparingReason">Preparing状態の理由</param>
    private void TransitionToPreparing(PreparingReason preparingReason)
    {
        switch (_currentConnectionState, preparingReason)
        {
            case (ConnectionState.Searching, PreparingReason.FoundNoDevices):
            case (ConnectionState.Selecting, PreparingReason.AllSelectionCandidatesDisconnected):
            case (ConnectionState.InUse, PreparingReason.InUseDeviceDisconnected):
                {
                    if (ChangeConnectionState(ConnectionState.Preparing))
                    {
                        _currentPreparingReason = preparingReason;
                        ShowPreparingMessage();
                        SelectionCandidatesChanged?.Invoke(Array.Empty<int>()); // 選択候補の変化をDeviceConnectionUIに通知
                    }
                    break;
                }
            default: break;
        }
    }

    /// <summary>
    /// PreparingReasonに対応する文章を表示
    /// </summary>
    private void ShowPreparingMessage()
    {
        switch (_currentPreparingReason)
        {
            case (PreparingReason.InitialStartup):
                _messageText.text = "使用するデバイスを接続してください";
                break;
            case (PreparingReason.FoundNoDevices):
                _messageText.text = "デバイスが見つかりませんでした";
                break;
            case (PreparingReason.AllSelectionCandidatesDisconnected):
                _messageText.text = "選択リストのデバイスが全て切断されました";
                break;
            case (PreparingReason.InUseDeviceDisconnected):
                _messageText.text = "使用中のデバイスが切断されました";
                break;
        }
    }

    /// <summary>
    /// デバイスの選択ボタンを押したときに実行する
    /// </summary>
    /// <param name="decidedDeviceHandle">選択したデバイスの識別番号</param>
    public void ConfirmDeviceSelection(int decidedDeviceHandle)
    {
        if (_currentConnectionState != ConnectionState.Selecting || !_selectionCandidateHandles.Contains(decidedDeviceHandle) || !JslStillConnected(decidedDeviceHandle))
        {
            return;
        }
        _inUseDeviceHandle = decidedDeviceHandle;
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
