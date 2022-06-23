using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseDoor : MonoBehaviour
{
    [SerializeField] private Collider character;
    private Animator animator;
    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (character == other)
            animator.SetBool("character_nearby", true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (character == other)
            animator.SetBool("character_nearby", false);
    }
}
