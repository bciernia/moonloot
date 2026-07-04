using UnityEngine;

public class ActiveEffect : MonoBehaviour
{
    protected float duration;
    protected float timer;
    private GameObject uiObject;

    private Effect Effect { get; set; }
    public GameObject VisualObject { get; set; }
    
    protected void Initialize(Effect effect, GameObject uiObject = null, GameObject visualObject = null)
    {
        duration = effect.Duration;
        this.uiObject = uiObject;
        Effect = effect;
        VisualObject = visualObject;
        timer = 0f;
    }

    protected virtual void Tick(float deltaTime) { }

    protected virtual void Update()
    {
        timer += Time.deltaTime;

        Tick(Time.deltaTime);

        if (timer >= duration)
        {
            Effect?.Expire(gameObject);

            DestroyUI();

            if (VisualObject != null)
                Destroy(VisualObject);

            StatusEffectUIManager.Instance.RemoveEffectUI(Effect.Name);

            Destroy(this);
        }
    }

    private void DestroyUI()
    {
        if (uiObject != null)
            Destroy(uiObject);
    }
}