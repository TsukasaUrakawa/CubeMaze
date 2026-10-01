using UnityEngine;

public class UIButtonChangeScene : MonoBehaviour
{
    public void ChangeToTitleScene()
    {
        PersistentSystem.Instance.MoveToTitleScene();
    }

    public void ChangeToMainScene()
    {
        PersistentSystem.Instance.MoveToMainScene();
    }
}
