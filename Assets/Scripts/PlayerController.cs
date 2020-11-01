using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed; // Movement speed of player
    public GameObject muzzleFlash;

    private Camera cameraObj; // Camera to follow player
    private Vector3 cameraDiff;
    private Rigidbody2D rb;

    GameController gameController; // Access to game functions
    Collider2D interactable = null; // Reference to an interactable object
    int score = 0; // How many enemies killed

    void Start()
    {
        cameraObj = Camera.main;
        cameraDiff = cameraObj.transform.position - transform.position;

        rb = GetComponent<Rigidbody2D>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        Physics2D.queriesHitTriggers = false; // Raycasts ignore triggers
    }

    void FixedUpdate()
    {
        movePlayer();
    }

    void Update()
    {
        // Player will look towards mouse cursor
        lookAtMouse();
        
        // If close to an interactable object, press SPACE to interact
        if (Input.GetButtonDown("Interact"))
        {
            if (interactable != null)
            {
                interactable.gameObject.SetActive(false);
                gameController.UpdateIntelCollected(true);
            }
        }
    }

    void LateUpdate()
    {
        // Camera follows player
        cameraObj.transform.position = transform.position + cameraDiff;
    }

    void movePlayer()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        rb.velocity = new Vector2(horizontal, vertical) * walkSpeed;
    }

    void lookAtMouse()
    {
        Vector3 mousePos = cameraObj.ScreenToWorldPoint(Input.mousePosition);
        Vector3 lookAt = new Vector3(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
        float zRot = Mathf.Atan2(lookAt.y, lookAt.x);

        // Rotate player to face mouse cursor
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, zRot * Mathf.Rad2Deg);

        // Fire a raycast bullet where the mouse cursor is pointing
        shootAtMouse(mousePos);
    }

    void shootAtMouse(Vector3 mousePosition)
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Vector3 position = Vector3.zero;

            foreach (Transform t in gameObject.GetComponentsInChildren<Transform>())
                if (t.gameObject.name == "MFSpawn")
                    position = t.position;

            // When firing, produce a muzzle flash like a real gun
            GameObject obj = Instantiate(muzzleFlash);
            obj.transform.position = position + new Vector3(0f, 0f, -1f); // Translate the instance to position on the sprite, which was set in the editor
            obj.transform.Rotate(transform.rotation.eulerAngles); // Make flash face in the direction with the player
            obj.GetComponent<MFAnimator>().SetVelocity(rb.velocity); // Fix weird desync when player is moving

            Vector2 direction = new Vector2(mousePosition.x - transform.position.x, mousePosition.y - transform.position.y);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, ~LayerMask.GetMask("Player"));
            if (hit.collider != null)
            {
                // If what the player shot was an enemy, kill them (or whatever their OnHit may do)
                // Yes, this is bad programming. We should be using interfaces or superclasses for these enemies.
                EnemyController enemy = hit.collider.GetComponent<EnemyController>();
                GunnedEnemy gunenemy = hit.collider.GetComponent<GunnedEnemy>();
                if (enemy != null)
                {
                    enemy.OnHit();

                    // Update kill count
                    score++;
                    gameController.UpdateScore(score);
                } 
                else if (gunenemy != null)
                {
                    gunenemy.OnHit();
                    score++;
                    gameController.UpdateScore(score);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Inform the user if they can interact with something
        if (collision.tag == "Pickup")
        {
            interactable = collision;
            gameController.UpdateCanInteract(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Stop informing the user if they can't interact with something
        if (collision.tag == "Pickup")
        {
            interactable = null;
            gameController.UpdateCanInteract(false);
        }
    }

    public void OnHit()
    {
        // Kill player
        gameObject.SetActive(false);
    }
}
