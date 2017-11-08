using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Point
{
    public int x;
    public int y;
    public float f;
    public float g;
    public float h;

    public Point parent;

    public Point(int _x, int _y, float _f = 0.0f, float _g = 0.0f, float _h = 0.0f)
    {
        x = _x;
        y = _y;
        f = _f;
        g = _g;
        h = _h;
        parent = null;
    }
}

public class CreateWorldFromMap : MonoBehaviour {

    public enum HeuristicType { Euclidian, Manhattan};
    public HeuristicType m_heruistic;

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
    public GameObject tile_outline;
    public GameObject redDot;

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

    float heuristic_weight;

    public void update_heuristic(float v)
    {
        heuristic_weight = v;
    }

    public void update_type(int i)
    {
        m_heruistic = (HeuristicType)i;
    }

    // Use this for initialization
    void Start () {
        map_root = new GameObject("Map_Root");
        CreateMap();
	}

    private void Update()
    {
        // set start and end points
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

    // Function that is run when you press the "Find Path" button
    public void print_path()
    {
        List<Vector2> l = FindPath();

        // could not reach the end for some reason
        if (l == null)
            return;

        // Give the player object a path that it will follow
        PlayerFollowGridPath player = FindObjectOfType<PlayerFollowGridPath>();

        // set up the player to move around
        player.enabled = true;
        player.transform.position = startPoint.transform.position;
        player.FollowPath(l, endPoint.transform.position);
    }
    
    public List<Vector2> FindPath()
    {
        // make sure that the start and end points are located
        if (!start_placed || !end_placed)
            return null;

        // Determine the square that the start and end points
        int s_i, s_j, e_i, e_j;
        
        s_i = Mathf.RoundToInt((startPoint.transform.position.x - left_side) / image_square_size);
        s_j = -Mathf.RoundToInt((startPoint.transform.position.y - top) / image_square_size);

        e_i = Mathf.RoundToInt((endPoint.transform.position.x - left_side) / image_square_size);
        e_j = -Mathf.RoundToInt((endPoint.transform.position.y - top) / image_square_size);

        // error if the start or end is outside of the square map
        if(s_i < 0 || s_i >= width || s_j < 0 || s_j >= height)
        {
            print("error: placed start tile off map");
            return null;
        }

        if (e_i < 0 || e_i >= width || e_j < 0 || e_j >= height)
        {
            print("error: placed end tile off map");
            return null;
        }

        // find the tile that the start space is in
        int s_x = s_i / num_squares_per_tile_x, s_y = s_j / num_squares_per_tile_y;
        if(map_representation[s_x, s_y] > tile_threshold_to_move)
        {
            print("placed start tile on a tile deemed unmovable");
            return null;
        }

        // find the tile that the end space is in
        int e_x = e_i / num_squares_per_tile_x, e_y = e_j / num_squares_per_tile_y;
        if (map_representation[e_x, e_y] > tile_threshold_to_move)
        {
            print("placed end tile on a tile deemed unmovable");
            return null;
        }

        // run A*

        // create the open points list and add the start
        List<Point> open_points = new List<Point>();
        open_points.Add(new Point(s_x, s_y));

        // create the empty closed points list
        List<Point> close_points = new List<Point>();

        while(open_points.Count > 0)
        {
            // Consider the point with the smallest f value
            Point best_point = FindSmallestInList(open_points);

            // if this node is the goal, make out path and stop
            if(best_point.x == e_x && best_point.y == e_y)
            {
                // make a list of vector2s to store the path
                List<Vector2> path = new List<Vector2>();

                // go through all the points and push them to the front of the list, then go to their parent
                Point p = best_point;
                while(p != null)
                {
                    // equations to get the x and y pos in worldspace for our tiles.
                    float x = left_side + (num_squares_per_tile_x - (1.0f + num_squares_per_tile_x) / 2) * image_square_size + p.x * num_squares_per_tile_x * image_square_size;
                    float y = top - (num_squares_per_tile_y - (1.0f + num_squares_per_tile_y) / 2) * image_square_size - p.y * num_squares_per_tile_y * image_square_size;

                    // push front of list
                    path.Insert(0, new Vector2(x, y));

                    // go to next node
                    p = p.parent;
                }

                return path;
            }

            // else move the current node to the closed list and conisder all of its neighbors

            // remove this tile from the open list. After, add it to closed list
            open_points.Remove(best_point);

            // get all the valid neighbors for this tile that we are looking at
            List<Point> neighbors = GetAdjacentOpenTiles(best_point.x, best_point.y);
            foreach(Point p in neighbors)
            {
                // make a new Point object for the successor
                Point successor = p;
                successor.g = best_point.g + 1;
                successor.h = Heuristic(successor.x, successor.y, e_x, e_y);
                successor.f = successor.g + successor.h;
                successor.parent = best_point;
                
                //PROF SLIDE PSUEDOCODE:
                //if this neighbor is in the closed list and our current g value is lower
                // update the neighbor with the new, lower g value
                // change the neighbor's parent to our current node

                // See if there is a point in the close_points list with the same xy
                Point same_close = FindXYInPointList(close_points, successor.x, successor.y);
                // if there is and it has a higher g than the sucessor node we created
                if(same_close != null)
                {
                    if(successor.g < same_close.g)
                    {
                        // update that node we found's g value
                        same_close.g = successor.g;

                        // update its parent
                        same_close.parent = best_point;
                        // don't do the other steps
                        continue;
                    }
                }
                // else if this neighbor is in the open list and our current g value is lower
                // update the neighbor with the new, lower g value
                // change the neighbor's parent to our current node
                Point same_open = FindXYInPointList(open_points, successor.x, successor.y);
                if (same_open != null){
                    if(successor.g < same_open.g)
                    {
                        same_open.g = successor.g;
                        same_open.parent = best_point;
                        continue;
                    }
                }

                // else the neighbor is not in either the open or closes list
                // add the neighbor to the open list and set its g value
                // making us the parent
                if(same_close == null && same_open == null)
                {
                    open_points.Add(successor);
                    // equations to get the x and y pos in worldspace for our tiles.
                    float x = left_side + (num_squares_per_tile_x - (1.0f + num_squares_per_tile_x) / 2) * image_square_size + successor.x * num_squares_per_tile_x * image_square_size;
                    float y = top - (num_squares_per_tile_y - (1.0f + num_squares_per_tile_y) / 2) * image_square_size - successor.y * num_squares_per_tile_y * image_square_size;

                    // make a red dot that signifies that we have started to look at this node but not yet considered its neighbors
                    GameObject g = Instantiate(redDot, new Vector3(x, y, -2), Quaternion.identity); 
                }
            }

            // Add tile to closed list
            close_points.Add(best_point);
        }

        // this means that we have looked at every node in open list and couldn't find a path. Probably want a slightly better UI thing for this
        print("could not find path :(");
        return null;
    }

    // helper function that finds if ther is a point in the list with the given X Y co-ords
    private Point FindXYInPointList(List<Point> l, int x, int y)
    {
        foreach(Point p in l)
        {
            if(p.x == x && p.y == y)
            {
                return p;
            }
        }

        return null;
    }

    // helper function that finds the point with the lowest F value in a list of points
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

    // helper function that determines all the valid neighbors to a given tile
    private List<Point> GetAdjacentOpenTiles(int x, int y)
    {
        // for now, I only look at up,down,left,right and I use a simple threshold for seeing if something is valid or not
        // this is a good place to do extra logic for seeing if a tile is valid
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

    // function that calculates the heuristic value for a tile at x,y given end at endX, endY
    //  this uses the m_heuristic variable to determine which function to use
    private float Heuristic(int x, int y, int endX, int endY)
    {
        // note: here is where we multiply by the weight, which can be set in the GUI
        switch (m_heruistic)
        {
            case HeuristicType.Euclidian:
                return heuristic_weight * HeuristicEuclidianDist(x, y, endX, endY);
            case HeuristicType.Manhattan:
                return heuristic_weight * HeuristicManhattanDist(x, y, endX, endY);
        }
        // in case something broke?
        return 0.0f;
    }

    // simple distance formula
    private float HeuristicEuclidianDist(int x, int y, int endX, int endY)
    {
        return Mathf.Sqrt((x - endX) * (x - endX) + (y - endY) * (y - endY));
    }

    // equation taken from slides
    private float HeuristicManhattanDist(int x, int y, int endX, int endY)
    {
        return Mathf.Abs(endX - x) + Mathf.Abs(endY - y);
    }

    // function that creates the world from an input file
    private void CreateMap()
    {
        // split the input text into lines
        string[] lines = input_map.text.Split('\n');

        // type I think is useless?
        type = lines[0].Split(' ')[1];
        // get the height and width of the map
        height = int.Parse(lines[1].Split(' ')[1]);
        width = int.Parse(lines[2].Split(' ')[1]);

        // determine the number of tiles that we need for the map. We also want to make sure that the far edge is not cut off
        tile_map_width = width % num_squares_per_tile_x == 0 ? width / num_squares_per_tile_x : width / num_squares_per_tile_x + 1;
        tile_map_height = height % num_squares_per_tile_y == 0 ? height / num_squares_per_tile_y : height / num_squares_per_tile_y + 1;

        // make a float 2d array to store the % of each tile that is covered
        map_representation = new float[tile_map_width, tile_map_height];

        // helper int that is just the area of each tile (for creating the map_rep above)
        int tile_area = num_squares_per_tile_x * num_squares_per_tile_y;

        // determine the left and right side of the map, so that the world is centered at or close to 0,0
        left_side = -width * image_square_size / 2;
        top = height * image_square_size / 2;

        // go through all the stuff in the input file
        for (int j = 4; j < lines.Length; ++j)
        {
            for(int i = 0; i < width; ++i )
            {
                // determine which of the tiles we are making
                GameObject to_spawn;

                // determine which tile we are in currently
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
                // make a square at the correct point
                Instantiate(to_spawn, new Vector3(left_side + i * image_square_size, top - (j-4) * image_square_size, 0), Quaternion.identity, map_root.transform);
            }
        }

        // set up the tiles
        for(int j = 0; j < tile_map_height; ++j)
        {
            for(int i = 0; i < tile_map_width; ++i)
            {
                // equations to get the x and y pos in worldspace for our tiles.
                float x = left_side + (num_squares_per_tile_x - (1.0f + num_squares_per_tile_x) / 2) * image_square_size + i * num_squares_per_tile_x * image_square_size;
                float y = top - (num_squares_per_tile_y - (1.0f + num_squares_per_tile_y) / 2) * image_square_size - j * num_squares_per_tile_y * image_square_size;

                // make a box around the tile, and scale it so that it fits around the edge
                GameObject g = Instantiate(tile_outline, new Vector2(x,y), Quaternion.identity);
                Vector3 scale = g.transform.localScale;
                scale.x = (float)num_squares_per_tile_x / 2;
                scale.y = (float)num_squares_per_tile_y / 2;
                g.transform.localScale = scale;
            }
        }
    }
}
