using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputNode : MonoBehaviour {
    public bool enemy = false;
    SpriteRenderer m_SpriteRenderer;

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
            enemy = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            m_SpriteRenderer.color = Color.black;
            enemy = false;
        }
    }

    public void Reset()
    {
        enemy = false;
        m_SpriteRenderer.color = Color.black;
    }
}
