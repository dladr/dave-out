using UnityEngine;

public class PowerMeterController : MonoBehaviour
{
    public MarkController MarkController;
    public Animator MeterAnimator;
    public int CurrentMeterLevel;
    public int MaxLevel = 27;

    public void IncreaseLevel(int increment = 4)
    {
        CurrentMeterLevel += increment;
        if(CurrentMeterLevel >= MaxLevel)
        {
            CurrentMeterLevel = MaxLevel;
            MeterAnimator.Play("MeterFlash");
        }

        MarkController.SetPowerLevel(CurrentMeterLevel);
    }

    public void DecreaseLevel(int decrement = 8)
    {
        MeterAnimator.Play("Empty");
        CurrentMeterLevel -= decrement;
        if (CurrentMeterLevel <= 0) {
            CurrentMeterLevel = 0;
        }

        MarkController.SetPowerLevel(CurrentMeterLevel);
    }

    public bool IsMaxedOut()
    {
        return CurrentMeterLevel >= MaxLevel;
    }
}
