using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerManager : MonoBehaviour
{
    [SerializeField] private GameObject container1;
    [SerializeField] private GameObject container2;

    void Start()
    {
        container1.SetActive(true);
        container2.SetActive(false);
    }

    public void OnContainer1ButtonPressed()
    {
        container2.SetActive(false);
        container1.SetActive(true);
    }

    public void OnContainer2ButtonPressed()
    {
        container1.SetActive(false);
        container2.SetActive(true);
    }
}
