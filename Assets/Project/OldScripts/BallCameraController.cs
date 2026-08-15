using UnityEngine;

public class BallCameraController : MonoBehaviour
{
    [SerializeField] private Transform _ball;

    [Header("Follow Settings")]
    [SerializeField] private float _smoothTime = 0.2f;

    private Vector3 _velocity;
    private float _fixedY;

    private void Start()
    {
        _fixedY = transform.position.y;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = new Vector3(
            _ball.position.x,
            _fixedY,
            _ball.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            _smoothTime
        );
    }
}