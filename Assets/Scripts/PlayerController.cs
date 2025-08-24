using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerState CurrentState;
    public Animator Anim;

    public float CurrentDodgeTime;
    public float MaxDodgeTime = 1.0f;
    public float MinDodgeTime = 0.2f;

    public float CurrentDodgeCooldownRemaining;
    public float DodgeCooldownTime = .1f;

    public float CurrentHorizontalInput;
    public float CurrentVerticalInput;


    private void Update()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        var isUpdatedHorizontalInput = horizontalInput != CurrentHorizontalInput;
        var isUpdatedVerticalInput = verticalInput != CurrentVerticalInput;
        CurrentHorizontalInput = horizontalInput;
        CurrentVerticalInput = verticalInput;

        UpdateDodgeTime();

        if(isUpdatedHorizontalInput)
        {
            if (CurrentDodgeCooldownRemaining <= 0 && CurrentState == PlayerState.Default && horizontalInput > 0)
            {
                DodgeRight();
                return;
            }

            if (CurrentDodgeCooldownRemaining <= 0 && CurrentState == PlayerState.Default && horizontalInput < 0)
            {
                DodgeLeft();
                return;
            }
        }

        if (isUpdatedVerticalInput)
        {
            if (CurrentDodgeCooldownRemaining <= 0 && CurrentState == PlayerState.Default && verticalInput < 0)
            {
                Duck();
                return;
            }
        }
    }

    private void UpdateDodgeTime()
    {

        if (CurrentDodgeCooldownRemaining > 0) { 
            CurrentDodgeCooldownRemaining -= Time.deltaTime;
        }

        if (CurrentState != PlayerState.DodgeLeft && CurrentState != PlayerState.DodgeRight && CurrentState != PlayerState.Duck)
        {
            return;
        }

        CurrentDodgeTime += Time.deltaTime;

        if (CurrentDodgeTime < MinDodgeTime) {
            return;
        }

        if (CurrentDodgeTime > MaxDodgeTime || !isDodgeButtonHeld()) { 
            PlayTransitionFromDodge();
            return;
        }


    }

    private bool isDodgeButtonHeld()
    {
        if (CurrentState == PlayerState.DodgeLeft && CurrentHorizontalInput < 0) { 
            return true;
        }

        if (CurrentState == PlayerState.DodgeRight && CurrentHorizontalInput > 0)
        {
            return true;
        }

        if (CurrentState == PlayerState.Duck && CurrentVerticalInput < 0)
        {
            return true;
        }

        return false;
    }

    public void OnDodgeTimeEnd()
    {
        if (CurrentState != PlayerState.DodgeLeft && CurrentState != PlayerState.DodgeRight && CurrentState != PlayerState.Duck)
        {
            return;
        }

        CurrentDodgeTime = 0;
        CurrentDodgeCooldownRemaining = DodgeCooldownTime;
        CurrentState = PlayerState.Default;
        Anim.Play("Default");
    }

    public void PlayTransitionFromDodge()
    {
        if (CurrentState == PlayerState.DodgeLeft) {
            Anim.Play("DodgeLeftReturn");
            return;
        }

        if (CurrentState == PlayerState.DodgeRight)
        {
            Anim.Play("DodgeRightReturn");
            return;
        }

        if (CurrentState == PlayerState.Duck)
        {
            Anim.Play("DuckReturn");
            return;
        }
    }


    public void DodgeLeft()
    {
        if(CurrentState != PlayerState.Default)
        {
            return;
        }

        CurrentState = PlayerState.DodgeLeft;
        Anim.Play("DodgeLeft");
    }

    public void DodgeRight() {
        if (CurrentState != PlayerState.Default)
        {
            return;
        }


        CurrentState = PlayerState.DodgeRight;
        Anim.Play("DodgeRight");
    }

    public void Duck()
    {
        if (CurrentState != PlayerState.Default)
        {
            return;
        }

        CurrentState = PlayerState.Duck;
        Anim.Play("Duck");
    }
}

public enum PlayerState
{
    Default,
    DodgeLeft,
    DodgeRight,
    Duck,
    PunchLeftLow,
    PunchRightLow,
    PunchLeftHigh,
    PunchRightHigh,
}
