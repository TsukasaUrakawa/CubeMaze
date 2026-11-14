using UnityEngine;

/// <summary>
/// シーン移動後も残る共通音源とシーン遷移担当への入口を提供する。
/// </summary>
public class PersistentSystem : MonoBehaviour
{
    /// <summary>
    /// 各シーンから共有する常駐インスタンス。
    /// </summary>
    private static PersistentSystem _instance;
    /// <summary>
    /// 現在登録されている常駐インスタンスを取得する。
    /// </summary>
    public static PersistentSystem Instance
    {
        get
        {
            return _instance;
        }
    }

    /// <summary>
    /// シーンをまたいで利用するクリックSEの音源。
    /// </summary>
    [SerializeField] private AudioSource _uiAudioController;

    /// <summary>
    /// フェードとシーン読込を実行する担当。
    /// </summary>
    [SerializeField] private ChangeScene _sceneController;

    /// <summary>
    /// 重複生成された個体を破棄し、採用した個体をシーン移動後も残す。
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            _instance = this;
        }
        // このオブジェクトと子に配置した共通音源・遷移担当を、次のシーンでも使う。
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 共通のクリックSEを再生する。
    /// </summary>
    public void PlayClickSound()
    {
        _uiAudioController.Play();
    }

    /// <summary>
    /// 常駐する遷移担当へタイトル画面への移動を依頼する。
    /// </summary>
    public void MoveToTitleScene()
    {
        _sceneController.MoveToTitleScene();
    }

    /// <summary>
    /// 常駐する遷移担当へメイン画面への移動を依頼する。
    /// </summary>
    public void MoveToMainScene()
    {
        _sceneController.MoveToMainScene();
    }

    /// <summary>
    /// 常駐する遷移担当へクリア画面への移動を依頼する。
    /// </summary>
    public void MoveToClearScene()
    {
        _sceneController.MoveToClearScene();
    }
}
