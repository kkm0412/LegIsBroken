using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BoostButton : MonoBehaviour
{
    public CarControl car; 
    //public float boostMultiplier = 2f; 
    public CarCrashStop carCrashStop;

    private bool boosted = false;

    //디버깅용
    public bool isPoked = false;

    void Update()
    {
        if(isPoked) //디버깅용
        {
            OnPokePressed(null);
            isPoked = false;
        }
    }

    public void OnPokePressed(SelectEnterEventArgs args)
    {
        if (boosted) return;

        boosted = true;
        car.isRunning = true;
        //carCrashStop.OnPressStartButton(); // 탑승 및 출발 호출
    }
    
}
