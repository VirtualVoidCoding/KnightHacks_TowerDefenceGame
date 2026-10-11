using UnityEngine;

public class EnemyLight : MonoBehaviour
{

    public float MaxHealth = 100f;
    public float CurrentHealth;
    public float speed = 5f;
    public int ID;

    public void Initialize()
    {
        CurrentHealth = MaxHealth;
        // Initialization code for EnemyLight
    }

}
