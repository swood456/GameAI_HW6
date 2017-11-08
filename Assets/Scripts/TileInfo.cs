using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfo : MonoBehaviour {

    //static info
    public int x, y;
    public bool passable;
    public float free;
    private int size;

    //dynamic info
    private bool changed = true;
    public bool searched = false;
    private SpriteRenderer img;

    public void Start()
    {
        img = GetComponent<SpriteRenderer>();
        img.color = Color.black;
    }

    private void Update()
    {
        if (changed)
        {
            //update image

            //display sprite
            float s = (free * 255.0f) / 255.0f;
            Color newColor = Color.white;
            newColor.r = s;
            newColor.b = s;
            newColor.g = s;

            //modify the alpha value of the color based on the percent of free space in this tile
            img.color = newColor;

            changed = false;
        }
    }

    public void CreateFromChildren(int x_, int y_, TileInfo[] children)
    {
        x = x_;
        y = y_;
        int numChild = children.Length;

        float freeSpace = 0;
        foreach (TileInfo c in children)
        {
            freeSpace += c.free;
            
        }
        free = freeSpace / numChild;

        if (free > 0.6)
        {
            passable = true;
        }
        changed = true;
    }

    public void CreateEmpty(int x_, int y_)
    {
        x = x_;
        y = y_;
        passable = true;
        free = 1;
        changed = true;
    }

    public void CreateBlocked(int x_, int y_)
    {
        x = x_;
        y = y_;
        passable = false;
        free = 0;
        changed = true;
    }
}
