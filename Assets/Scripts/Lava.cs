using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour {

    private void Update()
    {
        Vector3 rotationVector = transform.rotation.eulerAngles;
        rotationVector.z = 0;
        transform.rotation = Quaternion.Euler(rotationVector);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<BoxCollider2D>());
        }
    }
}
