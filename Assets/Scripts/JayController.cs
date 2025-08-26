using UnityEngine;
using UnityEngine.UI;

public class JayController : MonoBehaviour
{
    public Animator Anim;
    public JayState CurrentJayState;
    public Slider HealthSlider;
    public float CurrentHealthPercent = 1f;

    public void OnDefaultAnimationStateEnter()
    {
        CurrentJayState = JayState.Default;
    }

    public void OnPlayerPunchLeftLow()
    {
        if(CurrentJayState != JayState.Default)
        {
            return;
        }

        TakeDamage();
        CurrentJayState = JayState.StaggerRightLow;
        
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

        TakeDamage();
        CurrentJayState = JayState.StaggerRightHigh;
  
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

        TakeDamage();
        CurrentJayState = JayState.StaggerLeftLow;

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

        TakeDamage();
        CurrentJayState = JayState.StaggerLeftHigh;

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
}

public enum JayState
{
    Default,
    StaggerLeftLow,
    StaggerRightLow,
    StaggerLeftHigh,
    StaggerRightHigh,
    Falling,
}
