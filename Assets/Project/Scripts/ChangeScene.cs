using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private Image _transitionOverlay;
    [SerializeField, Range(1f, 3f)] private float _sceneChangeTime = 1.0f;

    public void MoveToMainScene()
    {
        _transitionOverlay.raycastTarget = true;
        _transitionOverlay.DOFade(1f, _sceneChangeTime).OnComplete(() => { SceneManager.LoadScene("MainScene", LoadSceneMode.Single); });
    }

    public void MoveToTitleScene()
    {
        _transitionOverlay.raycastTarget = true;
        _transitionOverlay.DOFade(1f, _sceneChangeTime).OnComplete(() => { SceneManager.LoadScene("TitleScene", LoadSceneMode.Single); });
    }

    public void MoveToClearScene()
    {
        _transitionOverlay.raycastTarget = true;
        _transitionOverlay.DOFade(1f, _sceneChangeTime).OnComplete(() => { SceneManager.LoadScene("ClearScene", LoadSceneMode.Single); });
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
