using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollSpawner : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            Instantiate(prefab, transform);
        }        
    }
}
