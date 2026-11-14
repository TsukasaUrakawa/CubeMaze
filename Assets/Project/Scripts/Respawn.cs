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

    // リスポーン地点の初期位置を保持する変数
    private Vector3 _initialPosition;

    // リスポーン地点の初期回転を保持する変数
    private Quaternion _initialRotation;

    private void Start()
    {
        _initialPosition = _respawnPoint.position;
        _initialRotation = _respawnPoint.rotation;
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
            GameObject teleportEffect = Instantiate(_effectPrefab, _initialPosition, _initialRotation);
            teleportEffect.transform.localScale = Vector3.one * _effectScale;
            _effectSE.Play();
            this._rigidbody.linearVelocity = Vector3.zero;
            this._rigidbody.angularVelocity = Vector3.zero;
            this._rigidbody.isKinematic = true;
            this._rigidbody.position = _initialPosition;
        }
    }
}
