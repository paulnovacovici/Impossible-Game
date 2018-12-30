using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Days : MonoBehaviour {
    public static int days = 0;
    Text text;

    // Use this for initialization
    void Start () {
        text = GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        text.text = "Days " + days;
	}
}
