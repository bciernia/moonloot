using System;
using UnityEngine;
using Input = UnityEngine.Windows.Input;

public class AttackManager : Singleton<AttackManager>
{
    public bool IsTriggered { get; set; }
}