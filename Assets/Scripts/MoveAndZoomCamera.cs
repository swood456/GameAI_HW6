using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAndZoomCamera : MonoBehaviour {

    Camera c;
    public float movespeed = 2.0f;

	// Use this for initialization
	void Start () {
        c = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 delta = Vector2.zero;
		if(Input.GetKey(KeyCode.RightArrow))
        {
            delta.x += movespeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            delta.x -= movespeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            delta.y += movespeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            delta.y -= movespeed * Time.deltaTime;
        }

        transform.position += (Vector3)delta;

        // zoom!
        if(Input.GetKeyDown(KeyCode.Minus))
        {
            c.orthographicSize = c.orthographicSize + 1;
        }
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            c.orthographicSize = Mathf.Max(1, c.orthographicSize - 1);
        }
    }
}
