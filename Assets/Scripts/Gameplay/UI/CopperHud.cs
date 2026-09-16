using TMPro;
using UnityEngine;

public class CopperHud : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private TMP_Text copperText;

    private void Awake()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();
    }

    private void OnEnable()
    {
        if (player == null)
            return;

        player.CoinsChanged += UpdateCopper;
        UpdateCopper(player.GetCoinAmount());
    }

    private void OnDisable()
    {
        if (player != null)
            player.CoinsChanged -= UpdateCopper;
    }

    private void UpdateCopper(int amount)
    {
        if (copperText != null)
            copperText.text = amount.ToString();
    }
}
