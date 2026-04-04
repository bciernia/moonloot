using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageTMP;

    private void Start()
    {
        var offsetX = Random.Range(-.5f, .5f);
        var offsetY = Random.Range(-.2f, .2f);

        transform.position += new Vector3(offsetX, offsetY, 0f);
    }

    public void SetDamageText(float damage)
    {
        damageTMP.text = damage.ToString();
    }

    public void DestroyText()
    {
        Destroy(gameObject);
    }
}
