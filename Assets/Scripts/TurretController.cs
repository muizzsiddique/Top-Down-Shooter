using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    GameObject player;
    float charge = 0.0f;
    float health = 100.0f;

    public Rigidbody2D gun;
    public float maxCharge;

    void Start()
    {
        player = GameObject.Find("Player");
    }

    void Update()
    {
        Vector3 toPlayer = player.transform.position - transform.position;

        Debug.Log(charge);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer, Mathf.Infinity, ~LayerMask.GetMask("Enemy"));
        if (hit.collider != null && hit.collider.gameObject == player)
        {
            gun.rotation = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

            charge = Mathf.Clamp(charge + 1.0f * Time.deltaTime, 0.0f, maxCharge);

            if (charge >= maxCharge)
            {
                charge = 0.0f;
                player.GetComponent<PlayerController>().OnHit();
            }
        }
        else
        {
            charge = Mathf.Clamp(charge - 1.0f * Time.deltaTime, 0.0f, maxCharge);
        }
    }

    public void OnHit()
    {
        health -= 10.0f;

        if (health <= 0.0f)
        {
            gameObject.SetActive(false);
        }
    }
}
