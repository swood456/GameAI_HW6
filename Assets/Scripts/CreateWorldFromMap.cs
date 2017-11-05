using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CreateWorldFromMap : MonoBehaviour {

    [SerializeField]
    private TextAsset input_map;

    [SerializeField]
    private GameObject void_square;

    [SerializeField]
    private GameObject tree_square;

    [SerializeField]
    private GameObject empty_square;

    [SerializeField]
    private float image_square_size;

    [SerializeField]
    private int num_squares_per_tile;

    string type;
    int height;
    int width;
    GameObject map_root;

    // Use this for initialization
    void Start () {
        map_root = new GameObject("Map_Root");
        CreateMap();
	}
	
    private void CreateMap()
    {
        string[] lines = input_map.text.Split('\n');

        type = lines[0].Split(' ')[1];
        height = int.Parse(lines[1].Split(' ')[1]);
        width = int.Parse(lines[2].Split(' ')[1]);
        

        float left_side = -width * image_square_size / 2;
        float top = height * image_square_size / 2;
        for(int j = 4; j < lines.Length; ++j)
        {
            //print("doing row " + (j-3) +" of size " + lines[j].Length);
            for(int i = 0; i < width; ++i )
            {
                
                GameObject to_spawn;

                switch(lines[j][i])
                {
                    case '@':
                        to_spawn = void_square;
                        break;
                    case 'T':
                        to_spawn = tree_square;
                        break;
                    default:
                        to_spawn = empty_square;
                        break;
                }

                Instantiate(to_spawn, new Vector3(left_side + i * image_square_size, top - (j-4) * image_square_size, 0), Quaternion.identity, map_root.transform);
            }
        }
    }
}
