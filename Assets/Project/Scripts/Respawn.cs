using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 場外検知からボールの停止・初期位置への配置・操作再開まで、一回の復帰を管理する。
/// </summary>
public class Respawn : MonoBehaviour
{
    /// <summary>
    /// 復帰時に位置・速度・物理移動の有効状態を変更するボール。
    /// </summary>
    private Rigidbody _rigidbody;
    /// <summary>
    /// 場外判定の基準。メッシュのローカルBoundsとTransformを利用する。
    /// </summary>
    [SerializeField] private Collider _outerCubeCollider;
    /// <summary>
    /// ボール中心を戻す地点。開始時のワールド位置・回転を保存する。
    /// </summary>
    [SerializeField] private Transform _respawnPoint;

    /// <summary>
    /// 迷路の初期化と、現在の機器姿勢を基準にした操作再開を依頼する相手。
    /// </summary>
    [SerializeField] private GyroController _gyroController;

    /// <summary>
    /// 復帰の配置成功時に生成する演出のPrefab。
    /// </summary>
    [SerializeField] private GameObject _effectPrefab;

    /// <summary>
    /// 生成した演出個体に適用する各軸共通のスケール。
    /// </summary>
    [SerializeField] private float _effectScale = 1f;

    /// <summary>
    /// OuterCubeのメッシュが持つローカル座標の境界。
    /// </summary>
    private Bounds _outerCubeBounds;

    /// <summary>
    /// 配置後、操作再開を試みるまでの待機時間（秒）。
    /// </summary>
    [SerializeField] private float _respawnWaitTime = 0.5f;

    /// <summary>
    /// 今回の配置後に経過した物理更新の時間（秒）。
    /// </summary>
    private float _waitElapsedTime = 0f;

    /// <summary>
    /// リスポーン地点の初期位置を保持する変数
    /// </summary>
    private Vector3 _initialPosition;

    /// <summary>
    /// リスポーン地点の初期回転を保持する変数
    /// </summary>
    private Quaternion _initialRotation;

    /// <summary>
    /// 復帰の受付から終了までの進行段階。迷路の操作モードとは別に管理する。
    /// </summary>
    public enum RespawnState
    {
        /// <summary>
        /// 新しい復帰要求を受け付けられる状態。
        /// </summary>
        Ready,
        /// <summary>
        /// ボールを停止し、迷路の初期化と配置を行っている状態。
        /// </summary>
        Restoring,
        /// <summary>
        /// 配置後、待機時間と操作再開条件を確認している状態。
        /// </summary>
        WaitingToResume,
        /// <summary>
        /// 初期化に失敗し、自動的な再受付を止めている状態。
        /// </summary>
        RecoveryRequired,
    }

    /// <summary>
    /// Ready以外の状態かを取得する。失敗による停止中も含む。
    /// </summary>
    public bool IsRespawning => _currentRespawnState != RespawnState.Ready;

    /// <summary>
    /// 復帰進行の保存先。初期値は列挙値0のReady。
    /// </summary>
    private RespawnState _currentRespawnState;

    /// <summary>
    /// 現在の復帰段階を外部へ読み取り専用で公開する。
    /// </summary>
    public RespawnState CurrentRespawnState => _currentRespawnState;

    /// <summary>
    /// ボール中心の位置が初期位置からどれだけ離れているかを許容する距離。
    /// </summary>
    private float _allowingDistance = 0.1f;

    /// <summary>
    /// ボールの物理的相互作用を検出するためのスフィアコライダー。
    /// </summary>
    private SphereCollider _sphereCollider;

    /// <summary>
    /// 重なっているかを判定する対象のレイヤー。ボールと迷路の壁などを除外する。
    /// </summary>
    [SerializeField] private LayerMask _targetLayer;

    /// <summary>
    /// ボールについているスフィアコライダーのスケール。
    /// </summary>
    private float _sphereScale;

    /// <summary>
    /// リスポーン地点に配置れたときに呼び出されるイベント。引数は復帰後のワールド位置と回転。
    /// </summary>
    [SerializeField] private UnityEvent<Vector3, Quaternion> _onRespawnPlaced;

    /// <summary>
    /// 現在のリスポーンID。
    /// </summary>
    private int _respawnId = -1;

    /// <summary>
    /// 配置通知済みの最後のリスポーンID。配置後に呼び出されるイベントの重複を防ぐために使用する。
    /// </summary>
    private int _lastNotifiedPlacedRespawnId = -1;

    /// <summary>
    /// 操作開始前の復帰先と、ボール・場外判定に必要な情報を保存する。
    /// </summary>
    private void Start()
    {
        _sphereScale = this.transform.lossyScale.x;

        // 操作前のワールド位置・回転を保存する。
        _initialPosition = _respawnPoint.position;
        _initialRotation = _respawnPoint.rotation;

        _sphereCollider = GetComponent<SphereCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _outerCubeBounds = _outerCubeCollider.GetComponent<MeshFilter>().sharedMesh.bounds;
    }

    /// <summary>
    /// 復帰待ち中は再開を試み、それ以外ではボール中心の場外を検知する。
    /// </summary>
    private void FixedUpdate()
    {
        if (_currentRespawnState == RespawnState.WaitingToResume)
        {
            _waitElapsedTime += Time.fixedDeltaTime;
            if (IsPlacementSafe())
            {
                if (_respawnId != _lastNotifiedPlacedRespawnId)
                {
                    _lastNotifiedPlacedRespawnId = _respawnId;
                    _onRespawnPlaced.Invoke(_initialPosition, _initialRotation);
                }

                if (_waitElapsedTime >= _respawnWaitTime)
                {
                    if (_gyroController.TryResumeAfterRespawn())
                    {
                        _rigidbody.isKinematic = false;
                        _currentRespawnState = RespawnState.Ready;
                    }
                }
            }
            else
            {
                _currentRespawnState = RespawnState.RecoveryRequired;
            }
            return;
        }
        // Boundsはメッシュのローカル座標なので、球の中心も同じ座標系へ変換して比較する。
        Vector3 ballLocalPosition = _outerCubeCollider.transform.InverseTransformPoint(_rigidbody.position);
        if (!_outerCubeBounds.Contains(ballLocalPosition))
        {
            RespawnBall();
        }
    }

    /// <summary>
    /// 重複要求を拒否し、停止・迷路初期化・配置を行う。成功した場合だけ演出する。
    /// </summary>
    private void RespawnBall()
    {
        if (_currentRespawnState != RespawnState.Ready)
        {
            return;
        }
        else
        {
            // 他の処理を呼ぶ前に受付済みにし、同じ復帰中の追加要求を拒否する。
            _currentRespawnState = RespawnState.Restoring;
            _respawnId++;
            this._rigidbody.linearVelocity = Vector3.zero;
            this._rigidbody.angularVelocity = Vector3.zero;
            this._rigidbody.isKinematic = true;
            if (_gyroController.ResetForRespawn())
            {
                this._rigidbody.position = _initialPosition;
                _waitElapsedTime = 0f;
                _currentRespawnState = RespawnState.WaitingToResume;
            }
            else
            {
                // 失敗を通常状態へ戻さず、次の物理更新で同じ復帰を繰り返さないようにする。
                _currentRespawnState = RespawnState.RecoveryRequired;
            }
        }
    }

    private bool IsPlacementSafe()
    {
        // ボールの中心を基準に、スフィアコライダーの半径で重なりを検出する。
        Collider[] overlappingColliders = Physics.OverlapSphere(_rigidbody.position, _sphereCollider.radius * _sphereScale, _targetLayer, QueryTriggerInteraction.Ignore);
        foreach (Collider collider in overlappingColliders)
        {
            if (Physics.ComputePenetration(
                _sphereCollider, _rigidbody.position, _rigidbody.rotation,
                collider, collider.transform.position, collider.transform.rotation,
                out Vector3 direction, out float distance
            ))
            {
                Debug.LogWarning($"ボールの配置が安全でない可能性があります。重なりが検出されました。：{collider.name}");
                // 重なりがある場合は配置が安全でないと判断する。
                return false;
            }
        }
        float measuredDistance = Vector3.Distance(_rigidbody.position, _initialPosition);
        if (measuredDistance > _allowingDistance)
        {
            Debug.LogWarning($"初期位置とボールの距離が許容範囲を超えています。：{measuredDistance}");
            // 初期位置からの距離が許容範囲を超えている場合も配置が安全でないと判断する。
            return false;
        }
        // ボールの中心が場外判定の範囲内にあるかを確認する。
        Vector3 ballLocalPosition = _outerCubeCollider.transform.InverseTransformPoint(_rigidbody.position);
        if (!_outerCubeBounds.Contains(ballLocalPosition))
        {
            Debug.LogWarning($"ボールの中心が場外判定の範囲外にあります。：{ballLocalPosition}");
            return false;
        }
        return true;
    }

    public void RespawnEffectPlayer(Vector3 position, Quaternion rotation)
    {
        GameObject teleportEffect = Instantiate(_effectPrefab, position, rotation);
        teleportEffect.transform.localScale = Vector3.one * _effectScale;
    }
}
