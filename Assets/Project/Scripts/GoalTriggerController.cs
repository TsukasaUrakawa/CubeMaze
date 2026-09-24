using UnityEngine;

public class GoalTriggerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _ballRigidbody;
    private bool _isCleared = false;
    private void OnTriggerEnter(Collider other)
    {
        if (_isCleared)
        {
            return;
        }
        if (other.attachedRigidbody != _ballRigidbody)
        {
            return;
        }
        _isCleared = true;
        PersistentSystem.Instance.MoveToClearScene();
    }
}
