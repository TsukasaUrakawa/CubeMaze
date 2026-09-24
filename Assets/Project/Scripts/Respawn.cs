using UnityEngine;

public class Respawn : MonoBehaviour
{
    private Rigidbody _rigidbody;
    [SerializeField] private Collider _outerCubeCollider;
    [SerializeField] private Transform _respawnPoint;
    [SerializeField] private GyroController _gyroController;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider != _outerCubeCollider)
        {
            return;
        }
        _gyroController.ResetForRespawn();
        this.transform.position = _respawnPoint.position;
        this._rigidbody.linearVelocity = Vector3.zero;
        this._rigidbody.angularVelocity = Vector3.zero;
    }
}
