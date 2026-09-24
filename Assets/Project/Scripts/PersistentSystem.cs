using UnityEngine;

public class PersistentSystem : MonoBehaviour
{
    private static PersistentSystem _instance;
    public static PersistentSystem Instance
    {
        get
        {
            return _instance;
        }
    }

    [SerializeField] private AudioSource _uiAudioController;

    [SerializeField] private ChangeScene _sceneController;

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
        DontDestroyOnLoad(gameObject);
    }

    public void PlayClickSound()
    {
        _uiAudioController.Play();
    }

    public void MoveToTitleScene()
    {
        _sceneController.MoveToTitleScene();
    }

    public void MoveToMainScene()
    {
        _sceneController.MoveToMainScene();
    }

    public void MoveToClearScene()
    {
        _sceneController.MoveToClearScene();
    }
}
