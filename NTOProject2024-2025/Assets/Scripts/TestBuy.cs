using System;
using UnityEngine;

public class TestBuy : MonoBehaviour
{
    public Objective obj1;
    public Objective obj2;
    public Objective obj3;
    public Objective obj4;
    public int id;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (id == 0)
            {
                obj1.CompleteObjective();
            }
            else if (id == 1)
            {
                obj2.CompleteObjective();
                obj4.CompleteObjective();
            }  
            else if (id == 2)
            {
                obj3.CompleteObjective();
            } 
        }
    }
}
