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
    [SerializeField] private float _effectScale = 1f;
    private Bounds _outerCubeBounds;
    [SerializeField] private float _respawnWaitTime = 0.5f;
    private float _waitElapsedTime = 0f;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _outerCubeBounds = _outerCubeCollider.GetComponent<MeshFilter>().sharedMesh.bounds;
    }

    private void FixedUpdate()
    {
        if (_gyroController.CurrentControllState == GyroController.ControllState.WaitingForRespawn)
        {
            _waitElapsedTime += Time.fixedDeltaTime;
            if (_waitElapsedTime >= _respawnWaitTime)
            {
                if (_gyroController.TryResumeAfterRespawn())
                {
                    _rigidbody.isKinematic = false;
                }
            }
            return;
        }
        Vector3 ballLocalPosition = _outerCubeCollider.transform.InverseTransformPoint(_rigidbody.position);
        if (!_outerCubeBounds.Contains(ballLocalPosition))
        {
            RespawnBall();
        }
    }

    private void RespawnBall()
    {
        if (!_gyroController.ResetForRespawn())
        {
            return;
        }
        else
        {
            _waitElapsedTime = 0f;
            GameObject teleportEffect = Instantiate(_effectPrefab, _respawnPoint.position, _respawnPoint.rotation);
            teleportEffect.transform.localScale = Vector3.one * _effectScale;
            _effectSE.Play();
            this._rigidbody.linearVelocity = Vector3.zero;
            this._rigidbody.angularVelocity = Vector3.zero;
            this._rigidbody.isKinematic = true;
            this._rigidbody.position = _respawnPoint.position;
        }
    }
}
