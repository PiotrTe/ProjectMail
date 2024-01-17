using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EndTriggerScript : MonoBehaviour
{
    public GameDirectorScript gameDirectorScript;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameDirectorScript.ObjectiveCounter == gameDirectorScript.MailBoxes.Length)
        {
            animator.SetBool("Objectives Complete", true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        gameDirectorScript.endTrigger = true;
    }
}
