using UnityEngine;

public class Actor : MonoBehaviour
{
    protected float moveSpeed;
    protected float jumpForce;

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
