using UnityEngine;

public class EnemyAim : MonoBehaviour
{
    public Transform firePoint; 
    public float rotateSpeed = 5f;
    
    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player == null) return;
        
        Vector3 direction = player.position - transform.position; 
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; 
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward); 
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotateSpeed * Time.deltaTime);
    }
}