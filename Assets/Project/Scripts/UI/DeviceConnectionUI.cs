using System.Collections.Generic;
using UnityEngine;
using static JSL.ControllerType;

/// <summary>
/// 候補一覧・詳細選択・接続状態の表示を仲介し、測定表示とパネルの操作可否を切り替える。
/// </summary>
public class DeviceConnectionUI : MonoBehaviour
{
    /// <summary>
    /// 検出候補ごとに生成する一覧の行。
    /// </summary>
    [SerializeField] private DeviceListItem _deviceListItemPrefab;
    /// <summary>
    /// 候補の行を配置するScrollViewのContent。
    /// </summary>
    [SerializeField] private RectTransform _deviceListContent;
    /// <summary>
    /// 候補・接続状態の通知元と、選択確定の依頼先。
    /// </summary>
    [SerializeField] private DeviceConnectManager _deviceConnectManager;
    /// <summary>
    /// クリックした候補の表示と、確定要求の通知を担当するUI。
    /// </summary>
    [SerializeField] private DeviceDetailUI _deviceDetailUI;
    /// <summary>
    /// 測定中に表示するアニメーション。測定の完了判定は接続管理が担当する。
    /// </summary>
    [SerializeField] private Animator _calibrationAnimator;
    /// <summary>
    /// 接続パネルの見た目・操作・クリック遮断をまとめて切り替える。
    /// </summary>
    private CanvasGroup _deviceConnectionCanvasGroup;

    /// <summary>
    /// 同じオブジェクトのCanvasGroupを取得する。
    /// </summary>
    private void Awake()
    {
        _deviceConnectionCanvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// 既存の候補行を破棄し、通知された候補IDから表示名と選択ボタンを作り直す。
    /// </summary>
    /// <param name="selectionCandidates">選択候補</param>
    private void OnSelectionCandidatesChanged(IReadOnlyList<int> selectionCandidates)
    {
        for (int i = 0; i < _deviceListContent.childCount; i++) // Contentの子オブジェクトを一つずつ削除
        {
            Transform child = _deviceListContent.GetChild(i);
            Destroy(child.gameObject);
        }

        // 表示名には同種内の番号を付けるが、実際の選択はJSLの機器IDで区別する。
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
    /// 接続状態の変化時に呼ばれるメソッド
    /// </summary>
    /// <param name="currentConnectionState">現在の接続状態</param>
    private void OnConnectionStateChanged(DeviceConnectManager.ConnectionState currentConnectionState)
    {
        _calibrationAnimator.gameObject.SetActive(currentConnectionState == DeviceConnectManager.ConnectionState.Calibrating);
        if (currentConnectionState == DeviceConnectManager.ConnectionState.Calibrating)
        {
            _calibrationAnimator.Play("Base Layer.CalibrationLoading", 0, 0f);
        }
        if (currentConnectionState == DeviceConnectManager.ConnectionState.InUse)
        {
            // 透明にするだけではクリックを遮るため、操作可否とRaycastも同時に無効にする。
            _deviceConnectionCanvasGroup.alpha = 0;
            _deviceConnectionCanvasGroup.interactable = false;
            _deviceConnectionCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            _deviceConnectionCanvasGroup.alpha = 1;
            _deviceConnectionCanvasGroup.interactable = true;
            _deviceConnectionCanvasGroup.blocksRaycasts = true;
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

    /// <summary>
    /// 候補・確定・接続の通知を購読し、現在の接続状態を表示へ反映する。
    /// </summary>
    private void OnEnable()
    {
        _deviceConnectManager.SelectionCandidatesChanged += OnSelectionCandidatesChanged;
        _deviceDetailUI.DeviceConfirmationRequested += OnDeviceConfirmationRequested;
        _deviceConnectManager.ConnectionStateChanged += OnConnectionStateChanged;
        OnConnectionStateChanged(_deviceConnectManager.CurrentConnectionState);
    }

    /// <summary>
    /// 再表示で通知が重複しないよう、この画面の購読を解除する。
    /// </summary>
    private void OnDisable()
    {
        _deviceConnectManager.SelectionCandidatesChanged -= OnSelectionCandidatesChanged;
        _deviceDetailUI.DeviceConfirmationRequested -= OnDeviceConfirmationRequested;
        _deviceConnectManager.ConnectionStateChanged -= OnConnectionStateChanged;
    }
}
