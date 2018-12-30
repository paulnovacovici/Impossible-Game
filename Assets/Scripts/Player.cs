using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public float jumpPower = 10.0f;
    Rigidbody2D myRigidBody;
    private bool isGrounded = false;
    private bool gameover = false;
    ChallengeController myChallengeController;
    public GameObject endExplosion;

	// Use this for initialization
	void Start () {
        myRigidBody = this.transform.GetComponent<Rigidbody2D>();
        myChallengeController = GameObject.FindObjectOfType<ChallengeController>();
    }
	
	void FixedUpdate () {
		if (!gameover && Input.GetKey(KeyCode.Space) && isGrounded)
        {
            myRigidBody.AddForce(Vector3.up * (jumpPower * myRigidBody.mass * myRigidBody.gravityScale * 20.0f));
        }
	}

    void GameOver()
    {
        gameover = true;
        myChallengeController.Gameover();
    }

    private void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Ground")
        {
            isGrounded = true;
        }
        if (collision.collider.tag == "Enemy")
        {
            Vector3 pos = transform.position;
            Destroy(gameObject);
            Instantiate(endExplosion, pos, Quaternion.identity);
            GameOver();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.tag == "Ground")
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == "Ground")
        {
            isGrounded = false;
        }
    }
}
