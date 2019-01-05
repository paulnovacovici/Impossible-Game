using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputNode : MonoBehaviour {
    SpriteRenderer m_SpriteRenderer;

    // If need to add more inputs make sure Length is at the end
    public enum Input { Empty, Enemy, Platform, Length }
    public float inp { get; set; }

    private void Start()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            //Set the main Color of the Material to red
            m_SpriteRenderer.color = Color.red;

            // Normalize
            inp = (float)Input.Enemy / (float)Input.Length;
        }
        else if (collision.tag == "Platform")
        {
            // Normalize
            m_SpriteRenderer.color = Color.blue;
            inp = (float) Input.Platform / (float) Input.Length;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" || collision.tag == "Platform")
        {
            m_SpriteRenderer.color = Color.black;
            inp = (float) Input.Empty;
        }
    }

    public void Reset()
    {
        inp = (float) Input.Empty;
        m_SpriteRenderer.color = Color.black;
    }
}
