using UnityEngine;

public class Respawn : MonoBehaviour
{
    private Rigidbody _rigidbody;
    [SerializeField] private Collider _outerCubeCollider;
    [SerializeField] private Transform _respawnPoint;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == _outerCubeCollider)
        {
            _rigidbody.position = _respawnPoint.position;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
