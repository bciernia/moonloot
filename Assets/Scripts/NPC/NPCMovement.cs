using System;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float moveSpeed;

    private readonly int moveX = Animator.StringToHash("MoveX");
    private readonly int moveY = Animator.StringToHash("MoveY");

    private Waypoint _waypoint;
    private Animator _animator;
    private Vector3 _previousPos;
    private int _currentPointIndex;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _waypoint = GetComponent<Waypoint>();
    }

    private void Update()
    {
        var nextPosition = _waypoint.GetPosition(_currentPointIndex);
        UpdateMoveValues(nextPosition);
        transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, nextPosition) < 0.2f)
        {
            _previousPos = nextPosition;
            _currentPointIndex = (_currentPointIndex + 1) % _waypoint.Points.Length;
        }
    }
    

    private void UpdateMoveValues(Vector3 nextPosition)
    {
        var direction = Vector2.zero;
        if (_previousPos.x < nextPosition.x && _previousPos.y < nextPosition.y) direction = new Vector2(1f, 1f);
        if (_previousPos.x > nextPosition.x && _previousPos.y < nextPosition.y) direction = new Vector2(-1f, 1f);
        if (_previousPos.x < nextPosition.x && _previousPos.y > nextPosition.y) direction = new Vector2(1f, -1f);
        if (_previousPos.x > nextPosition.x && _previousPos.y > nextPosition.y) direction = new Vector2(-1f, -1f);
        
        _animator.SetFloat(moveX, direction.x);
        _animator.SetFloat(moveY, direction.y);
    }
}
