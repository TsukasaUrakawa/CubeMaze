using System.Collections.Generic;
using UnityEngine;

public class DeviceConnectionUI : MonoBehaviour
{
    [SerializeField] private DeviceListItem _deviceListItemPrefab;
    [SerializeField] private RectTransform _contentTransform;
    [SerializeField] private DeviceConnectManager _deviceConnectManager;

    /// <summary>
    /// デバイスの選択候補それぞれで行う処理
    /// </summary>
    /// <param name="selectionCandidates">選択候補</param>
    private void ReceiveSelectionEvent(IReadOnlyList<int> selectionCandidates)
    {
        foreach (int selectioncandidate in selectionCandidates)
        {
            DeviceListItem deviceListItem;
            deviceListItem = Instantiate(_deviceListItemPrefab, _contentTransform);
            deviceListItem.InitializeDeviceInfo(selectioncandidate, $"{selectioncandidate}に対応するデバイス名");
        }
    }

    private void OnEnable()
    {
        _deviceConnectManager.SelectionCandidatesPrepared += ReceiveSelectionEvent;
    }

    private void OnDisable()
    {
        _deviceConnectManager.SelectionCandidatesPrepared -= ReceiveSelectionEvent;
    }
}
