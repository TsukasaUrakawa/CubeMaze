using System;
using TMPro;
using UnityEngine;

/// <summary>
/// UIに存在するDeviceListItemの処理
/// </summary>
public class DeviceListItem : MonoBehaviour
{
    /// <summary>
    /// 選択候補にあるデバイスそれぞれの識別番号を格納
    /// </summary>
    private int _deviceHandle = -1;
    [SerializeField] private TextMeshProUGUI _deviceNameText;
    /// <summary>
    /// 選択されたデバイスを渡す
    /// </summary>
    public event Action<int> DeviceSelectionRequested;

    public void InitializeDeviceInfo(int deviceHandle, string deviceDisplayName)
    {
        _deviceHandle = deviceHandle;
        _deviceNameText.text = deviceDisplayName;
    }

    /// <summary>
    /// DeviceListItemのOnClickから呼ばれる
    /// デバイスの識別番号を渡す
    /// </summary>
    public void RequestDeviceSelection()
    {
        DeviceSelectionRequested?.Invoke(_deviceHandle);
    }
}
