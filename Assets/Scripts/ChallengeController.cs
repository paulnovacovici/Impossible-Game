using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeController : MonoBehaviour {

    public float scrollSpeed = 5.0f;
    public GameObject[] challenges;
    public float frequency = .5f;
    float counter = 0.0f;
    public Transform spawnPoint;
    ScoreScript scoreScript;
    private bool gameover = false;

	// Use this for initialization
	void Start () {
        scoreScript = GameObject.FindObjectOfType<ScoreScript>();
        GenerateRandomChallenge();
    }
	
	// Update is called once per frame
	void Update () {
        if (gameover)
        {
            return;
        }

        //Generate Objects
        if (counter <= 0.0f)
        {
            GenerateRandomChallenge();
        }
        else
        {
            counter -= Time.deltaTime * frequency;
        }

        GameObject currentChild;
        for (int i = 0; i < transform.childCount; i++)
        {
            currentChild = transform.GetChild(i).gameObject;
            ScrollChallenge(currentChild);

            if (currentChild.transform.position.x <= -15)
            {
                Destroy(currentChild);
            }
        }
	}


    void ScrollChallenge(GameObject currentChallenge)
    {
        currentChallenge.transform.position -= Vector3.right * (scrollSpeed * Time.deltaTime);
    }

    void GenerateRandomChallenge()
    {
        GameObject newChallenge = (GameObject) Instantiate(challenges[Random.Range(0, challenges.Length)], spawnPoint.transform.position, Quaternion.identity);
        newChallenge.transform.parent = transform;
        counter = 1.0f;
    }

    public void Gameover()
    {
        scoreScript.Gameover();
        gameover = true;
    }


    public void Reset()
    {
        foreach (Transform child in transform)
        {
            // Hack for oncollision exit not being called
            child.transform.position = new Vector3(100000000.0f, 10000000.0f, 10000000.0f);
            Destroy(child.gameObject);
        }
    }
}
