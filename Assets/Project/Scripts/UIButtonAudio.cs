using UnityEngine;

public class UIButtonAudio : MonoBehaviour
{
    public void PlayClickSound()
    {
        PersistentSystem.Instance.PlayClickSound();
    }
}
