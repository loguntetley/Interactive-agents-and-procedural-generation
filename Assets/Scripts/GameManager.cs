using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> instianteAtStart = new List<GameObject>();

    private void Start()
    {
        foreach (var gameObject in instianteAtStart)
        {
            Instantiate(gameObject);
        }
    }
}
