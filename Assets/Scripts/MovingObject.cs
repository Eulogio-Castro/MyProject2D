using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    public float moveTime = 0.1f;
    public LayerMask blockingLayer;
    public LayerMask platformLayer;
    public float floatHeight;     // Desired floating height.
    public float liftForce;       // Force to apply when lifting the rigidbody.
    public float damping;         // Force reduction proportional to speed (reduces bouncing).


    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private float inverseMoveTime;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
    }

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;

        Vector2 end = start + new Vector2(xDir, yDir);
        Vector2 down = start + Vector2.down;

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        RaycastHit2D platformHit = Physics2D.Linecast(start, down, platformLayer);


        boxCollider.enabled = true;

        if (hit.transform == null)
        {
            if (platformHit.transform != null)
                StartCoroutine(JumpMovement());
            StartCoroutine(SmoothMovement(end));
            return true;
        }
        return false;
    }

    protected IEnumerator JumpMovement()
    {
        RaycastHit2D platformHit = Physics2D.Raycast(transform.position, Vector2.down);

            // If it hits something...
            if (platformHit.collider != null)
            {

                // Calculate the distance from the surface and the "error" relative
                // to the floating height
                float distance = Mathf.Abs(platformHit.point.y - transform.position.y);
                float heightError = floatHeight - distance;

                // The force is proportional to the height error, but we remove a part of it
                // according to the object's speed.
                float force = liftForce * heightError - rb2D.velocity.y * damping;

                // Apply the force to the rigidbody.
                rb2D.AddForce(Vector3.up * force);
            }

            yield return null;
    }


    protected IEnumerator SmoothMovement(Vector3 end)
    {
        //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
        //Square magnitude is used instead of magnitude because it's computationally cheaper.
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        //While that distance is greater than a very small amount (Epsilon, almost zero):
        while (sqrRemainingDistance > float.Epsilon)
        {
            //Find a new position proportionally closer to the end, based on the moveTime
            Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

            //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
            rb2D.MovePosition(newPostion);

            //Recalculate the remaining distance after moving.
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            //Return and loop until sqrRemainingDistance is close enough to zero to end the function
            yield return null;
        }
    }

    protected virtual void AttemptMove <T> (int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;

        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null)
            return;
        T hitComponent = hit.transform.GetComponent<T>();

        if (!canMove && hitComponent != null)
            OnCantMove(hitComponent);

    }

    protected abstract void OnCantMove <T> (T component)
        where T : Component;

}
