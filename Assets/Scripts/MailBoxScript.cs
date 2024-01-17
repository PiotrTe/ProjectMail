using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MailBoxScript : MonoBehaviour
{
    public GameObject MailMan;
    GameDirectorScript GameDirector;
    Animator m_Animator;
    // Start is called before the first frame update
    void Start()
    {
        GameDirector = FindObjectOfType<GameDirectorScript>();
        m_Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameDirector.IncrementObjectiveCounter();
            m_Animator.SetBool("MailIn", true);
            Debug.Log("Mail put in mailbox");
        }
    }
}
