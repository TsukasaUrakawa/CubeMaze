using UnityEngine;
using UnityEngine.InputSystem; 

public class CubeTiltController : MonoBehaviour
{
    [Header("‰ñ“]İ’è")]
    [SerializeField] private float maxTiltAngle = 20f; // Å‘åŒXÎŠp
    [SerializeField] private float rotationSpeed = 5f; // ‰ñ“]‘¬“x

    private Vector2 tiltInput;

    public void OnTilt(InputValue value)
    {
        //InputSystem‚©‚ç‚Ì“ü—Í‚ğæ“¾‚µ‚ÄAtiltInput‚ÉŠi”[‚·‚é
        tiltInput = value.Get<Vector2>();
    }

    void Update()
    {
        float targetX = -tiltInput.y * maxTiltAngle; // ã‰º‚ÌŒX‚«
        float targetZ = -tiltInput.x * maxTiltAngle; // ¶‰E‚ÌŒX‚«i”½“]‚³‚¹‚éj

        Quaternion targetRotation = Quaternion.Euler(targetX, 0f, targetZ);

        // Œ»İ‚Ì‰ñ“]Šp‚©‚ç–Ú•W‚Ì‰ñ“]Šp‚ÉŒü‚©‚Á‚Äˆê’è‘¬“x‚Å‰ñ“]‚³‚¹‚é
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
