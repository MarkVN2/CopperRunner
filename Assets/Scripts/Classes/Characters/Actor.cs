using UnityEngine;

public class Actor : MonoBehaviour
{
    [SerializeField]
    protected float moveSpeed = 5f;

    [SerializeField]
    protected float jumpForce = 7f;

    public void SetMoveSpeed(float newSpeed)
    {
        this.moveSpeed = newSpeed;
    }

    public void SetJumpForce(float newJumpForce)
    {
        this.jumpForce = newJumpForce;
    }
    public float GetMoveSpeed() 
    {
        return moveSpeed;
    }
    public float GetJumpForce()
    {
        return jumpForce;
    }

}
