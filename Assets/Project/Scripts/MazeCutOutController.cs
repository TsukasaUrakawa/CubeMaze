using UnityEngine;

public class MazeCutOutController : MonoBehaviour
{
    [SerializeField] Camera _mainCamera;
    [SerializeField] Transform _ballTransform;
    [SerializeField] Material _mazeCutOutMaterial;

    private void LateUpdate()
    {
        Vector3 ballViewportPosition = _mainCamera.WorldToViewportPoint(_ballTransform.position);
        _mazeCutOutMaterial.SetVector("_HoleCenter", new Vector4(ballViewportPosition.x, ballViewportPosition.y, 0, 0));
    }
}
