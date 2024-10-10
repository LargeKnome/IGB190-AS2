using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public string gateName;
    public bool startOpen = false;
    private Animator animator;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        if (startOpen)
        {
            SetOpen(true);
        }
    }

    public void SetOpen(bool isOpen)
    {
        if (isOpen)
        {
            animator.Play("Open");
        }
        else
        {
            animator.Play("Closed");
        }
    }
}
