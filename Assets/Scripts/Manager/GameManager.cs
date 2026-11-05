using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance;
   [SerializeField]
   private Player activePlayer;
   public Player ActivePlayer => activePlayer;

    private void Awake()
    {
        activePlayer = Object.FindAnyObjectByType<Player>();
        if (Instance != this)
        {
            Instance = this;
        }
        else
        {
            Instance = null;
        }
    }

}
