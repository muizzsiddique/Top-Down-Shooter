using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MFAnimator : MonoBehaviour
{
    private Rigidbody2D rb;

    public float animationSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetVelocity(Vector3 velocity)
    {
        rb.velocity = velocity;
    }

    void Update()
    {
        transform.localScale -= new Vector3(1/animationSpeed, 1/animationSpeed) * Time.deltaTime;
        if (transform.localScale.x <= 0f)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
