using UnityEngine;

public class PlayerInput
{
    public bool CheckIfPressedAttack()
    {
        return Input.GetButtonDown("Fire2");
    }

    public bool CheckIfPressedSlide()
    {
        return Input.GetButtonDown("Fire1");
    }

    public bool CheckIfPressedJump()
    {
        return Input.GetButtonDown("Jump");
    }

    public bool CheckIfJumpCancelled()
    {
        return Input.GetKeyUp(KeyCode.Space);
    }

    public Vector2 GetMovementInputVector()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
}
