using System;
using TMPro;
using UnityEngine;

public class DeviceDetailUI : MonoBehaviour
{
    private int _pendingDeviceHandle = -1;
    [SerializeField] private TextMeshProUGUI _deviceNameText;

    /// <summary>
    /// デバイスの確定が要求されたことを通知
    /// </summary>
    public event Action<int> DeviceConfirmationRequested;


    /// <summary>
    /// 確定前のデバイスハンドルと表示名を設定する
    /// </summary>
    /// <param name="deviceHandle">選択候補として表示するデバイスの識別番号</param>
    /// <param name="deviceNameText">選択候補として表示するデバイスのコントローラー種別名</param>
    public void SetPendingDevice(int deviceHandle, string deviceNameText)
    {
        _pendingDeviceHandle = deviceHandle;
        _deviceNameText.text = deviceNameText;
    }

    /// <summary>
    /// 選択ボタンを押したときに呼ばれる
    /// 確定前の識別番号を通知
    /// </summary>
    public void RequestDeviceConfirmation()
    {
        if (_pendingDeviceHandle == -1)
        {
            return;
        }

        DeviceConfirmationRequested?.Invoke(_pendingDeviceHandle);
    }
}
