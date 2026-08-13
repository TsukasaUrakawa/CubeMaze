using Unity.VisualScripting;
using UnityEngine;

public class BallCameraController : MonoBehaviour
{
    [SerializeField] private Transform _ball;

    [Header("カメラ追従の補正")]
    [SerializeField] private float _smoothTime = 0.15f;

    private Vector3 _velocity;
    private void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _ball.position,
            ref _velocity,
            _smoothTime
            );
    }
}
