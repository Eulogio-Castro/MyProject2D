using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject
{
    public float restartLevelDelay = 1f;
    public float attackDelay = 1f;
    public int pointsPerPotion = 30;
    public int scorePerKill = 50;
    public int destroyableDamage = 1;
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip attackSource;
    public Text healthText;
    public Text maxText;
    public Slider slider;

    private Animator animator;
    private SpriteRenderer spriterenderer;
    private Rigidbody2D rigid2D;
    private BoxCollider2D bCollider;
    private GameObject floor;
    private Transform floorTransform;
    private int maxHealth;
    private int health;
    private int score;
    private int vertical;
    private int horizontal;
    private bool isAttacking;
    private string lastDir; //last direction travelled to give orientation to sprite
   


    // Start is called before the first frame update
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        spriterenderer = GetComponent<SpriteRenderer>();
        rigid2D = GetComponentInParent<Rigidbody2D>();
        floor = GameObject.Find("Floor Transform");
        floorTransform = floor.transform;

        maxHealth = GameManager.instance.playerMaxHealth;
        health = GameManager.instance.playerHealth;
        score = GameManager.instance.playerScore;

        slider.value = health;
        healthText.text = health.ToString();
        maxText.text = "/ " + maxHealth.ToString();

        base.Start();
        isAttacking = false;

    }

    private void OnDisable()
    {
        GameManager.instance.playerScore = score;
        GameManager.instance.playerHealth = health;
        GameManager.instance.playerScore = score;
    }

    // Update is called once per frame
    void Update()    {
        if (!GameManager.instance.playersTurn)
            return;

        GetAxes();
            //AttemptMove<Destroyable>(horizontal, vertical); 

        if(horizontal!=0)
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);

        if (rigid2D.velocity.magnitude < maxSpeed)
            rigid2D.AddForce(new Vector3(Input.GetAxisRaw("Horizontal") * moveTime, 0));

        if (horizontal !=0)
        {
            if (horizontal < 0)
                lastDir = "Left";
            if (horizontal > 0)
                lastDir = "Right";
            animator.SetTrigger("PlayerWalk");
            if (lastDir == "Left")
                spriterenderer.flipX = true;
            if (lastDir == "Right")
                spriterenderer.flipX = false;
        }

        Jump();

        bool attack = Input.GetButton("Jump");
        
        if ((attack) && (!isAttacking))
        {
            StartCoroutine(Attack(attack, attackDelay));
        }
       
        
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
            {
                if (maxHealth - health < pointsPerPotion)
                    health = maxHealth;
                else
                    health += pointsPerPotion;
            }
            collision.gameObject.SetActive(false);
        }

        if (collision.tag == "Exit")
        {
            StartCoroutine(NextLevel(collision.name, 1f));

            enabled = false;
        }

        healthText.text = health.ToString();
        slider.value = health;
    }

   void GetAxes()
    {
        horizontal = 0;
        vertical = 0;

        horizontal = (int)(Input.GetAxisRaw("Horizontal"));
        vertical = (int)Input.GetAxisRaw("Vertical");
    }


    void Jump()
    {
        Vector2 start = transform.position;
        RaycastHit2D platformHit = Physics2D.Linecast(transform.position, floorTransform.position, platformLayer.value);
        
        if (vertical > 0 && platformHit.collider != null)
        {
            float distance = Mathf.Abs(platformHit.point.y - transform.position.y);
            float heightError = floatHeight - distance;
            float force = liftForce * heightError - rigid2D.velocity.y * damping;
            animator.SetTrigger("PlayerJump");
            // Apply the force to the rigidbody.
            rigid2D.AddForce(Vector2.up * force);
        }
        
    }

    IEnumerator Attack(bool attack, float attackDelay)
    {
        if (attack)
        {
            isAttacking = true;
            animator.SetTrigger("PlayerAttack");
            SoundManager.instance.PlaySingle(attackSource);
            yield return new WaitForEndOfFrame();
            StartCoroutine(AttackingDelay(attackDelay));
        }
    }

    IEnumerator AttackingDelay(float attackDelay)
    {;
        yield return new WaitForSecondsRealtime(attackDelay);
        isAttacking = false;
    }

    IEnumerator NextLevel(string name, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(name);
    }
}
