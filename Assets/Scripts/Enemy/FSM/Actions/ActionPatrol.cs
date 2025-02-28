using System;
using System.Collections;
using UnityEngine;

public class ActionPatrol : FSMAction
{
    [Header("Configuration")]

    private Waypoint _waypoint;
    private int _pointIndex;

    private bool canGoToNextPoint = true;

    private EnemyAnimator _enemyAnimator;
    private EnemyStatistics _enemyStatistics;
    
    private void Awake()
    {
        _waypoint = GetComponent<Waypoint>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }

    public override void Act()
    {
        FollowPath();
    }

    private void FollowPath()
    {
        if (!canGoToNextPoint) return;
        
        var waypointTransform = GetWaypointPosition();
        var direction = ((Vector2)waypointTransform - (Vector2)transform.position).normalized;

        //ZOMBIE IDLE NIE MA ANIMACJI W LEWO        
        _enemyAnimator.FlipSpriteXOff();
        _enemyAnimator.SetIsMoving(true);
        _enemyAnimator.SetMoveAnimation(new Vector2(direction.x, direction.y));
        transform.position = Vector3.MoveTowards(transform.position, waypointTransform, _enemyStatistics.Speed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, waypointTransform) <= 0.1f && canGoToNextPoint)
        {
            _enemyAnimator.SetIsMoving(false);
            StartCoroutine(WaitAtThePoint());
            UpdateNextPosition();
        }
    }

    private void UpdateNextPosition()
    {
        _pointIndex++;
        if (_pointIndex > _waypoint.Points.Length - 1)
        {
            _pointIndex = 0;
        }
    }

    private Vector3 GetWaypointPosition()
    {
        return _waypoint.GetPosition(_pointIndex);
    }

    private IEnumerator WaitAtThePoint()
    {
        canGoToNextPoint = false;
        yield return new WaitForSeconds(3f);
        canGoToNextPoint = true;
    }

    private bool CheckIfShouldFlipX(float characterXPosition, float targetPointXPosition) => characterXPosition < targetPointXPosition;
}
