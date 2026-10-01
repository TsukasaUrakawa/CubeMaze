using UnityEngine;

public class TitleCubeRotate : MonoBehaviour
{
    private Transform _titleCubeTransform;
    [SerializeField] private float _rotateSpeed = 72f;

    void Awake()
    {
        _titleCubeTransform = this.transform;
    }
    void Update()
    {
        _titleCubeTransform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime, Space.World);
    }
}
