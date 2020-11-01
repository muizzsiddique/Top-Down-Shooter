using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnedEnemy : MonoBehaviour
{
    // The same as EnemyController with altered code,
    // this should be handled properly using OOP principles

    enum State { Patrol, Wait, lookShoot, WaitBeforeShoot }

    Rigidbody2D rb;
    GameObject player;

    State state = State.Wait;
    bool canChangeState = false;
    IEnumerator patrolRoutine;
    IEnumerator ShootRoutine;

    Vector3 target;
    bool inBoundary = true;

    float rotation = 0.0f;

    public GameObject muzzleFlash;
    public Collider2D boundary;

    public float moveSpeed;
    public float viewRange;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player");

        patrolRoutine = DoPatrol(1f, 2f);
        ShootRoutine = ShootPlayer(1f, 2f);
        StartCoroutine(patrolRoutine);
    }


    void FixedUpdate()
    {
        switch (state)
        {
            case State.Patrol:
                rb.velocity = (target - transform.position).normalized * moveSpeed;
                if (rb.velocity != Vector2.zero)
                    rotation = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
                rb.rotation = rotation;
                break;
            case State.Wait:
                rb.velocity = Vector3.zero;
                if (rb.velocity != Vector2.zero)
                    rotation = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
                rb.rotation = rotation;
                break;
            case State.WaitBeforeShoot:
                lookAtEnemy();
                break;
            case State.lookShoot:
                lookAtEnemy();

                Vector3 toPlayer = player.transform.position - transform.position;
                float shootAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg + Random.Range(-20.0f, +20.0f);
                float shootX = Mathf.Cos(shootAngle * Mathf.Deg2Rad);
                float shootY = Mathf.Sin(shootAngle * Mathf.Deg2Rad);

                RaycastHit2D enemyHit = Physics2D.Raycast(transform.position, new Vector3(shootX, shootY), viewRange, ~LayerMask.GetMask("Enemy"));
                MuzzleFlash();//Muzzle Flash for the gunned enemy
                if (enemyHit.collider != null && enemyHit.collider.gameObject == player)
                {
                    player.GetComponent<PlayerController>().OnHit();
                }
                break;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position, viewRange, ~LayerMask.GetMask("Enemy"));
        bool canSeePlayer = hit.collider != null && hit.collider.name == "Player";

        if (canChangeState && state != State.lookShoot && canSeePlayer)
        {
            state = State.lookShoot;
            StopCoroutine(patrolRoutine);
        }
        else if (state == State.lookShoot && !canSeePlayer)
        {
            StartCoroutine(patrolRoutine);
        }
        else if (state == State.lookShoot && canSeePlayer)
        {
            StartCoroutine(ShootRoutine);
        }
    }

    public void lookAtEnemy()
    {
        rb.velocity = Vector3.zero;
        Vector3 direction = player.transform.position - transform.position; //Getting the 3d direction for the player to look at.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; //Calculating the angle between the players direction and the enemies.
        rb.rotation = angle; //Making the rigidbody rotate to that angle
    }

    public void MuzzleFlash()
    {
        GameObject obj = Instantiate(muzzleFlash);
        Vector3 position = Vector3.zero;
        foreach (Transform t in gameObject.GetComponentsInChildren<Transform>())
            if (t.gameObject.name == "MFSpawn")
                position = t.position;
        obj.transform.position = position + new Vector3(0f, 0f, -1f);
        obj.transform.Rotate(transform.rotation.eulerAngles + new Vector3(0, 0, 90));
        obj.GetComponent<MFAnimator>().SetVelocity(rb.velocity);
    }

    IEnumerator ShootPlayer(float startTime, float waitTime)
    {
        canChangeState = false;
        state = State.WaitBeforeShoot;
        yield return new WaitForSeconds(startTime);

        canChangeState = true;
        while (true)
        {
            state = State.lookShoot;
            state = State.WaitBeforeShoot;
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator DoPatrol(float startTime, float waitTime)
    {
        canChangeState = false;
        state = State.Wait;
        NextTarget();
        yield return new WaitForSeconds(startTime);

        canChangeState = true;
        while (true)
        {
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
            player.GetComponent<PlayerController>().OnHit();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Collider2D>() == boundary)
        {
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
