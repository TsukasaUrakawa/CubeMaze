using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// 画面を暗転させてシーンを読み込み、読込後に明転と透過シェーダーの切替を行う。
/// </summary>
public class ChangeScene : MonoBehaviour
{
    /// <summary>
    /// 画面全体を覆うImage。アルファで暗転し、遷移中のクリックを遮る。
    /// </summary>
    [SerializeField] private Image _transitionOverlay;
    /// <summary>
    /// 暗転・明転それぞれの補間にかける時間（秒）。
    /// </summary>
    [SerializeField, Range(1f, 3f)] private float _sceneChangeTime = 1.0f;

    /// <summary>
    /// クリックを遮断し、暗転完了後にMainSceneへ置き換える。
    /// </summary>
    public void MoveToMainScene()
    {
        _transitionOverlay.raycastTarget = true;
        _transitionOverlay.DOFade(1f, _sceneChangeTime).OnComplete(() => { SceneManager.LoadScene("MainScene", LoadSceneMode.Single); });
    }

    /// <summary>
    /// クリックを遮断し、暗転完了後にTitleSceneへ置き換える。
    /// </summary>
    public void MoveToTitleScene()
    {
        _transitionOverlay.raycastTarget = true;
        _transitionOverlay.DOFade(1f, _sceneChangeTime).OnComplete(() => { SceneManager.LoadScene("TitleScene", LoadSceneMode.Single); });
    }

    /// <summary>
    /// クリックを遮断し、暗転完了後にClearSceneへ置き換える。
    /// </summary>
    public void MoveToClearScene()
    {
        _transitionOverlay.raycastTarget = true;
        _transitionOverlay.DOFade(1f, _sceneChangeTime).OnComplete(() => { SceneManager.LoadScene("ClearScene", LoadSceneMode.Single); });
    }
    /// <summary>
    /// 読込先に合わせて透過穴を切り替え、明転後にクリック遮断を解除する。
    /// </summary>
    /// <param name="scene">読み込まれたシーン。</param>
    /// <param name="loadSceneMode">シーン読込時のモード。現在の分岐では使用しない。</param>
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        // タイトルでは迷路の透過穴を無効にし、他のシーンでは有効にする。
        if (scene.name == "TitleScene")
        {
            Shader.SetGlobalFloat("_DisableHole", 1f);
        }
        else
        {
            Shader.SetGlobalFloat("_DisableHole", 0f);
        }
        if (_transitionOverlay.color.a <= 0)
        {
            _transitionOverlay.raycastTarget = false;
            return;
        }
        else
        {
            _transitionOverlay.DOFade(0f, _sceneChangeTime).OnComplete(() => { _transitionOverlay.raycastTarget = false; });
        }
    }

    /// <summary>
    /// シーン読込完了の通知を購読する。
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    /// <summary>
    /// 無効な個体へ読込完了の通知が届かないよう購読を解除する。
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    /// <summary>
    /// 実行中のゲームアプリケーションの終了を要求する。
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
