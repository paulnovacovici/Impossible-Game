using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour {
    public static int scoreValue = 0;
    Text score;
    bool gameover = false;

	// Use this for initialization
	void Start () {
        score = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!gameover)
        {
            scoreValue += (int)(Time.deltaTime * 100);
            score.text = "Score: " + scoreValue;
        }
	}

    public void Gameover()
    {
        gameover = true;
    }

    public void Reset()
    {
        scoreValue = 0;
    }
}
