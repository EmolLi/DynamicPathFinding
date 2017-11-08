using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class Cell
{
    public int occupiedBy;  // 0: unoccupied, 1: people, 2: plant, 3: wall
    public Vector3 pos;
    public GameObject tile;
    public bool bookedAsGoal;
    /**
    public bool visited;
    public Vector3 pos;
    public GameObject north;    // 1
    public GameObject east;     // 2    
    public GameObject west;     // 3
    public GameObject south;    // 4
    public List<int> neighbors;**/
}

[System.Serializable]
public class Shopper
{
    public Vector2 cellPos;
    public GameObject shopper;
    public ShopperMovement sm;
    public int goal = 2;
    public Vector2 goalDestPos;
    public bool goalReachedInThisPlanningWin;

    public void setPath(Vector3[] path, bool goalReachedInThisPlanningWin)
    {
        sm.path = path;
        sm.cur = 0;
        sm.destinationReached = false;
        this.goalReachedInThisPlanningWin = goalReachedInThisPlanningWin;
    }
}

[System.Serializable]
public class Shop
{
    public Vector2[] cellPos;
    public Cell[] cells;
}

public class MallManager : MonoBehaviour {
    //-------------- static value-----------------
    // tile occupiant
    public const int UNOCCUPIED = 0;
    public const int SHOPPER = 1;
    public const int PLANT = 2;
    public const int WALL = 3;

    // shopper goal
    public const int MOVE = 0;
    public const int SHOP = 1;
    public const int IDLE = 2;




    public GameObject plant;
    public GameObject floor;
    public GameObject wall;
    public GameObject shopper;

    int mapWidth;
    int mapLen;
    int validPlantRowInAFloor;

    public int shopperCnt = 5;
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
    public Shop[] shops;

    public int plantCnt = 4;



    public Cell[,] map;

    public Shopper[] shoppers;

    private GameObject floorHolder;
    private GameObject shopHolder;
    private GameObject plantHolder;
    private GameObject shopperHolder;


    // Use this for initialization
    void Start () {

        Dictionary<Vector3, int> a = new Dictionary<Vector3, int>();
        a.Add(new Vector3(1, 1, 1), 1);
        Debug.Log(a.ContainsKey(new Vector3(1, 1, 1)));

        initMap();
        buildMap();
        initShoppers();
        InvokeRepeating("selectShopperGoals", startingInSec, selectGoalIntervalTime);
        //shoppers[0].sm.path = new Vector3[1];
        //shoppers[0].sm.path[0] = map[6, 7].pos;
        //Debug.Log(shoppers[0].sm.path);
    }


    void initMap()
    {
        // compute floor parameters
        mapWidth = (1 + shopCntPerFloor) * shopInterval + (2 + shopInnerWidth) * shopCntPerFloor - 2;   // wall width is 1, 2 wall: 1*2 = 2
        floorLen = (2 + shopInnerLen) + shopFrontSpaceLen;
        floorWidth = mapWidth;
        mapLen = floorLen * 2 + stairLen;
        validPlantRowInAFloor = shopFrontSpaceLen - 2;


        // init map
        map = new Cell[mapLen, mapWidth];
        shops = new Shop[shopCntPerFloor * 2];
        for (int i = 0; i< shopCntPerFloor * 2; i++)
        {
            shops[i] = new Shop();
            shops[i].cellPos = new Vector2[shopInnerLen * shopInnerWidth];
            shops[i].cells = new Cell[shopInnerLen * shopInnerWidth];
        }

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

                // shop inner space
                for (int k = 1; k <= shopInnerLen; k++)
                {
                    shops[i].cellPos[j*shopInnerLen + k - 1] = new Vector2(k, j + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 1);
                    shops[i].cells[j * shopInnerLen + k - 1] = map[k, j + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 1];
                }

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

                // shop inner space
                for (int k = 1; k <= shopInnerLen; k++)
                {
                    shops[i + shopCntPerFloor].cellPos[j * shopInnerLen + k - 1] = new Vector2(floor2ShopLenBase + k, j + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 1);
                    shops[i + shopCntPerFloor].cells[j * shopInnerLen + k - 1] = map[floor2ShopLenBase + k, j + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 1];
                }

                // top
                map[shopInnerLen + 1 + floor2ShopLenBase, j + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 1].occupiedBy = WALL;
            }
            // entrance
            map[floor2ShopLenBase, i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 2].occupiedBy = UNOCCUPIED;
        }


        // stairs
        int spaceBetweenStairs = (floorWidth - stairCnt) / (stairCnt + 1);
        float heightDiffBetweenStairs = (floorTwoHeight - floorOneHeight) / (float)(stairLen);
        //Debug.Log(spaceBetweenStairs);
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

        // plants
        for (int i = 0; i< plantCnt; i++)
        {
            //Cell c = null;
            //while (c == null || c.occupiedBy != UNOCCUPIED)
            //{
                Vector2 plantPos = getRandomOccupantPos();
               Cell c = map[(int)plantPos.x, (int)plantPos.y];
            //}

            c.occupiedBy = PLANT;
        }


    }

    Vector2 getRandomOccupantPos()
    {


        Cell c = null;
        Vector2 pos = new Vector2();
        while (c == null || c.occupiedBy != UNOCCUPIED || c.bookedAsGoal)
        {
            int col = Random.Range(0, floorWidth);
            int rowInFloor = Random.Range(0, validPlantRowInAFloor);
            int floorNumber = Random.Range(0, 2);


            if (floorNumber == 0)
            {
                // floor 1
                pos = new Vector2(rowInFloor + 2 + shopInnerLen + 1, col);
               
            }
            // floor 2
           else pos = new Vector2(floorLen + stairLen + rowInFloor + 1, col);

            c = map[(int)pos.x, (int)pos.y];
        }
        return pos;
    }

    void buildMap()
    {
        
        floorHolder = new GameObject();
        floorHolder.name = "floors";
        floorHolder.transform.parent = gameObject.transform;

        shopHolder = new GameObject();
        shopHolder.name = "shops";
        shopHolder.transform.parent = gameObject.transform;

        plantHolder = new GameObject();
        plantHolder.name = "plants";
        plantHolder.transform.parent = gameObject.transform;

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
                    c.tile = tempFloor;

                    // build occupant
                    if (c.occupiedBy == WALL)
                    {
                        Vector3 occupantPos = c.pos;
                        //occupantPos.y += 0.5f;  // floor thick 
                        occupant = Instantiate(wall, occupantPos, Quaternion.identity);
                        occupant.transform.parent = shopHolder.transform;
                    }
                    if (c.occupiedBy == PLANT)
                    {
                        Vector3 occupantPos = c.pos;
                        //occupantPos.y += 0.5f;  // floor thick 
                        occupant = Instantiate(plant, occupantPos, Quaternion.identity);
                        occupant.transform.parent = plantHolder.transform;
                    }

                }

            }
        }
    }
	


    void initShoppers()
    {
        shopperHolder = new GameObject();
        shopperHolder.name = "shoppers";
        shoppers = new Shopper[shopperCnt];

        for (int i = 0; i < shopperCnt; i++)
        {
            Vector2 shopperPos = getRandomOccupantPos();
            Cell c = map[(int)shopperPos.x, (int)shopperPos.y];
            c.occupiedBy = SHOPPER;

            Vector3 occupantPos = c.pos;
            occupantPos.y += 0.5f;  // floor thick 
            GameObject s = Instantiate(shopper, occupantPos, Quaternion.identity) as GameObject;
            s.transform.parent = shopperHolder.transform;

            Shopper sObj = new Shopper();
            sObj.shopper = s;
            sObj.cellPos = shopperPos;
            sObj.sm = s.GetComponent<ShopperMovement>();
            sObj.goalDestPos = shopperPos;  // current pos

            shoppers[i] = sObj;
        }
       
    }
	// Update is called once per frame
	void Update () {
    }






    //-------------------shopper motion & motion planning----------------------------------
    public float selectGoalIntervalTime = 2f;
    public float startingInSec = 1f;
    public int planningWinSize; // steptime
    public int firstShopper;    // start with this shopper to do the planning


    void selectShopperGoals()
    {
        foreach (Shopper s in shoppers)
        {
            selectSingleShopperGoal(s);
        }
    }

    void selectSingleShopperGoal(Shopper s)
    {
        if (s.goal == IDLE || (s.sm.destinationReached && s.goalReachedInThisPlanningWin))
        {
            // able to set a new goal
            float random = Random.Range(0, 5);
            Debug.Log(random);
            if (random == 0)
            {
                // IDLE
                //map[(int)s.goalDestPos.x, (int)s.goalDestPos.y].bookedAsGoal = false;
                s.goal = IDLE;

            }
            else if (random < 3)
            {
                // MOVE
                map[(int)s.goalDestPos.x, (int)s.goalDestPos.y].bookedAsGoal = false;
                s.goal = MOVE;
                Vector2 randomDes = getRandomOccupantPos();
                s.goalDestPos = randomDes;
                map[(int)randomDes.x, (int)randomDes.y].bookedAsGoal = true;
            }
            else
            {
                // SHOP
                s.goal = SHOP;
                map[(int)s.goalDestPos.x, (int)s.goalDestPos.y].bookedAsGoal = false;

                Vector2 des = new Vector2();
                int randomShop = -1;
                bool desFound = false;
                while (!desFound)
                {
                    randomShop = Random.Range(0, shopCntPerFloor * 2);
                    Shop shop = shops[randomShop];
                    // select a spot
                    for (int i = 0; i< shop.cells.Length; i++)
                    {
                        if (shop.cells[i].occupiedBy == UNOCCUPIED && !shop.cells[i].bookedAsGoal)
                        {
                            des = shop.cellPos[i];
                            shop.cells[i].bookedAsGoal = true;
                            desFound = true;
                        }
                    }
                }
                // assume ppl cnt < spots in shops
                s.goalDestPos = des;
                map[(int)s.goalDestPos.x, (int)s.goalDestPos.y].bookedAsGoal = true;
            }
        }

    }



    [System.Serializable]
    public class AStarCell
    {
        public Cell cell;
        public Vector3 pos; //<x, y, t>
        public int c;
        public int h;
        public int f;
        public int reservedBy;  // player index
        public AStarCell prev;

        public AStarCell(Cell cell, Vector3 pos, int c, int h, int reservedBy, AStarCell prev)
        {
            this.cell = cell;
            this.pos = pos;
            this.c = c;
            this.h = h;
            this.f = c + h;
            this.reservedBy = reservedBy;
            this.prev = prev;
        }
    }

    public Dictionary<Vector3, AStarCell> reservationTable;    // key: (x, y, t)

    private int manhattanDist(Vector2 p1, Vector2 p2)
    {
        return (int) (Mathf.Abs(p2.x - p1.x) + Mathf.Abs(p2.y - p1.y));
    }

    private void pathSearch()
    {
        // init

        reservationTable = new Dictionary<Vector3, AStarCell>();
        HashSet<AStarCell> openSet = new HashSet<AStarCell>();
        HashSet<AStarCell> closeSet = new HashSet<AStarCell>();

        Shopper s = shoppers[firstShopper];
        // starting node
        AStarCell cur = new AStarCell(map[(int)s.cellPos.x, (int)s.cellPos.y], new Vector3(s.cellPos.x, s.cellPos.y, 0), 0, manhattanDist(s.cellPos, s.goalDestPos), firstShopper, null);
        openSet.Add(cur);
        while (openSet.Count > 0)
        {
            // find node with least f
            foreach (AStarCell ac in openSet)
            {
                if (ac.f < cur.f)
                {
                    cur = ac;
                }
            }
            openSet.Remove(cur);

            int x;
            int y;
            int t = (int)cur.pos.z + 1;
            int c = (int)cur.c + 1;
            if (t < planningWinSize)
            {
                // get successors
                AStarCell[] successors = new AStarCell[5];
                // up
                Vector3 up = new Vector3(cur.pos.x - 1, cur.pos.y, t);
                if (cur.pos.x > 0 &&
                    map[(int)cur.pos.x - 1, (int)cur.pos.y].occupiedBy == UNOCCUPIED &&
                    !reservationTable.ContainsKey(up))
                {
                    successors[0] = new AStarCell(map[(int)cur.pos.x - 1, (int)cur.pos.y], up, c, manhattanDist(new Vector2(cur.pos.x, cur.pos.y), s.goalDestPos), -1, cur);
                }

                // down
                x = (int)cur.pos.x + 1;
                y = (int)cur.pos.y;
                Vector3 down = new Vector3(x, y, t);
                if (x < mapLen && map[x, y].occupiedBy == UNOCCUPIED && !reservationTable.ContainsKey(down))
                {
                    successors[1] = new AStarCell(map[x, y], down, c, manhattanDist(new Vector2(x, y), s.goalDestPos), -1, cur);
                }

                // left
                x = (int)cur.pos.x;
                y = (int)cur.pos.y - 1;
                Vector3 left = new Vector3(x, y, t);
                if (y >= 0 && map[x, y].occupiedBy == UNOCCUPIED && !reservationTable.ContainsKey(down))
                {
                    successors[2] = new AStarCell(map[x, y], left, c, manhattanDist(new Vector2(x, y), s.goalDestPos), -1, cur);
                }

                // right
                x = (int)cur.pos.x;
                y = (int)cur.pos.y + 1;
                Vector3 right = new Vector3(x, y, t);
                if (y < mapWidth && map[x, y].occupiedBy == UNOCCUPIED && !reservationTable.ContainsKey(right))
                {
                    successors[3] = new AStarCell(map[x, y], right, c, manhattanDist(new Vector2(x, y), s.goalDestPos), -1, cur);
                }

                // idle
                x = (int)cur.pos.x;
                y = (int)cur.pos.y;
                Vector3 idle = new Vector3(x, y, t);
                if (!reservationTable.ContainsKey(idle))
                {
                    successors[4] = new AStarCell(map[x, y], idle, c, manhattanDist(new Vector2(x, y), s.goalDestPos), -1, cur);
                }



                // search successor
                for (int i = 0; i < 5; i++)
                {
                    AStarCell successor = successors[i];
                    if (successor != null)
                    {
                        // stop search if the successor is goal
                        if (successor.pos.x == s.goalDestPos.x && successor.pos.y == s.goalDestPos.y)
                        {
                            // path found

                        }
                    }
                }
            }


        }
    }
}

/**
public class PathFindingManager{
    public int planningWinSize; // steptime
    public int firstShopper;    // start with this shopper to do the planning
    }**/