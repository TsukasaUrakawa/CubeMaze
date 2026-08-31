using System.Collections.Generic;
using UnityEngine;

public class DeviceConnectionUI : MonoBehaviour
{
    [SerializeField] private DeviceListItem _deviceListItemPrefab;
    [SerializeField] private RectTransform _deviceListContent;
    [SerializeField] private DeviceConnectManager _deviceConnectManager;

    /// <summary>
    /// デバイスの選択候補それぞれで行う処理
    /// </summary>
    /// <param name="selectionCandidates">選択候補</param>
    private void OnSelectionCandidatesChanged(IReadOnlyList<int> selectionCandidates)
    {
        // Contentの子オブジェクトの総数を保存
        int childCount = _deviceListContent.childCount;
        // Contentの子オブジェクトを一つずつ削除
        for (int i = 0; i < childCount; i++)
        {
            Transform child = _deviceListContent.GetChild(i);
            Destroy(child.gameObject);
        }

        // 選択候補のButtonを一つずつ生成
        foreach (int selectionCandidateHandle in selectionCandidates)
        {
            DeviceListItem deviceListItem;
            deviceListItem = Instantiate(_deviceListItemPrefab, _deviceListContent);
            deviceListItem.InitializeDeviceInfo(selectionCandidateHandle, $"{selectionCandidateHandle}に対応するデバイス名"); // 識別番号に対応したデバイス名を表示
        }
    }

    private void OnEnable()
    {
        _deviceConnectManager.SelectionCandidatesChanged += OnSelectionCandidatesChanged;
    }

    private void OnDisable()
    {
        _deviceConnectManager.SelectionCandidatesChanged -= OnSelectionCandidatesChanged;
    }
}
