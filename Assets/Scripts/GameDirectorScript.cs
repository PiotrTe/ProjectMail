using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class GameDirectorScript : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;
    public GameObject[] MailBoxes;
    public int ObjectiveCounter = 0;
    bool ObjectiveComplete = false;
    // Start is called before the first frame update
    void Start()
    {
        // Find all game objects with the tag "Objective" and assign them to MailBoxes
        MailBoxes = GameObject.FindGameObjectsWithTag("Objective");
        
        if (objectiveText != null)
        {
            objectiveText.text = "Remaining Deliveries: " + ObjectiveCounter + "/" + MailBoxes.Length;
        }
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
    public void IncrementObjectiveCounter()
    {
        ObjectiveCounter++;

        // Update the UI text
        if (objectiveText != null)
        {
            objectiveText.text = "Remaining Deliveries: " + ObjectiveCounter + "/" + MailBoxes.Length;
        }
    }

    
}
