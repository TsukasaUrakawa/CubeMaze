using UnityEngine;

/// <summary>
/// 各シーンのButtonから、常駐している共通クリック音源へ再生要求を渡す。
/// </summary>
public class UIButtonAudio : MonoBehaviour
{
    /// <summary>
    /// ButtonのOnClickから呼び出し、その時点の常駐音源でクリックSEを鳴らす。
    /// </summary>
    public void PlayClickSound()
    {
        PersistentSystem.Instance.PlayClickSound();
    }
}
