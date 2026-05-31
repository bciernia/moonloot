using System.Globalization;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI floatingTextTMP;

    private void Start()
    {
        var offsetX = Random.Range(-.5f, .5f);
        var offsetY = Random.Range(-.2f, .2f);

        transform.position += new Vector3(offsetX, offsetY, 0f);
    }

    public void SetDamageText(float damage)
    {
        floatingTextTMP.color = Color.red;
        floatingTextTMP.text = damage.ToString(CultureInfo.InvariantCulture);
    }
    
    public void SetHealText(float healAmount)
    {
        floatingTextTMP.color = Color.green;
        floatingTextTMP.text = healAmount.ToString(CultureInfo.InvariantCulture);
    }
    
    public void SetManaText(float manaAmount)
    {
        floatingTextTMP.color = Color.blue;
        floatingTextTMP.text = manaAmount.ToString(CultureInfo.InvariantCulture);
    }

    public void SetFloatingWarningText(string text)
    {
        floatingTextTMP.color = Color.yellow;
        floatingTextTMP.text = text;
    }
    
    public void SetFloatingGoldText(string text)
    {
        floatingTextTMP.color = Color.gold;
        floatingTextTMP.text = text;
    }
    
    public void SetFloatingText(string text)
    {
        floatingTextTMP.text = text;
    }

    public void DestroyText()
    {
        Destroy(gameObject);
    }
}
