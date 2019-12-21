using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField]
    UnityEvent OnEnter;

    [SerializeField]
    UnityEvent OnExit;

    public void OnTriggerEnter(Collider other)
    {
        this.OnEnter.Invoke();
    }

    public void OnTriggerExit(Collider other)
    {
        this.OnExit.Invoke();
    }
}
