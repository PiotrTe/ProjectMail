using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDirectorScript : MonoBehaviour
{
    public GameObject[] MailBoxes;
    public int ObjectiveCounter = 0;
    bool ObjectiveComplete = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ObjectiveComplete == false)
        {
            if (ObjectiveCounter == MailBoxes.Length)
            {
                Debug.Log("Objective Complete!");
                ObjectiveComplete = true;

            }
        }
    }
}
