using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 一覧の一行に機器IDと表示名を保持し、クリック時に選択要求を通知する。
/// </summary>
public class DeviceListItem : MonoBehaviour
{
    /// <summary>
    /// 選択候補にあるデバイスそれぞれの識別番号を格納
    /// </summary>
    private int _deviceHandle = -1;
    /// <summary>
    /// この行に表示する機器名。
    /// </summary>
    [SerializeField] private TextMeshProUGUI _deviceNameText;
    /// <summary>
    /// 選択されたデバイスを渡す
    /// </summary>
    public event Action<int, string> DeviceSelectionRequested;

    /// <summary>
    /// 生成された一覧の行へ、候補の識別番号と表示名を設定する。
    /// </summary>
    /// <param name="deviceHandle">JSLが候補に割り当てた識別番号。</param>
    /// <param name="deviceDisplayName">同種内の番号などを含む、画面表示用の名前。</param>
    public void InitializeDeviceInfo(int deviceHandle, string deviceDisplayName)
    {
        _deviceHandle = deviceHandle;
        _deviceNameText.text = deviceDisplayName;
    }

    /// <summary>
    /// DeviceListItemのOnClickから呼ばれる
    /// デバイスの識別番号と現在の表示名を渡す。
    /// </summary>
    public void RequestDeviceSelection()
    {
        DeviceSelectionRequested?.Invoke(_deviceHandle, _deviceNameText.text);
    }
}
