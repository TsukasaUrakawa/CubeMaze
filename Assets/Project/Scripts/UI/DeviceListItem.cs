using System;
using TMPro;
using UnityEngine;

public class DeviceListItem : MonoBehaviour
{
    private int _deviceHandle = -1;
    [SerializeField] private TextMeshProUGUI _deviceNameText;
    public event Action<int> NoticeSelectedDevice;

    public void InitializeDeviceInfo(int deviceHandle, string deviceModelText)
    {
        _deviceHandle = deviceHandle;
        _deviceNameText.text = deviceModelText;
    }
    /// <summary>
    /// DeviceListItemのOnClickから呼ばれる
    /// デバイスの識別番号を通知する
    /// </summary>
    public void NoticeDeviceHandle()
    {
        NoticeSelectedDevice?.Invoke(_deviceHandle);
    }
}
