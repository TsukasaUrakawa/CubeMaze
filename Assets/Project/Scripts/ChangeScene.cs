using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void MoveToMainScene()
    {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    public void MoveToTitleScene()
    {
        SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
    }

    public void MoveToClearScene()
    {
        SceneManager.LoadScene("ClearScene", LoadSceneMode.Single);
    }
}
