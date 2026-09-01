using System.Collections.Generic;
using UnityEngine;
using static JSL.ControllerType;

public class DeviceConnectionUI : MonoBehaviour
{
    [SerializeField] private DeviceListItem _deviceListItemPrefab;
    [SerializeField] private RectTransform _deviceListContent;
    [SerializeField] private DeviceConnectManager _deviceConnectManager;
    [SerializeField] private DeviceDetailUI _deviceDetailUI;

    /// <summary>
    /// デバイスの選択候補それぞれで行う処理
    /// </summary>
    /// <param name="selectionCandidates">選択候補</param>
    private void OnSelectionCandidatesChanged(IReadOnlyList<int> selectionCandidates)
    {
        int childCount = _deviceListContent.childCount; // Contentの子オブジェクトの総数を保存
        for (int i = 0; i < childCount; i++) // Contentの子オブジェクトを一つずつ削除
        {
            Transform child = _deviceListContent.GetChild(i);
            Destroy(child.gameObject);
        }

        Dictionary<int, int> controllerTypeCounts = new Dictionary<int, int>();

        // 選択候補のButtonを一つずつ生成
        foreach (int selectionCandidateHandle in selectionCandidates)
        {
            int controllerType = _deviceConnectManager.GetControllerType(selectionCandidateHandle); // コントローラーの種別番号を取得
            if (controllerTypeCounts.ContainsKey(controllerType))
            {
                controllerTypeCounts[controllerType]++;
            }
            else
            {
                controllerTypeCounts.Add(controllerType, 1);
            }
            string displayName = GetControllerDisplayName(controllerType); // 種別番号に対応したコントローラーの種別名を取得

            DeviceListItem deviceListItem;
            deviceListItem = Instantiate(_deviceListItemPrefab, _deviceListContent);
            deviceListItem.InitializeDeviceInfo(selectionCandidateHandle, $"{displayName} {controllerTypeCounts[controllerType]}"); // 識別番号に対応したコントローラーの種別名と、同種内での番号を表示
            deviceListItem.DeviceSelectionRequested += OnDeviceSelectionRequested;
        }
    }

    /// <summary>
    /// コントローラー種別を表示名に変換
    /// </summary>
    /// <param name="controllerType">コントローラー種別を表す定数</param>
    /// <returns>コントローラー名</returns>
    private string GetControllerDisplayName(int controllerType) => controllerType switch
    {
        JoyConLeft => "左JoyCon",
        JoyConRight => "右JoyCon",
        ProController => "Proコントローラー",
        DualShock4 => "DualShock4",
        DualSense => "DualSense",
        _ => "未知のコントローラー"
    };

    /// <summary>
    /// 選択を要求されたデバイスの識別番号と表示名を DeviceDetailUIへ渡す
    /// </summary>
    /// <param name="deviceHandle">選択を要求されたデバイスの識別番号</param>
    /// <param name="deviceNameText">選択を要求されたデバイスのコントローラーの種別名</param>
    private void OnDeviceSelectionRequested(int deviceHandle, string deviceNameText)
    {
        _deviceDetailUI.SetPendingDevice(deviceHandle, deviceNameText);
    }

    /// <summary>
    /// デバイスの確定要求をDeviceConnectManagerへ渡す
    /// </summary>
    /// <param name="deviceHandle">確定を要求されたデバイスの識別番号</param>
    private void OnDeviceConfirmationRequested(int deviceHandle)
    {
        _deviceConnectManager.ConfirmDeviceSelection(deviceHandle);
    }

    private void OnEnable()
    {
        _deviceConnectManager.SelectionCandidatesChanged += OnSelectionCandidatesChanged;
        _deviceDetailUI.DeviceConfirmationRequested += OnDeviceConfirmationRequested;
    }

    private void OnDisable()
    {
        _deviceConnectManager.SelectionCandidatesChanged -= OnSelectionCandidatesChanged;
        _deviceDetailUI.DeviceConfirmationRequested -= OnDeviceConfirmationRequested;
    }
}
