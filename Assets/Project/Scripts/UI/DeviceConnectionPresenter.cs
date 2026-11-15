using UnityEngine;
using static DeviceConnectManager;

public class DeviceConnectionPresenter : MonoBehaviour
{
    /// <summary>
    /// ModelのDeviceConnectManager。接続状態の変化を監視する。
    /// </summary>
    [SerializeField] private DeviceConnectManager _deviceConnectManager;

    [SerializeField] private DeviceConnectionUI _deviceConnectionUI;

    /// <summary>
    /// メッセージの表示や候補一覧の生成を担当するUI。接続状態の変化に応じて、測定表示とパネルの操作可否を切り替える。
    /// </summary>
    /// <param name="currentState">現在の接続状態</param>
    public void OnConnectionStateChanged(ConnectionState currentState)
    {
        switch (currentState)
        {
            case ConnectionState.Preparing:
                ShowPreparingMessage();
                break;
            default:
                _deviceConnectionUI.SetMessage("");
                break;
        }
    }

    /// <summary>
    /// PreparingReasonに対応する文章を表示
    /// </summary>
    public void ShowPreparingMessage()
    {
        switch (_deviceConnectManager.CurrentPreparingReason)
        {
            case (PreparingReason.InitialStartup):
                _deviceConnectionUI.SetMessage("使用するデバイスを接続してください");
                break;
            case (PreparingReason.FoundNoDevices):
                _deviceConnectionUI.SetMessage("デバイスが見つかりませんでした");
                break;
            case (PreparingReason.AllSelectionCandidatesDisconnected):
                _deviceConnectionUI.SetMessage("選択リストのデバイスが全て切断されました");
                break;
            case (PreparingReason.ActiveDeviceDisconnected):
                _deviceConnectionUI.SetMessage("選択したデバイスが切断されました");
                break;
        }
    }

    private void OnEnable()
    {
        _deviceConnectManager.ConnectionStateChanged += OnConnectionStateChanged;
        OnConnectionStateChanged(_deviceConnectManager.CurrentConnectionState);
    }

    private void OnDisable()
    {
        _deviceConnectManager.ConnectionStateChanged -= OnConnectionStateChanged;
    }
}
