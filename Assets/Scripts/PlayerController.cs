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
    public float RapidSuperPunchWindowRemaining;
    public float RapidSuperPunchWindow = .5f;

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
        
        CurrentHorizontalInput = horizontalInput;
        CurrentVerticalInput = verticalInput;

        if (CurrentState == PlayerState.Default)
        {
            SetIsLow(verticalInput <= 0);
            
        }
       
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

        if (Input.GetKeyDown(KeyCode.T) && SingletonManager.Get<PowerMeterController>().IsMaxedOut())
        {
            if (verticalInput > 0)
            {
                SuperPunchHigh();
            }
            else
            {
                SuperPunchLow();
            }

            return;
        }
    }

    public void SuperPunchHigh()
    {
        Debug.LogWarning("Super Punch High!");

        if (CurrentState == PlayerState.RapidSuperPunchHigh) {
            return;
        }

        if (CurrentState == PlayerState.SuperPunchHigh && RapidSuperPunchWindowRemaining > 0) {
            RapidSuperPunchHigh();
            return;
        }

        if(CurrentState != PlayerState.SuperPunchHigh)
        {
            RapidSuperPunchWindowRemaining = RapidSuperPunchWindow;
            CurrentState = PlayerState.SuperPunchHigh;
            //TODO: Start Super Punch Animation
        }
    }

    public void RapidSuperPunchHigh()
    {
        Debug.LogWarning("Rapid Super Punch High!");
        RapidSuperPunchWindowRemaining = 0;
        CurrentState = PlayerState.RapidSuperPunchHigh;
        //TODO: Start Rapid Super Punch Animation
    }

    public void SuperPunchLow()
    {
        Debug.LogWarning("Super Punch Low!");

        if (CurrentState == PlayerState.RapidSuperPunchLow)
        {
            return;
        }

        if (CurrentState == PlayerState.SuperPunchLow && RapidSuperPunchWindowRemaining > 0)
        {
            RapidSuperPunchLow();
            return;
        }

        if (CurrentState != PlayerState.SuperPunchHigh)
        {
            RapidSuperPunchWindowRemaining = RapidSuperPunchWindow;
            CurrentState = PlayerState.SuperPunchLow;
            Anim.Play("SuperPunchLow");
        }
    }

    public void RapidSuperPunchLow()
    {
        Debug.LogWarning("Rapid Super Punch Low!");
        RapidSuperPunchWindowRemaining = 0;
        CurrentState = PlayerState.RapidSuperPunchLow;
        Anim.Play("SuperPunchLowRapidStart");
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

    public void OnSuperPunchLowHitFrame()
    {
        JayController.OnPlayerSuperPunchLow();
    }

    public void OnSuperPunchLowRapidLeftHitFrame()
    {
        JayController.OnPlayerSuperPunchLowRapid(isLeft: true);
    }

    public void OnSuperPunchLowRapidRightHitFrame()
    {
        JayController.OnPlayerSuperPunchLowRapid(isLeft: false);
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

    public void BlockHigh()
    {
        CurrentState = PlayerState.BlockHigh;
        Anim.Play("BlockHigh");
    }

    public void BlockLow()
    {
        CurrentState = PlayerState.BlockLow;
        Anim.Play("BlockLow");
    }

    public void OnOpponentPunchHighLeft()
    {
        if (CurrentState == PlayerState.DodgeLeft || CurrentState == PlayerState.DodgeRight || CurrentState == PlayerState.Duck)
        {
            Debug.LogWarning("Dodged High Left Punch!");
            return;
        }

        if (CurrentState == PlayerState.Default && !IsLow)
        {
            Debug.LogWarning("Blocked High Left Punch!");
            BlockHigh();
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

        if (CurrentState == PlayerState.Default && !IsLow) {
            Debug.LogWarning("Blocked High Right Punch!");
            BlockHigh();
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

    public bool OnOpponentPunchLowLeft()
    {

        if (CurrentState == PlayerState.DodgeLeft || CurrentState == PlayerState.DodgeRight)
        {
            Debug.LogWarning("Dodged Low Left Punch!");
            return false;
        }

        if (CurrentState == PlayerState.Default && IsLow)
        {
            Debug.LogWarning("Blocked Low Left Punch!");
            BlockLow();
            return false;
        }

        TakeDamage();
        CurrentState = PlayerState.staggerLeftLow;

        if (CurrentHealthPercent > 0)
        {
            Anim.Play("StaggerRightLow");
        }
        else
        {
            Anim.Play("FallRightLow");
        }

        return true;
    }

    public bool OnOpponentPunchLowRight()
    {

        if (CurrentState == PlayerState.DodgeLeft || CurrentState == PlayerState.DodgeRight)
        {
            Debug.LogWarning("Dodged Low Right Punch!");
            return false;
        }

        if (CurrentState == PlayerState.Default && IsLow)
        {
            Debug.LogWarning("Blocked Low Right Punch!");
            BlockLow();
            return false;
        }

        TakeDamage();
        CurrentState = PlayerState.staggerRightLow;

        if (CurrentHealthPercent > 0)
        {
            Anim.Play("StaggerLeftLow");
        }
        else
        {
            Anim.Play("FallLeftLow");
        }

        return true;
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
        SingletonManager.Get<PowerMeterController>().DecreaseLevel();
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
    BlockHigh,
    BlockLow,
    SuperPunchHigh,
    RapidSuperPunchHigh,
    SuperPunchLow,
    RapidSuperPunchLow,
}
