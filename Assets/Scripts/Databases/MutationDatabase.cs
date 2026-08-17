using System.Collections.Generic;
using UnityEngine;

public class MutationDatabase : MonoBehaviour
{
    [SerializeField] private List<MutationData> _mutations;

    public IReadOnlyList<MutationData> Mutations => _mutations;
}