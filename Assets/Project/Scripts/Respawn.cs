using Unity.VisualScripting;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    private Rigidbody _rigidbody;
    [SerializeField] private Collider _outerCubeCollider;
    [SerializeField] private Transform _respawnPoint;
    [SerializeField] private GyroController _gyroController;
    [SerializeField] private GameObject _effectPrefab;
    [SerializeField] private AudioSource _effectSE; 
    private Bounds _outerCubeBounds;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _outerCubeBounds = _outerCubeCollider.GetComponent<MeshFilter>().sharedMesh.bounds;
    }

    private void FixedUpdate()
    {
        Vector3 ballLocalPosition = _outerCubeCollider.transform.InverseTransformPoint(_rigidbody.position);
        if (!_outerCubeBounds.Contains(ballLocalPosition))
        {
            RespawnBall();
        }
    }

    private void RespawnBall()
    {
        _gyroController.ResetForRespawn();
        Instantiate(_effectPrefab, this.gameObject.transform);
        _effectSE.Play();
        this._rigidbody.position = _respawnPoint.position;
        this._rigidbody.linearVelocity = Vector3.zero;
        this._rigidbody.angularVelocity = Vector3.zero;
    }
}
