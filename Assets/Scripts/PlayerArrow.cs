using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArrow : MonoBehaviour {

    public Animator animator;
	
	
    public void ArrowSetOk()
    {
        animator.SetTrigger("Ok");
    }

    public void ArrowSetKo()
    {
        animator.SetTrigger("Ko");
    }

    public void ArrowSetQuiteOk()
    {
        animator.SetTrigger("QuiteOk");
    }



}
