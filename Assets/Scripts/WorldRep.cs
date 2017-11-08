using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRep : MonoBehaviour {

    //base map info
    //string type;
    int mapHeight, mapWidth;

    //[SerializeField]
    public GameObject genericTile;

    GameObject[,] wholeMap;

    [Header("Initialization Info")]
    public TextAsset input_map;
    //[Header("Sprite info")]
    //[SerializeField]

    //[SerializeField]
    //private GameObject void_square;

    //[SerializeField]
    //private GameObject tree_square;

    //[SerializeField]
    //private GameObject empty_square;

    [Header("Execution Variables")]
    public int tileSize;

    //Compressed map info
    int Height, Width;
    GameObject[,] world;

    // Use this for initialization
    void Start () {
        GetMap();
        CreateWorldRep();
	}

    // create a representation of all map spaces
    private void GetMap()
    {
        // split the input text into lines
        string[] lines = input_map.text.Split('\n');

        // type I think is useless?
        //type = lines[0].Split(' ')[1];

        // get the height and width of the map
        mapHeight = int.Parse(lines[1].Split(' ')[1]);
        mapWidth = int.Parse(lines[2].Split(' ')[1]);

        // make a float 2d array to store the % of each tile that is covered
        wholeMap = new GameObject[mapWidth, mapHeight];

        // go through all the stuff in the input file
        for (int j = 4; j < lines.Length; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                // determine which of the tiles we are making

                wholeMap[i, j-4] = Instantiate(genericTile, new Vector3(i, j-4, 0), Quaternion.identity);
                switch (lines[j][i])
                {
                    case '@':
                        wholeMap[i, j - 4].GetComponent<TileInfo>().CreateBlocked(i, j-4);
                        break;
                    case 'T':
                        wholeMap[i, j - 4].GetComponent<TileInfo>().CreateBlocked(i, j - 4);
                        break;
                    default:
                        wholeMap[i, j - 4].GetComponent<TileInfo>().CreateEmpty(i, j - 4);
                        break;
                }
                
            }
        }
    }

    void CreateWorldRep()
    {
        Width = mapWidth % tileSize == 0 ? mapWidth / tileSize: mapWidth / tileSize + 1;
        Height = mapHeight % tileSize == 0 ? mapHeight / tileSize : mapHeight / tileSize + 1;

        world = new GameObject[Width, Height];
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                world[i, j].GetComponent<TileInfo>().CreateFromChildren(i, j, GetChildren(tileSize, i, j));
            }
        }
    }

    TileInfo[] GetChildren(int size, int x, int y)
    {
        TileInfo[] children = new TileInfo[size*size];
        int xStart = size * x;
        int yStart = size * y;

        for (int k = 0; k < tileSize; k++)
        {
            int thisx = xStart + k;
            for (int l = 0; l < tileSize; l++)
            {
                int thisy = yStart + l;
                int childnum = k * size + l;

                // if the child is in wholeMap, it is added
                if (thisx < mapWidth && thisy < mapHeight){
                    children[childnum] = wholeMap[thisx, thisy].GetComponent<TileInfo>();
                }
                else
                {
                    //if no in wholeMap, assume child is impassable
                    TileInfo blockedTile = new TileInfo();
                    blockedTile.CreateBlocked(thisx, thisy);
                    children[childnum] = blockedTile;
                }
            }
        }
        return children;
    }
}
