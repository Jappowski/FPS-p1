using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParent : MonoBehaviour
{
    [SerializeField] private GameObject child;

    [SerializeField] private Transform parent;

    private void Start()
    {
        child.transform.SetParent(parent);
    }
}
