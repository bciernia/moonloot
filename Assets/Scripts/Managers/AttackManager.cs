using System;
using UnityEngine;
using Input = UnityEngine.Windows.Input;

public class AttackManager : MonoBehaviour
{
    public static AttackManager Instance;
    public bool IsTriggered { get; set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
}