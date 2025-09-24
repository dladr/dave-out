using Assets.Scripts.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JayController : MonoBehaviour
{
    public Animator Anim;
    public JayState CurrentJayState;
    public Slider HealthSlider;
    public float CurrentHealthPercent = 1f;
    public PlayerController PlayerController;
    public RingController RingController;
    public bool IsLow;
    public bool IsBlocking;

    public bool AutoAttack;
    public float TimeUntilNextAttack;
    public float MinTimeUntilNextAttack;
    public float MaxTimeUntilNextAttack;

    public bool CanParryAttack;
    public List<PlayerState> ParryPlayerStates = new List<PlayerState>();


    private void Awake()
    {
        PlayerController = SingletonManager.Get<PlayerController>();
        RingController = SingletonManager.Get<RingController>();
        ResetAttackTime();
    }

    public void SetCanParryTrue()
    {
        CanParryAttack = true;
    }

    public void SetCanParryFalse()
    {
        CanParryAttack = false;
    }

    public void ResetAttackTime()
    {
        TimeUntilNextAttack = Random.Range(MinTimeUntilNextAttack, MaxTimeUntilNextAttack);
    }

    public void UpdateAttackTime() {
        if (!AutoAttack) {
            return;
        }

        if(CurrentJayState != JayState.Default || IsBlocking)
        {
            ResetAttackTime();
            return;
        }

        TimeUntilNextAttack -= Time.deltaTime;
        if (TimeUntilNextAttack <= 0) { 
            DoNextAttack();
            ResetAttackTime() ;
        }
    }

    public void DoNextAttack()
    {
        var randomBool = Random.Range(0, 2) > 0;
        if (randomBool)
        {
            PunchHighLeft();
        }
        else {
            PunchHighRight();
        }

    }

    private void Update()
    {
        UpdateAttackTime();
        if(Input.GetKeyDown(KeyCode.O))
        {
            SetIsLow(false);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SetIsLow(true);
        }

        if (Input.GetKeyDown(KeyCode.I)) {
            if (IsLow)
            {
                {
                    PunchLowLeft();
                }
            }
            else
            {
                PunchHighLeft();
            }
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (IsLow)
            {
                {
                    PunchLowRight();
                }
            }
            else
            {
                PunchHighRight();
            }

            return;
        }
    }

    public void SetIsLow(bool isLow)
    {
        IsLow = isLow;
        Anim.SetBool("IsLow", isLow);
    }

    public void OnLeftHighPunchContactFrame()
    {
        SetCanParryFalse();
        PlayerController.OnOpponentPunchHighLeft();
    }

    public void OnRightHighPunchContactFrame()
    {
        SetCanParryFalse();
        PlayerController.OnOpponentPunchHighRight();
    }

    public void OnLeftLowPunchContactFrame()
    {
        SetCanParryFalse();
        if (PlayerController.OnOpponentPunchLowLeft()) {
            Anim.Play("PunchLeftLowFollowThrough");
        }
    }

    public void OnRightLowPunchContactFrame()
    {
        SetCanParryFalse();
        if (PlayerController.OnOpponentPunchLowRight())
        {
            Anim.Play("PunchRightLowFollowThrough");
        }
    }

    public void OnDefaultAnimationStateEnter()
    {
        if(CurrentJayState == JayState.PunchingHighLeft)
        {
            
        }

        if (CurrentJayState == JayState.PunchingHighRight)
        {

        }

        if (CurrentJayState == JayState.StaggerLeftLow || CurrentJayState == JayState.StaggerRightLow)
        {
            SetIsLow(true);
        }

        if (CurrentJayState == JayState.StaggerLeftHigh || CurrentJayState == JayState.StaggerRightHigh)
        {
            SetIsLow(false);
        }


        CurrentJayState = JayState.Default;
        IsBlocking = false;
    }

    public void BlockLow()
    {
        Anim.Play("BlockLow", 0, 0);
        IsBlocking = true;
        //CurrentJayState = JayState.BlockingLow;
    }

    public void BlockHigh()
    {
        Anim.Play("BlockHigh", 0, 0);
        IsBlocking = true;
        //CurrentJayState = JayState.BlockingHigh;
    }

    public void OnPlayerPunchLeftLow()
    {
        var isParry = CheckForParry(PlayerState.PunchLeftLow);
        if (!isParry)
        {
            if (CurrentJayState != JayState.Default)
            {
                return;
            }

            if (IsLow)
            {
                BlockLow();
                return;
            }
        }

        TakeDamage();
        CurrentJayState = JayState.StaggerRightLow;
        RingController.MoveRingPosition(true);
        
        if (CurrentHealthPercent > 0)
        {
            Anim.Play("StaggerLowRight");
        }
        else
        {
            Anim.Play("FallLowRight");
        }
    }

    public void OnPlayerSuperPunchLow()
    {
        if (CurrentHealthPercent <= 0)
        {
            return;
        }

        var isParry = CheckForParry(PlayerState.PunchLeftLow);
        if (!isParry)
        {
            if (CurrentJayState != JayState.Default)
            {
                return;
            }

            if (IsLow)
            {
                BlockLow();
                return;
            }
        }

        TakeDamage(.2f);
        CurrentJayState = JayState.StaggerRightLow;
        RingController.MoveRingPosition(true);

        if (CurrentHealthPercent > 0)
        {
            Anim.Play("StaggerLowRightSuper");
        }
        else
        {
            Anim.Play("FallLowRight");
        }
    }

    public void OnPlayerSuperPunchLowRapid(bool isLeft)
    {
        if(CurrentHealthPercent <= 0)
        {
            return;
        }

        var isParry = CheckForParry(PlayerState.PunchLeftLow);
        if (!isParry)
        {
            if (CurrentJayState != JayState.Default)
            {
                return;
            }

            if (IsLow)
            {
                BlockLow();
                return;
            }
        }

        TakeDamage(.1f);
        CurrentJayState = isLeft ? JayState.StaggerRightLow : JayState.StaggerLeftLow;
        RingController.MoveRingPosition(true);

        if (isLeft)
        {
            if (CurrentHealthPercent > 0)
            {
                Anim.Play("StaggerLowRight");
            }
            else
            {
                Anim.Play("FallLowRight");
            }
        }
        else
        {
            if (CurrentHealthPercent > 0)
            {
                Anim.Play("StaggerLowLeft");
            }
            else
            {
                Anim.Play("FallLowLeft");
            }
        }

    }

    public void OnPlayerPunchLeftHigh()
    {
        var isParry = CheckForParry(PlayerState.PunchLeftHigh);
        if (!isParry)
        {
            if (CurrentJayState != JayState.Default)
            {
                return;
            }

            if (!IsLow)
            {
                BlockHigh();
                return;
            }
        }

        TakeDamage();
        CurrentJayState = JayState.StaggerRightHigh;
        RingController.MoveRingPosition(true);

        if (CurrentHealthPercent > 0)
        {
            Anim.Play("StaggerHighRight");
        }
        else
        {
            Anim.Play("FallHighRight");
        }
    }

    public bool CheckForParry(PlayerState playerAttackState)
    {
        if(!CanParryAttack)
        {
            return false;
        }

        if (ParryPlayerStates.Contains(playerAttackState)) { 
            return true;
        }

        return false;
    }

    public void OnPlayerPunchRightLow()
    {
        var isParry = CheckForParry(PlayerState.PunchRightLow);
        if(!isParry)
        {
            if (CurrentJayState != JayState.Default)
            {
                return;
            }

            if (IsLow)
            {
                BlockLow();
                return;
            }
        }
      

        TakeDamage();
        CurrentJayState = JayState.StaggerLeftLow;
        RingController.MoveRingPosition(false);

        if (CurrentHealthPercent > 0)
        {
            Anim.Play("StaggerLowLeft");
        }
        else
        {
            Anim.Play("FallLowLeft");
        }
    }

    public void OnPlayerPunchRightHigh()
    {
        var isParry = CheckForParry(PlayerState.PunchRightHigh);
        if (!isParry)
        {
            if (CurrentJayState != JayState.Default)
            {
                return;
            }

            if (!IsLow)
            {
                BlockHigh();
                return;
            }
        }

        TakeDamage();
        CurrentJayState = JayState.StaggerLeftHigh;
        RingController.MoveRingPosition(false);

        if (CurrentHealthPercent > 0)
        {
            Anim.Play("StaggerHighLeft");
        }
        else {
            Anim.Play("FallHighLeft");
        }
    }

    private void TakeDamage(float damage = .05f)
    {
        CurrentHealthPercent -= damage;
        if(CurrentHealthPercent <= 0f)
        {
            CurrentHealthPercent = 0f;
            Debug.LogWarning("Jay should fall down now!");
        }

        HealthSlider.value = CurrentHealthPercent;
        SingletonManager.Get<PowerMeterController>().IncreaseLevel();
    }

    private void ResetParries()
    {
        CanParryAttack = false;
        ParryPlayerStates = new List<PlayerState>();
    }

    private void PunchHighLeft()
    {
        if (CurrentJayState != JayState.Default)
        {
            return;
        }

        ResetParries();
        ParryPlayerStates.Add(PlayerState.PunchLeftHigh);

        CurrentJayState = JayState.PunchingHighLeft;
        Anim.Play("PunchLeftHigh");
    }

    private void PunchLowLeft()
    {
        if (CurrentJayState != JayState.Default)
        {
            return;
        }

        ResetParries();
        ParryPlayerStates.Add(PlayerState.PunchLeftLow);

        CurrentJayState = JayState.PunchingLowLeft;
        Anim.Play("PunchLeftLow");
    }

    private void PunchHighRight()
    {
        if (CurrentJayState != JayState.Default)
        {
            return;
        }

        ResetParries();
        ParryPlayerStates.Add(PlayerState.PunchRightHigh);

        CurrentJayState = JayState.PunchingHighRight;
        Anim.Play("PunchRightHigh");
    }

    private void PunchLowRight()
    {
        if (CurrentJayState != JayState.Default)
        {
            return;
        }

        ResetParries();
        ParryPlayerStates.Add(PlayerState.PunchRightLow);

        CurrentJayState = JayState.PunchingLowRight;
        Anim.Play("PunchRightLow");
    }

    public void MoveRingRandom()
    {
        var isRight = Random.value > .5f;
        RingController.MoveRingPosition(isRight);
    }
}

public enum JayState
{
    Default,
    StaggerLeftLow,
    StaggerRightLow,
    StaggerLeftHigh,
    StaggerRightHigh,
    Falling,
    PunchingHighLeft,
    PunchingHighRight,
    PunchingLowLeft,
    PunchingLowRight,
    BlockingHigh,
    BlockingLow,
}
