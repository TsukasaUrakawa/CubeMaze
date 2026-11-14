using UnityEngine;

/// <summary>
/// 指定ボールのゴール接触を受け付け、一度だけクリア画面への遷移を依頼する。
/// </summary>
public class GoalTriggerController : MonoBehaviour
{
    /// <summary>
    /// ゴール対象のボール。他のColliderの接触と区別する。
    /// </summary>
    [SerializeField] private Rigidbody _ballRigidbody;
    /// <summary>
    /// このゴールですでにクリアを受け付けたか。
    /// </summary>
    private bool _isCleared = false;
    /// <summary>
    /// 初回の対象ボール接触だけをクリアとして受け付ける。
    /// </summary>
    /// <param name="other">ゴールのTriggerに入ったCollider。</param>
    private void OnTriggerEnter(Collider other)
    {
        if (_isCleared)
        {
            return;
        }
        // ボールの子Colliderからの接触も、所属するRigidbodyで対象を判定する。
        if (other.attachedRigidbody != _ballRigidbody)
        {
            return;
        }
        // 遷移を依頼する前に受付済みにし、続く接触で二重に遷移要求を出さない。
        _isCleared = true;
        PersistentSystem.Instance.MoveToClearScene();
    }
}
