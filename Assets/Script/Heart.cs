using UnityEngine;

public class Heart : MonoBehaviour
{
    [Header("Heal")]
    [SerializeField] private int healAmount = 20;

    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        // Chỉ Player mới có thể nhặt Heart
        if (!other.CompareTag("Player"))
            return;

        PlayerMovement player =
            other.GetComponent<PlayerMovement>();

        if (player == null)
        {
            Debug.LogWarning(
                "Heart: Không tìm thấy PlayerMovement trên Player!"
            );

            return;
        }

        // Hồi máu
        bool healed =
            player.Heal(healAmount);

        // Nếu hồi máu thành công
        if (healed)
        {
            collected = true;

            Debug.Log(
                "Player đã nhặt Heart và hồi " +
                healAmount +
                " HP."
            );

            Destroy(gameObject);
        }
        else
        {
            Debug.Log(
                "Player đang đầy máu. Heart không bị nhặt."
            );
        }
    }
}