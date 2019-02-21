using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyable : MonoBehaviour
{
    public AudioClip swingSound;
    public AudioClip stabSound;
    public Sprite dmgSprite;
    public int health;

    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DamageDestroyable(int loss)
    {
        SoundManager.instance.RandomizeSfx(swingSound, stabSound);


        spriteRenderer.sprite = dmgSprite;
        health -= loss;

        if (health <= 0)
            gameObject.SetActive(false);

    }
}
