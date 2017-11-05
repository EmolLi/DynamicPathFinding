using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MallManager : MonoBehaviour {
    public const int UNOCCUPIED = 0;
    public const int PEOPLE = 1;
    public const int PLANT = 2;
    public const int WALL = 3;


    [System.Serializable]
    public class Cell
    {
        public int occupiedBy;  // 0: unoccupied, 1: people, 2: plant, 3: wall
        public Vector3 pos;
        /**
        public bool visited;
        public Vector3 pos;
        public GameObject north;    // 1
        public GameObject east;     // 2    
        public GameObject west;     // 3
        public GameObject south;    // 4
        public List<int> neighbors;**/
    }



    public GameObject plant;
    public GameObject floor;
    public GameObject wall; // cube

    int mapWidth;
    int mapLen;

    public int stairLen;
    int stairWidth = 1;
    int stairCnt = 4;
    int floorLen;   // x
    int floorWidth; // y
    int floorOneHeight = 0;
    int floorTwoHeight = 4;

    int shopCntPerFloor = 6;
    int shopInnerWidth = 3;
    int shopInnerLen = 3;
    int shopInterval = 2;
    int shopFrontSpaceLen = 4;


    public Vector3[] plantsPos;
    public Cell[,] map; // conceptual map, bool value means that if this  



    private GameObject floorHolder;


    // Use this for initialization
    void Start () {
        initMap();
        buildMap();

    }


    void initMap()
    {
        // compute floor parameters
        mapWidth = (1 + shopCntPerFloor) * shopInterval + (2 + shopInnerWidth) * shopCntPerFloor - 2;   // wall width is 1, 2 wall: 1*2 = 2
        floorLen = (2 + shopInnerLen) + shopFrontSpaceLen;
        floorWidth = mapWidth;
        mapLen = floorLen * 2 + stairLen;


        // init map
        map = new Cell[mapLen, mapWidth];
        // floor 1
        for (int i = 0; i < floorWidth; i++)
        {
            for (int j = 0; j< floorLen; j++)
            {
                Cell c = new Cell();
                c.occupiedBy = UNOCCUPIED;
                c.pos = new Vector3(i, floorOneHeight, j);
                map[j, i] = c;
            }
        }
        // floor1 shops
        for (int i = 0; i < shopCntPerFloor; i++)
        {
            // vertical walls
            for (int j = 0; j < shopInnerLen + 2; j++)
            {
                for (int k = 0; k < shopInterval * 0.5 + 1; k++)
                {
                    // left
                    map[j, k + i * (shopInterval + 2 + shopInnerWidth)].occupiedBy = WALL;
                    // right
                    map[j, k + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + shopInnerWidth + 1].occupiedBy = WALL;
                }
            }
            // horizontal walls
            for (int j = 0; j < shopInnerWidth; j++)
            {
                // bottom
                map[0, j + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 1].occupiedBy = WALL;
                // top
                map[shopInnerLen + 1, j + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 1].occupiedBy = WALL;
            }
            // entrance
            map[shopInnerLen + 1, i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 2].occupiedBy = UNOCCUPIED;
        }



        // floor 2
        for (int i = 0; i < floorWidth; i++)
        {
            for (int j = 0; j < floorLen; j++)
            {
                Cell c = new Cell();
                c.occupiedBy = UNOCCUPIED;
                c.pos = new Vector3(i, floorTwoHeight, j + floorLen + stairLen);
                //Debug.Log(c.pos.y);

                map[j + floorLen + stairLen, i] = c;
            }
        }

        // floor2 shops
        int floor2ShopLenBase = floorLen + stairLen + shopFrontSpaceLen;
        for (int i = 0; i < shopCntPerFloor; i++)
        {
            // vertical walls
            for (int j = 0; j < shopInnerLen + 2; j++)
            {
                for (int k = 0; k < shopInterval * 0.5 + 1; k++)
                {
                    // left
                    map[j + floor2ShopLenBase, k + i * (shopInterval + 2 + shopInnerWidth)].occupiedBy = WALL;
                    // right
                    map[j + floor2ShopLenBase, k + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + shopInnerWidth + 1].occupiedBy = WALL;
                }
            }
            // horizontal walls
            for (int j = 0; j < shopInnerWidth; j++)
            {
                // bottom
                map[floor2ShopLenBase, j + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 1].occupiedBy = WALL;
                // top
                map[shopInnerLen + 1 + floor2ShopLenBase, j + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 1].occupiedBy = WALL;
            }
            // entrance
            map[floor2ShopLenBase, i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 2].occupiedBy = UNOCCUPIED;
        }


        // stairs
        int spaceBetweenStairs = (floorWidth - stairCnt) / (stairCnt + 1);
        float heightDiffBetweenStairs = (floorTwoHeight - floorOneHeight) / (float)(stairLen);
        Debug.Log(spaceBetweenStairs);
        for (int i = 0; i < stairCnt; i++)
        {
            for (int j = 0; j< stairLen; j++)
            {
                Cell c = new Cell();
                c.occupiedBy = UNOCCUPIED;
                c.pos = new Vector3((i + 1) * spaceBetweenStairs, heightDiffBetweenStairs * j , j + floorLen);
                map[j + floorLen, (i + 1) * spaceBetweenStairs] = c;
            }
        }


    }

    void buildMap()
    {
        floorHolder = new GameObject();
        floorHolder.name = "floors";

        GameObject tempFloor;
        GameObject occupant;
        for (int i = 0; i < mapLen; i++)
        {
            for (int j = 0; j< mapWidth; j++)
            {
                Cell c = map[i, j];
                if (c != null)
                {
                    //Debug.Log(c.pos.y);
                    tempFloor = Instantiate(floor, c.pos, Quaternion.identity);
                    tempFloor.tag = "floor";
                    tempFloor.transform.parent = floorHolder.transform;

                    // build wall
                    if (c.occupiedBy == WALL)
                    {
                        Vector3 occupantPos = c.pos;
                        occupantPos.y += 0.25f;  // floor thick 
                        occupant = Instantiate(wall, occupantPos, Quaternion.identity);
                    }
                }

            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
