using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public struct Point
{
    public int x;
    public int y;
    public float f;
    public float g;
    public float h;
    // TODO: parnets | C# doesn't like a point having a point variable :(
    public Point(int _x, int _y, float _f = 0.0f, float _g = 0.0f, float _h = 0.0f)
    {
        x = _x;
        y = _y;
        f = _f;
        g = _g;
        h = _h;
    }

    // equality nonsense | maybe don't need??
    //public override bool Equals(object obj)
    //{
    //    return obj is Point && this == (Point)obj;
    //}

    //public static bool operator ==(Point a, Point b)
    //{
    //    return a.x == b.x && a.y == b.y;
    //}

    //public static bool operator !=(Point a, Point b)
    //{
    //    return !(a == b);
    //}
}

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
            f_pos.z = -1.0f;

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

    public List<Point> FindPath()
    {
        List<Point> path = new List<Point>();
        if (!start_placed || !end_placed)
            return path;

        // first, locate what square the start and end is in
        int s_i, s_j, e_i, e_j;
        
        s_i = Mathf.RoundToInt((startPoint.transform.position.x - left_side) / image_square_size);
        s_j = -Mathf.RoundToInt((startPoint.transform.position.y - top) / image_square_size);

        e_i = Mathf.RoundToInt((endPoint.transform.position.x - left_side) / image_square_size);
        e_j = -Mathf.RoundToInt((endPoint.transform.position.y - top) / image_square_size);

        if(s_i < 0 || s_i >= width || s_j < 0 || s_j >= height)
        {
            print("error: placed start tile off map");
            return path;
        }

        if (e_i < 0 || e_i >= width || e_j < 0 || e_j >= height)
        {
            print("error: placed end tile off map");
            return path;
        }

        int s_x = s_i / num_squares_per_tile_x, s_y = s_j / num_squares_per_tile_y;
        if(map_representation[s_x, s_y] > tile_threshold_to_move)
        {
            print("placed start tile on a tile deemed unmovable");
            return path;
        }

        int e_x = e_i / num_squares_per_tile_x, e_y = e_j - 4 / num_squares_per_tile_y;
        if (map_representation[e_x, e_y] > tile_threshold_to_move)
        {
            print("placed end tile on a tile deemed unmovable");
            return path;
        }

        // now actually do a*
        print("I think I can I think I can!");

        List<Point> open_points = new List<Point>();
        open_points.Add(new Point(s_x, s_y));

        List<Point> close_points = new List<Point>();
        int count = 0;

        while(open_points.Count > 0 && count < 100)
        {
            count++; // there for debugging so we don't hit infinite loop

            // consider the best node in the open list
            Point best_point = FindSmallestInList(open_points);

            // if this node is the goal
            if(best_point.x == e_x && best_point.y == e_y)
            {
                break;
            }

            // else move the current node to the closest list and conisder all of its neighbors
            open_points.Remove(best_point);

            float cur_g = 0.0f; // do this!!!!

            List<Point> neighbors = GetAdjacentOpenTiles(best_point.x, best_point.y);
            foreach(Point p in neighbors)
            {
                Point successor = p;
                successor.g = best_point.g + 1;
                successor.h = HeuristicEuclidianDist(successor.x, successor.y, e_x, e_y);
                successor.f = successor.g + successor.h;

                //MIT psuedocode

                // I think this should find a point that has same x,y as sucessor?? - it does not
                // code modified from https://msdn.microsoft.com/en-us/library/5kthb929(v=vs.110).aspx
                Point? same = open_points.FindLast(
                    delegate (Point pt)
                    {
                        return p.x == successor.x && p.y == successor.y;
                    });

                if(same != null)
                {
                    //if a node with the same position as successor is in the OPEN list \
                    //which has a lower f than successor, skip this successor
                    if(same.Value.f < successor.f)
                    {
                        continue;
                    }
                    
                }

                //if a node with the same position as successor is in the CLOSED list \ 
                //    which has a lower f than successor, skip this successor
                same = close_points.FindLast(
                    delegate (Point pt)
                    {
                        return p.x == successor.x && p.y == successor.y;
                    });
                if(same != null && same.Value.f < successor.f)
                {
                    continue;
                }

                //otherwise, add the node to the open list
                open_points.Add(successor);


                //PROF SLIDE PSUEDOCODE:
                //if this neighbor is in the closest list and our current g value is lower
                // update the neighbor with the new, lower g value
                // change the neighbor's parent to our current node


                // else if this neighbor is in the open list and our current g value is lower
                // update the neighbor iwth the new, lower g value
                // change the neighbor's parent to our current node

                // else the neight is not iin either the open or closes list
                // add the neighbor to the open list and ste its g value
            }

            close_points.Add(best_point);


        }

        print("found a path! count = " + count);
        return path;
    }

    private Point FindSmallestInList(List<Point> l)
    {
        // never will have an empty list
        Point smallest_point = new Point(0,0);
        float smallest_cost = float.MaxValue;
        foreach(Point p in l)
        {
            if(p.f < smallest_cost)
            {
                smallest_point = p;
                smallest_cost = p.f;
            }
        }
        return smallest_point;
    }

    private List<Point> GetAdjacentOpenTiles(int x, int y)
    {
        List<Point> adjacent = new List<Point>();

        // left
        if(x > 0 && map_representation[x-1, y] < tile_threshold_to_move)
        {
            adjacent.Add(new Point(x-1,y));
        }

        // right
        if (x < width - 1 && map_representation[x + 1, y] < tile_threshold_to_move)
        {
            adjacent.Add(new Point(x + 1, y));
        }

        // up
        if (y > 0 && map_representation[x, y-1] < tile_threshold_to_move)
        {
            adjacent.Add(new Point(x, y - 1));
        }

        // down
        if (y < height-1 && map_representation[x, y + 1] < tile_threshold_to_move)
        {
            adjacent.Add(new Point(x, y + 1));
        }
        return adjacent;
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
