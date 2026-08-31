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
        // Contentの子オブジェクトの総数を保存
        int childCount = _contentTransform.childCount;
        // Contentの子オブジェクトを一つずつ削除
        for (int i = 0; i < childCount; i++)
        {
            Transform child = _contentTransform.GetChild(i);
            Destroy(child.gameObject);
        }

        // 選択候補のButtonを一つずつ生成
        foreach (int selectioncandidate in selectionCandidates)
        {
            DeviceListItem deviceListItem;
            deviceListItem = Instantiate(_deviceListItemPrefab, _contentTransform);
            deviceListItem.InitializeDeviceInfo(selectioncandidate, $"{selectioncandidate}に対応するデバイス名");
        }
    }

    private void OnEnable()
    {
        _deviceConnectManager.SelectionCandidatesChanged += ReceiveSelectionEvent;
    }

    private void OnDisable()
    {
        _deviceConnectManager.SelectionCandidatesChanged -= ReceiveSelectionEvent;
    }
}
