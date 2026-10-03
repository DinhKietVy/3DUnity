using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Camera cam;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    private void LateUpdate()
    {
        // Thanh máu luôn hướng về camera
        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        if (fillImage == null)
            return;

        float percent = (float)currentHealth / maxHealth;

        fillImage.fillAmount = Mathf.Clamp01(percent);
    }
}