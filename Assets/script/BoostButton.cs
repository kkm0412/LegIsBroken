using UnityEngine;

public class BoostButton : MonoBehaviour
{
    public CarControl car; 
    public float boostMultiplier = 2f; 

    private bool boosted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (boosted) return;

        if (other.CompareTag("Player"))
        {
            boosted = true;
            car.moveSpeed *= boostMultiplier;  
            Debug.Log("BOOST ACTIVATED!");
        }
    }
}
