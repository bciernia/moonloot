using UnityEngine;

public class TestSpell : MonoBehaviour
{
    [SerializeField] private GameObject Spell;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            Instantiate(Spell, mouseWorldPos, Quaternion.identity);        }       
    }
}
