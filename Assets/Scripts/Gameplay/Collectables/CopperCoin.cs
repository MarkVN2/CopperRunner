using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CopperCoin : MonoBehaviour
{
    [SerializeField]
    [Min(1)]
    private int value = 1;

    private bool collected;

    private void Awake()
    {
        Collider2D coinCollider = GetComponent<Collider2D>();
        coinCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Collect(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collect(collision.collider);
    }

    private void Collect(Collider2D other)
    {
        if (collected || other == null)
            return;

        Player player = other.GetComponentInParent<Player>();
        if (player == null || player.IsDead)
            return;

        collected = true;
        player.AddCoin(value);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        collected = false;
    }
}
