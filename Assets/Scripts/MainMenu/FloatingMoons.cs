using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D

public class FloatingMoonsArc : MonoBehaviour
{
    public float speed = 1f;               
    public float maxLightIntensity = 15f;  
    public GameObject[] moons;             

    public Transform leftPoint;            
    public Transform topPoint;             
    public Transform rightPoint;           

    private int currentMoon = 0;
    private float t = 0f;                  

    private Light2D moonLight;

    void Start()
    {
        foreach (GameObject moon in moons)
        {
            moon.transform.position = leftPoint.position;

            var sr = moon.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "MainMenu_Moon";
                sr.sortingOrder = 1;
            }

            moonLight = moon.GetComponentInChildren<Light2D>();
            if (moonLight != null)
                moonLight.intensity = 0f;

            moon.SetActive(false);
        }
    }

    void Update()
    {
        if (moons.Length == 0) return;

        GameObject moon = moons[currentMoon];
        moonLight = moon.GetComponentInChildren<Light2D>();

        if (!moon.activeSelf)
        {
            moon.transform.position = leftPoint.position;
            moon.SetActive(true);
            t = 0f;
        }

        t += speed * Time.deltaTime;

        Vector3 m1 = Vector3.Lerp(leftPoint.position, topPoint.position, t);
        Vector3 m2 = Vector3.Lerp(topPoint.position, rightPoint.position, t);
        Vector3 moonPos = Vector3.Lerp(m1, m2, t);
        moonPos.z = 1f;
        moon.transform.position = moonPos;

        var lightIntensity = 0f;
        if (t <= 0.5f)
            lightIntensity = Mathf.Lerp(0f, maxLightIntensity, t / 0.5f);
        else
            lightIntensity = Mathf.Lerp(maxLightIntensity, 0f, (t - 0.5f) / 0.5f);
        SetMoonLightIntensity(moonLight, lightIntensity);

        // --- Koniec animacji ---
        if (t >= 1f)
        {
            moon.SetActive(false);
            SetMoonLightIntensity(moonLight, 0f);

            currentMoon++;
            if (currentMoon >= moons.Length)
                currentMoon = 0;
        }
    }

    void SetMoonLightIntensity(Light2D light, float intensity)
    {
        if (light == null) return;
        light.intensity = intensity;
    }
}
