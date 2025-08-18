using UnityEngine;

public class CharacterClothesAnimator : MonoBehaviour
{
    public SpriteRenderer headRenderer;
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer handsRenderer;
    public SpriteRenderer pantsRenderer;
    public SpriteRenderer shoesRenderer;

    public Sprite[] headSprites;
    public Sprite[] bodySprites;
    public Sprite[] handsSprites;
    public Sprite[] pantsSprites;
    public Sprite[] shoesSprites;

    private int frame;

    void Update()
    {
        frame = (int)(Time.time * 10) % bodySprites.Length;

        headRenderer.sprite = headSprites[frame];
        bodyRenderer.sprite = bodySprites[frame];
        handsRenderer.sprite = handsSprites[frame];
        pantsRenderer.sprite = pantsSprites[frame];
        shoesRenderer.sprite = shoesSprites[frame];
    }
}
