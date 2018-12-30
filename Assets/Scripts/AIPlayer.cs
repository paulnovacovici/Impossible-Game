using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNet;
using System;

public class AIPlayer : MonoBehaviour {

    public float jumpPower = 3f;
    Rigidbody2D myRigidBody;
    private bool isGrounded = false;
    private bool ended = false;
    ChallengeController myChallengeController;
    GameObject inputs;
    public GameObject endExplosion;
    public int jump;
    float[] ini;
    public int numJumps;

    public NN nn = new NN(12, 4);
    AICtrl C;
    public float fitness;
    public string brain;
    List<Collision2D> colliders = new List<Collision2D>();

    // Use this for initialization
    void Start()
    {
        numJumps = 1;
        myRigidBody = transform.GetComponent<Rigidbody2D>();
        myChallengeController = GameObject.FindObjectOfType<ChallengeController>();
        inputs = GameObject.Find("Inputs");
        C = Camera.main.GetComponent<AICtrl>();

        tag = "Active";
    }

    void GameOver()
    {
        ended = true;
        tag = "Passive";
        nn.SetFitness(fitness);

        C.allPatt.Remove(this);

        CheckIfLast();
    }

    void CheckIfLast()
    {
        GameObject[] f = GameObject.FindGameObjectsWithTag("Active");

        if (f.Length == 0)
        {
            C.NewDay();
        }
    }

    private void FixedUpdate()
    {
        brain = nn.ReadBrain();

        if (isGrounded)
        {
            if (jump > 0)
            {
                numJumps++;
                myRigidBody.AddForce(Vector3.up * (jumpPower * myRigidBody.mass * myRigidBody.gravityScale * 20.0f));
            }

            float[] inps = new float[inputs.transform.childCount];

            int i = 0;
            foreach (Transform child in inputs.transform)
            {
                inps[i] = child.gameObject.GetComponent<InputNode>().enemy ? 1 : 0;
                i++;
            }
            
            fitness = (ended) ? fitness : (ScoreScript.scoreValue/(float)Math.Pow(numJumps*10,2));

            jump = ended ? 0 : (nn.CalculateNN(inps));
        }
        
    }

    public void Reset()
    {
        for(int i = colliders.Count-1; i >= 0; i--)
        {
            OnCollisionExit2D(colliders[i]);
        }

        fitness = 0;
        nn.SetFitness(fitness);
        tag = "Active";
        isGrounded = false;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
        ended = false;
    }

    /// <summary>
    /// Set the Genetic Code for the attempt
    /// </summary>
    /// <param name="i"></param>
    public void SetBrain(float[] i)
    {
        ini = i;
        nn.IniWeights(ini);
    }

    private void RemovePlayer()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Rigidbody2D>().isKinematic = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<BoxCollider2D>());
            colliders.Add(collision);
        }

        if (collision.collider.tag == "Ground")
        {
            isGrounded = true;
        }
        if (collision.collider.tag == "Enemy")
        {
            Vector3 pos = transform.position;
            RemovePlayer();
            //Instantiate(endExplosion, pos, Quaternion.identity);
            GameOver();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
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
            colliders.Remove(collision);
            isGrounded = false;
        }
    }
}
