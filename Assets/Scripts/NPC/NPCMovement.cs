using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float moveSpeed;

    private Waypoint _waypoint;
    private EnemyAnimator _enemyAnimator;
    private Vector3 _previousPos;
    private int _currentPointIndex;
    private bool isWaiting = false;

    private void Awake()
    {
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _waypoint = GetComponent<Waypoint>();
    }

    private void Update()
    {
        if (isWaiting) return;
        
        var nextPosition = _waypoint.GetPosition(_currentPointIndex);
        UpdateMoveValues(nextPosition);
        transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, nextPosition) < 0.2f)
        {
            _previousPos = nextPosition;
            _currentPointIndex = (_currentPointIndex + 1) % _waypoint.Points.Length;

            StartCoroutine(WaitOnPoint());
        }
    }
    

    private void UpdateMoveValues(Vector3 nextPosition)
    {
        var direction = Vector2.zero;
        if (_previousPos.x < nextPosition.x && _previousPos.y < nextPosition.y) direction = new Vector2(1f, 1f);
        if (_previousPos.x > nextPosition.x && _previousPos.y < nextPosition.y) direction = new Vector2(-1f, 1f);
        if (_previousPos.x < nextPosition.x && _previousPos.y > nextPosition.y) direction = new Vector2(1f, -1f);
        if (_previousPos.x > nextPosition.x && _previousPos.y > nextPosition.y) direction = new Vector2(-1f, -1f);
        
        _enemyAnimator.SetMoveAnimation(new Vector2(direction.x, direction.y));
        _enemyAnimator.SetIsMoving(true);
    }

    private IEnumerator WaitOnPoint()
    {
        isWaiting = true;
        _enemyAnimator.SetIsMoving(false);
        yield return new WaitForSeconds(3f);
        isWaiting = false;
    }
}
