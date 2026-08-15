using UnityEngine;
using static JSL;
using TMPro;

public class GyroController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _errorText;
    private void Start()
    {
        int isConnected = JslConnectDevices();
        if (isConnected == 0 )
        {
            _errorText.gameObject.SetActive(true);
            _errorText.text = "デバイスが接続されていません";
        }
    }
}
