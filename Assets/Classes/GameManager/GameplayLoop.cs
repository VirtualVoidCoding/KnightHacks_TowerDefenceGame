using UnityEngine;

public class GameplayLoop : MonoBehaviour
{
    public bool GameLoopEnd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    IEnumerator GameplayLoop()
    {
        while (GameLoopEnd == false)
        {

            //Spawn enemies

            //Spawn Towers

            //Move enemies

            //Tick towers

            //apply Effects

            //Damage enemies

            //Remove enemies

            //Remove towers

            yield return null;
        }
    }
}
