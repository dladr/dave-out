using Assets.Scripts.Helpers;
using UnityEngine;
using UnityEngine.UI;

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

    public JayController JayController;

    public Slider HealthSlider;
    public float CurrentHealthPercent = 1f;
    public bool IsLow = true;

    private void Awake()
    {
        JayController = SingletonManager.Get<JayController>();
    }

    private void Update()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        var isUpdatedHorizontalInput = horizontalInput != CurrentHorizontalInput;
        var isUpdatedVerticalInput = verticalInput != CurrentVerticalInput;
        SetIsLow(verticalInput <= 0);
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

        if (Input.GetKeyDown(KeyCode.F) && CurrentState == PlayerState.Default) {
            if (verticalInput > 0)
            {
                PunchLeftHigh();
            }
            else {
                PunchLeftLow();
            }
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.H) && CurrentState == PlayerState.Default)
        {
            if (verticalInput > 0)
            {
                PunchRightHigh();
            }
            else
            {
                PunchRightLow();
            }

            return;
        }
    }

    public void SetIsLow(bool isLow)
    {
        IsLow = isLow;
        Anim.SetBool("IsLow", isLow);
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

    public void OnPunchLeftLowHitFrame()
    {
        JayController.OnPlayerPunchLeftLow();
    }

    public void OnPunchRightLowHitFrame()
    {
        JayController.OnPlayerPunchRightLow();
    }

    public void OnPunchLeftHighHitFrame()
    {
        JayController.OnPlayerPunchLeftHigh();
    }

    public void OnPunchRightHighHitFrame()
    {
        JayController.OnPlayerPunchRightHigh();
    }

    public void OnDefaultStateEnter()
    {
        if (CurrentState == PlayerState.DodgeLeft || CurrentState == PlayerState.DodgeRight || CurrentState == PlayerState.Duck)
        {
            CurrentDodgeTime = 0;
            CurrentDodgeCooldownRemaining = DodgeCooldownTime;
            CurrentState = PlayerState.Default;
            Anim.Play("Default");
            return;
        }

        if (CurrentState == PlayerState.PunchLeftLow)
        {
            CurrentState = PlayerState.Default;
            Anim.Play("Default");
            return;
        }

        if (CurrentState == PlayerState.PunchRightLow)
        {
            CurrentState = PlayerState.Default;
            Anim.Play("Default");
            return;
        }

        if (CurrentState == PlayerState.PunchLeftHigh)
        {
            CurrentState = PlayerState.Default;
            Anim.Play("Default");
            return;
        }

        if (CurrentState == PlayerState.PunchRightHigh)
        {
            CurrentState = PlayerState.Default;
            Anim.Play("Default");
            return;
        }

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

    public void PunchLeftLow()
    {
        if (CurrentState != PlayerState.Default)
        {
            return;
        }

        CurrentState = PlayerState.PunchLeftLow;
        Anim.Play("PunchLeftLow");
    }

    public void PunchRightLow()
    {
        if (CurrentState != PlayerState.Default)
        {
            return;
        }

        CurrentState = PlayerState.PunchRightLow;
        Anim.Play("PunchRightLow");
    }

    public void PunchLeftHigh()
    {
        if (CurrentState != PlayerState.Default)
        {
            return;
        }

        CurrentState = PlayerState.PunchLeftHigh;
        Anim.Play("PunchLeftHigh");
    }


    public void PunchRightHigh()
    {
        if (CurrentState != PlayerState.Default)
        {
            return;
        }

        CurrentState = PlayerState.PunchRightHigh;
        Anim.Play("PunchRightHigh");
    }

    public void OnOpponentPunchHighLeft()
    {
        if (CurrentState == PlayerState.DodgeLeft || CurrentState == PlayerState.DodgeRight || CurrentState == PlayerState.Duck)
        {
            Debug.LogWarning("Dodged High Left Punch!");
            return;
        }

        TakeDamage();
        CurrentState = PlayerState.staggerRightHigh;

        if (CurrentHealthPercent > 0)
        {
            Anim.Play("StaggerRightHigh");
        }
        else
        {
            Anim.Play("FallRightHigh");
        }
    }

    public void OnOpponentPunchHighRight()
    {
        if (CurrentState == PlayerState.DodgeLeft || CurrentState == PlayerState.DodgeRight || CurrentState == PlayerState.Duck)
        {
            Debug.LogWarning("Dodged High Left Punch!");
            return;
        }

        TakeDamage();
        CurrentState = PlayerState.staggerLeftHigh;

        if (CurrentHealthPercent > 0)
        {
            Anim.Play("StaggerLeftHigh");
        }
        else
        {
            Anim.Play("FallLeftHigh");
        }
    }

    private void TakeDamage()
    {
        CurrentHealthPercent -= .05f;
        if (CurrentHealthPercent <= 0f)
        {
            CurrentHealthPercent = 0f;
            Debug.LogWarning("Player should fall down now!");
        }

        HealthSlider.value = CurrentHealthPercent;
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
    staggerLeftLow,
    staggerRightLow,
    staggerLeftHigh,
    staggerRightHigh,
}
