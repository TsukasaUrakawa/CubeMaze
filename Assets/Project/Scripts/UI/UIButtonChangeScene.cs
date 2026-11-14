using UnityEngine;

/// <summary>
/// 各シーンのButtonから、常駐するシーン遷移担当へ移動要求を渡す。
/// </summary>
public class UIButtonChangeScene : MonoBehaviour
{
    /// <summary>
    /// ButtonのOnClickからタイトル画面への移動を依頼する。
    /// </summary>
    public void ChangeToTitleScene()
    {
        PersistentSystem.Instance.MoveToTitleScene();
    }

    /// <summary>
    /// ButtonのOnClickからメイン画面への移動を依頼する。
    /// </summary>
    public void ChangeToMainScene()
    {
        PersistentSystem.Instance.MoveToMainScene();
    }
}
