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
    private int num_squares_per_tile_x;

    [SerializeField]
    private int num_squares_per_tile_y;

    public GameObject startPoint;
    public GameObject endPoint;

    [SerializeField]
    private float tile_threshold_to_move = 0.5f;

    string type;
    int height;
    int width;
    int tile_map_width;
    int tile_map_height;
    float left_side;
    float top;
    GameObject map_root;
    float[,] map_representation;
    bool start_placed = false;
    bool end_placed = false;

    // Use this for initialization
    void Start () {
        map_root = new GameObject("Map_Root");
        CreateMap();
	}

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.E))
        {
            Camera c = Camera.main;
            Vector2 mouse_pos_world;
            mouse_pos_world.x = Input.mousePosition.x;
            mouse_pos_world.y = Input.mousePosition.y;

            Vector3 f_pos = c.ScreenToWorldPoint(new Vector3(mouse_pos_world.x, mouse_pos_world.y, c.nearClipPlane));
            f_pos.z = 0.0f;

            // TODO: make sure the space is within the map

            if (Input.GetKeyDown(KeyCode.S))
            {
                startPoint.transform.position = f_pos;
                start_placed = true;
            }
            else
            {
                endPoint.transform.position = f_pos;
                end_placed = true;
            }
        }
    }

    public void FindPath()
    {
        if (!start_placed || !end_placed)
            return;

        // first, locate what square the start and end is in
        int s_i, s_j, e_i, e_j;
        
        s_i = Mathf.RoundToInt((startPoint.transform.position.x - left_side) / image_square_size);
        s_j = -Mathf.RoundToInt((startPoint.transform.position.y - top) / image_square_size);

        e_i = Mathf.RoundToInt((endPoint.transform.position.x - left_side) / image_square_size);
        e_j = -Mathf.RoundToInt((endPoint.transform.position.y - top) / image_square_size);

        if(s_i < 0 || s_i >= width || s_j < 0 || s_j >= height)
        {
            print("error: placed start tile off map");
            return;
        }

        if (e_i < 0 || e_i >= width || e_j < 0 || e_j >= height)
        {
            print("error: placed end tile off map");
            return;
        }

        int s_x = s_i / num_squares_per_tile_x, s_y = s_j / num_squares_per_tile_y;
        if(map_representation[s_x, s_y] > tile_threshold_to_move)
        {
            print("placed start tile on a tile deemed unmovable");
            return;
        }

        int e_x = e_i / num_squares_per_tile_x, e_y = e_j - 4 / num_squares_per_tile_y;
        if (map_representation[e_x, e_y] > tile_threshold_to_move)
        {
            print("placed end tile on a tile deemed unmovable");
            return;
        }

        // now actually do a*
        print("I think I can I think I can!");
    }

    private float HeuristicEuclidianDist(int x, int y, int endX, int endY)
    {
        return Mathf.Sqrt((x - endX) * (x - endX) + (y - endY) * (y - endY));
    }

    private float HeuristicManhattanDist(int x, int y, int endX, int endY)
    {
        return Mathf.Abs(endX - x) + Mathf.Abs(endY - y);
    }

    private void CreateMap()
    {
        string[] lines = input_map.text.Split('\n');

        type = lines[0].Split(' ')[1];
        height = int.Parse(lines[1].Split(' ')[1]);
        width = int.Parse(lines[2].Split(' ')[1]);

        tile_map_width = width % num_squares_per_tile_x == 0 ? width / num_squares_per_tile_x : width / num_squares_per_tile_x + 1;
        tile_map_height = height % num_squares_per_tile_y == 0 ? height / num_squares_per_tile_y : height / num_squares_per_tile_y + 1;

        map_representation = new float[tile_map_width, tile_map_height];
        int tile_area = num_squares_per_tile_x * num_squares_per_tile_y;

        left_side = -width * image_square_size / 2;
        top = height * image_square_size / 2;
        for (int j = 4; j < lines.Length; ++j)
        {
            for(int i = 0; i < width; ++i )
            {
                
                GameObject to_spawn;
                int m_x = i / num_squares_per_tile_x, m_y = (j-4) / num_squares_per_tile_y;
                switch(lines[j][i])
                {
                    case '@':
                        to_spawn = void_square;
                        map_representation[m_x, m_y] += 1.0f / tile_area;
                        break;
                    case 'T':
                        to_spawn = tree_square;
                        map_representation[m_x, m_y] += 1.0f / tile_area;
                        break;
                    default:
                        to_spawn = empty_square;
                        break;
                }

                Instantiate(to_spawn, new Vector3(left_side + i * image_square_size, top - (j-4) * image_square_size, 0), Quaternion.identity, map_root.transform);
            }
        }

        //for(int j = 0; j < tile_map_width; ++j)
        //{
        //    for(int i = 0; i < tile_map_height; ++i)
        //    {
        //        print(map_representation[i,j]);
        //    }
        //}
    }
}
