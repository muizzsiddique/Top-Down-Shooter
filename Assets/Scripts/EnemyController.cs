using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    enum State { Patrol, Wait, Chase } // Substitute for FSM

    Rigidbody2D rb;
    GameObject player; 

    State state = State.Wait; // Initial state
    bool canChangeState = false;
    IEnumerator patrolRoutine;

    Vector3 target; // Random position to move to
    bool inBoundary = true;

    float rotation = 0.0f;

    public Collider2D boundary;
    public float moveSpeed;
    public float viewRange; // View distance/How far to check for player

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player");

        // Begin patrolling
        patrolRoutine = DoPatrol(1f, 2f);
        StartCoroutine(patrolRoutine);
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Patrol:
                rb.velocity = (target - transform.position).normalized * moveSpeed;
                break;
            case State.Wait:
                rb.velocity = Vector3.zero;
                break;
            case State.Chase:
                rb.velocity = (player.transform.position - transform.position).normalized * moveSpeed;
                break;
        }

        // Rotate enemy based on movement
        if (rb.velocity != Vector2.zero)
            rotation = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = rotation;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position, viewRange, ~LayerMask.GetMask("Enemy"));
        bool canSeePlayer = hit.collider != null && hit.collider.name == "Player";

        // If it can see the player and it has room to walk, begin chase
        if (canChangeState && state != State.Chase && inBoundary && canSeePlayer)
        {
            state = State.Chase;
            StopCoroutine(patrolRoutine);
        }
        // If out of bounds, or can't see the player, stop chasing and begin patrol
        else if (state == State.Chase && !inBoundary && !canSeePlayer)
        {
            StartCoroutine(patrolRoutine);
        }
    }

    IEnumerator DoPatrol(float startTime, float waitTime)
    {
        canChangeState = false; // Not allowed to chase
        state = State.Wait;
        NextTarget(); // Choose a new position to walk to
        yield return new WaitForSeconds(startTime);

        canChangeState = true; // Can chase now
        while (true)
        {
            // Move to a point, wait a couple seconds, rinse and repeat
            state = State.Patrol;
            yield return new WaitUntil(() => (target - transform.position).magnitude < 0.3f);
            state = State.Wait;
            NextTarget();
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void OnHit()
    {
        StopCoroutine(patrolRoutine);
        GetComponentInParent<Transform>().gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == player)
        {
            // Attack player if within melee range
            player.GetComponent<PlayerController>().OnHit();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Collider2D>() == boundary)
        {
            // if out of bounds, walk back to patrol, ignoring player
            inBoundary = false;
            if (gameObject.activeSelf) StartCoroutine(patrolRoutine);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Collider2D>() == boundary)
        {
            inBoundary = true;
        }
    }

    void NextTarget()
    {
        target = new Vector3(
            Random.Range(boundary.bounds.min.x, boundary.bounds.max.x),
            Random.Range(boundary.bounds.min.y, boundary.bounds.max.y)
        );
    }
}
