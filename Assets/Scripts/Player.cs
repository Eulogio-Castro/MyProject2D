using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject
{
    public float restartLevelDelay = 1f;
    public int pointsPerPotion = 30;
    public int scorePerKill = 50;
    public int destroyableDamage = 1;
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public Text healthText;

    private Animator animator;
    private int maxHealth;
    private int health;
    private int score;
   


    // Start is called before the first frame update
    protected override void Start()
    {
        animator = GetComponent<Animator>();

        maxHealth = GameManager.instance.playerMaxHealth;
        health = GameManager.instance.playerHealth;
        score = GameManager.instance.playerScore;

        healthText.text = "Health: " + health;

        base.Start();

    }

    private void OnDisable()
    {
        GameManager.instance.playerScore = score;
        GameManager.instance.playerHealth = health;
        GameManager.instance.playerScore = score;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playersTurn)
            return;

        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)(Input.GetAxisRaw("Horizontal"));
        vertical = (int)(Input.GetAxisRaw("Vertical"));
        AttemptMove<Destroyable>(horizontal, vertical); 
    }


    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        base.AttemptMove<T>(xDir,yDir);
        RaycastHit2D hit;
        if(Move(xDir, yDir, out hit))
        {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2); 
        }

        GameManager.instance.playersTurn = false;

    }

    protected override void OnCantMove <T> (T component)
    {
        Destroyable hitDestroyable = component as Destroyable;
        hitDestroyable.DamageDestroyable(destroyableDamage);
        animator.SetTrigger("playerAttack");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Potion")
        {
            if (health < maxHealth)
            health += pointsPerPotion;
            collision.gameObject.SetActive(false);
        }

        if (collision.tag == "Exit")
        {
            StartCoroutine(NextLevel(collision.name, 1f));

            enabled = false;
        }


    }

    IEnumerator NextLevel(string name, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(name);
    }
}
