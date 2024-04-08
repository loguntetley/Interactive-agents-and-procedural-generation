using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YPositionLock : MonoBehaviour
{
    [SerializeField] private float yLock = 0.01f;
    void Update()
    {
        if (transform.position.y < yLock)
            transform.position = new Vector3(transform.position.x, yLock, transform.position.z);
    }
}
