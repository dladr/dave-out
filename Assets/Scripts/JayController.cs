using Assets.Scripts.Helpers;
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

    private void Awake()
    {
        PlayerController = SingletonManager.Get<PlayerController>();
        RingController = SingletonManager.Get<RingController>();
        ResetAttackTime();
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
        var randomBool = Random.Range(0, 1) > 0;
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
            PunchHighLeft();
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            PunchHighRight();
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
        PlayerController.OnOpponentPunchHighLeft();
    }

    public void OnRightHighPunchContactFrame()
    {
        PlayerController.OnOpponentPunchHighRight();
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
        if(CurrentJayState != JayState.Default)
        {
            return;
        }

        if (IsLow) { 
            BlockLow();
            return;
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

    public void OnPlayerPunchLeftHigh()
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

    public void OnPlayerPunchRightLow()
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
        if (CurrentJayState != JayState.Default)
        {
            return;
        }

        if (!IsLow)
        {
            BlockHigh();
            return;
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

    private void TakeDamage()
    {
        CurrentHealthPercent -= .05f;
        if(CurrentHealthPercent <= 0f)
        {
            CurrentHealthPercent = 0f;
            Debug.LogWarning("Jay should fall down now!");
        }

        HealthSlider.value = CurrentHealthPercent;
    }

    private void PunchHighLeft()
    {
        if (CurrentJayState != JayState.Default)
        {
            return;
        }

        CurrentJayState = JayState.PunchingHighLeft;
        Anim.Play("PunchLeftHigh");
    }

    private void PunchHighRight()
    {
        if (CurrentJayState != JayState.Default)
        {
            return;
        }

        CurrentJayState = JayState.PunchingHighRight;
        Anim.Play("PunchRightHigh");
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
