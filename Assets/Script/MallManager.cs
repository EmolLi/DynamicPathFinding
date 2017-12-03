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
}

[System.Serializable]
public class Shopper
{
    public int id;
    public Vector2 cellPos;
    public GameObject shopper;
    public ShopperMovement sm;
    public int goal = 2;
    public Vector2 goalDestPos;
    public bool goalReachedInThisPlanningWin;
    public Vector3[] tempPath;
    public bool tempGoalReached;
    public int failedCnt;


    // test
    public List<int> timeSteps = new List<int>();
    public List<bool> findDest = new List<bool>();
    public int failtToFindDestCnt;

    public void setPath()
    {
        sm.path = this.tempPath;
        sm.next = 0;
        sm.destinationReached = false;
        this.goalReachedInThisPlanningWin = this.tempGoalReached;
    }
}

[System.Serializable]
public class Shop
{
    public Vector2[] cellPos;
    public Cell[] cells;
}

public class MallManager : MonoBehaviour
{
    //-------------- static value-----------------
    // tile occupiant
    public const int UNOCCUPIED = 0;
    ////public const int SHOPPER = 1;
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



    // test
    public int testCnt = 50;    // 10 goals
    public bool testMode = false;

    // Use this for initialization
    void Start()
    {

        initMap();
        buildMap();
        initShoppers();
        selectShopperGoals();
        pathSearch();

    }
    /**
    void test()
    {
        
    }**/



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
        for (int i = 0; i < shopCntPerFloor * 2; i++)
        {
            shops[i] = new Shop();
            shops[i].cellPos = new Vector2[shopInnerLen * shopInnerWidth];
            shops[i].cells = new Cell[shopInnerLen * shopInnerWidth];
        }

        // floor 1
        for (int i = 0; i < floorWidth; i++)
        {
            for (int j = 0; j < floorLen; j++)
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
                    shops[i].cellPos[j * shopInnerLen + k - 1] = new Vector2(k, j + i * (shopInterval + 2 + shopInnerWidth) + shopInterval / 2 + 1);
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
            for (int j = 0; j < stairLen; j++)
            {
                Cell c = new Cell();
                c.occupiedBy = UNOCCUPIED;
                c.pos = new Vector3((i + 1) * spaceBetweenStairs, heightDiffBetweenStairs * j, j + floorLen);
                map[j + floorLen, (i + 1) * spaceBetweenStairs] = c;
            }
        }

        // plants
        for (int i = 0; i < plantCnt; i++)
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
            for (int j = 0; j < mapWidth; j++)
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


        HashSet<Vector2> dup = new HashSet<Vector2>();
        for (int i = 0; i < shopperCnt; i++)
        {
            Vector2 shopperPos = getRandomOccupantPos();
            while (dup.Contains(shopperPos))
            {
                shopperPos = getRandomOccupantPos();
            }
            dup.Add(shopperPos);


            Cell c = map[(int)shopperPos.x, (int)shopperPos.y];
            //c.occupiedBy = SHOPPER;

            Vector3 occupantPos = c.pos;
            occupantPos.y += 0.5f;  // floor thick 
            GameObject s = Instantiate(shopper, occupantPos, Quaternion.identity) as GameObject;
            s.transform.parent = shopperHolder.transform;

            Shopper sObj = new Shopper();
            sObj.id = i;
            sObj.shopper = s;
            sObj.cellPos = shopperPos;
            sObj.sm = new ShopperMovement();
            sObj.goalDestPos = shopperPos;  // current pos

            shoppers[i] = sObj;

            //Debug.Log("s" + sObj.id + " " + getKey(sObj.cellPos));
        }



    }
    // Update is called once per frame
    //void Update()
    //{
    //}






    //-------------------shopper motion & motion planning----------------------------------
    public float playerMovementUpdateFrequency = 1f;
    public float startingInSec = 1f;
    public int planningWinSize = 20; // steptime
    public int firstShopper;    // start with this shopper to do the planning
    public int planningWinTimeCounter = 0;

    void selectShopperGoals()
    {
        foreach (Shopper s in shoppers)
        {
            selectSingleShopperGoal(s);
        }
    }

    void selectSingleShopperGoal(Shopper s)
    {
        //if (s.goal == IDLE || (s.sm.destinationReached && s.goalReachedInThisPlanningWin))
        //{
        // able to set a new goal
        int random = Random.Range(0, 2);
        if (random == 0)
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
                for (int i = 0; i < shop.cells.Length; i++)
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

            // test
            if (testMode)
            {
                if (s.failedCnt != -1)
                s.timeSteps.Add(s.failedCnt);
            }
            s.failedCnt = 0;
        }
        //}

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

        public string key()
        {
            return MallManager.getKey(this.pos);
        }

    }

    public Dictionary<string, AStarCell> reservationTable;    // key: (x, y, t)
    public Dictionary<string, AStarCell> closeSet;
    Dictionary<string, AStarCell> openSet;

    private int manhattanDist(Vector2 p1, Vector2 p2)
    {
        return (int)(Mathf.Abs(p2.x - p1.x) + Mathf.Abs(p2.y - p1.y));
    }

    private void pathSearch()
    {
        // init
        reservationTable = new Dictionary<string, AStarCell>();

        for (int i = 0; i < shopperCnt; i++)
        {
            findPathForAShopper(shoppers[(i + firstShopper) % shopperCnt]);
        }

        foreach (Shopper sh in shoppers)
        {
            sh.setPath();
        }

        planningWinTimeCounter = 0;
        firstShopper = (firstShopper + 1) % shopperCnt;
        InvokeRepeating("updatePlayerMovement", 0f, playerMovementUpdateFrequency);

    }


    private void findPathForAShopper(Shopper s)
    {
        openSet = new Dictionary<string, AStarCell>();
        closeSet = new Dictionary<string, AStarCell>();

        // starting node
        AStarCell cur = new AStarCell(map[(int)s.cellPos.x, (int)s.cellPos.y], new Vector3(s.cellPos.x, s.cellPos.y, 0), 0, manhattanDist(s.cellPos, s.goalDestPos), firstShopper, null);
        openSet.Add(cur.key(), cur);
        while (openSet.Count > 0)
        {
            openSet.Remove(cur.key());
            closeSet.Add(cur.key(), cur);


            // =======stop search========
            // 1. dest found
            // 2. time is up
            //if (cur.pos.x == s.goalDestPos.x && cur.pos.y == s.goalDestPos.y)
            //{
            // construct path
            //s.tempGoalReached = true;
            //s.tempPath = constructPath(cur, s.id);
            //return;
            //}

            if (cur.pos.z == planningWinSize - 1)
            {
                if (cur.pos.x == s.goalDestPos.x && cur.pos.y == s.goalDestPos.y)
                {
                    s.tempGoalReached = true;
                    
                }
                else
                {
                    s.tempGoalReached = false;
                    s.failedCnt++;
                    if (s.failedCnt == 8)
                    {
                        s.failedCnt = -1;
                        // fail
                        s.failtToFindDestCnt++;
                        selectSingleShopperGoal(s);
                    }
                }
                s.tempPath = constructPath(cur, s.id);


                // test
                if (testMode)
                {
                    s.findDest.Add(s.tempGoalReached);
                }
                return;
            }



            // ==========continue search========
            int x;
            int y;
            int t = (int)cur.pos.z + 1;
            int c = (int)cur.c + 1;


            // get traversable successors
            // a valid next move should be 1. traversable, 2. not in closedSet
            // we check if the successor is traversable before we put it in the successors array
            // then check if it's in closedSet when we are searching successors to choose next step
            List<AStarCell> successors = new List<AStarCell>();

            // up
            x = (int)cur.pos.x - 1;
            y = (int)cur.pos.y;
            Vector3 up = new Vector3(x, y, t);

            if (x >= 0 && map[x, y] != null && map[x, y].occupiedBy == UNOCCUPIED)
            {

                successors.Add(new AStarCell(map[x, y], up, c, manhattanDist(new Vector2(x, y), s.goalDestPos), -1, cur));
            }

            // down
            x = (int)cur.pos.x + 1;
            y = (int)cur.pos.y;
            Vector3 down = new Vector3(x, y, t);

            if (x < mapLen && map[x, y] != null && map[x, y].occupiedBy == UNOCCUPIED)
            {
                successors.Add(new AStarCell(map[x, y], down, c, manhattanDist(new Vector2(x, y), s.goalDestPos), -1, cur));
            }

            // left
            x = (int)cur.pos.x;
            y = (int)cur.pos.y - 1;
            Vector3 left = new Vector3(x, y, t);

            if (y >= 0 && map[x, y] != null && map[x, y].occupiedBy == UNOCCUPIED)
            {
                successors.Add(new AStarCell(map[x, y], left, c, manhattanDist(new Vector2(x, y), s.goalDestPos), -1, cur));
            }


            // right
            x = (int)cur.pos.x;
            y = (int)cur.pos.y + 1;
            Vector3 right = new Vector3(x, y, t);
            if (y < mapWidth && map[x, y] != null && map[x, y].occupiedBy == UNOCCUPIED)
            {
                successors.Add(new AStarCell(map[x, y], right, c, manhattanDist(new Vector2(x, y), s.goalDestPos), -1, cur));
            }

            // idle
            x = (int)cur.pos.x;
            y = (int)cur.pos.y;
            Vector3 idle = new Vector3(x, y, t);
            successors.Add(new AStarCell(map[x, y], idle, c, manhattanDist(new Vector2(x, y), s.goalDestPos), -1, cur));



            // search successor
            foreach (AStarCell successor in successors)
            {
                if (closeSet.ContainsKey(successor.key())) continue;
                if (reservationTable.ContainsKey(successor.key())) continue;


                if (s.id > 0)
                {
                    if (reservationTable.ContainsKey(successor.key()))
                    {
                        Debug.Log("asc");
                    }
                }

                // no need to explore this successor if it's already in the closedSet

                // add successor to openSearch to explore later if
                // 1. new path to neighbor is shorter (smaller f)
                // 2. neighbor is not in openSet
                if (!openSet.ContainsKey(successor.key())) openSet.Add(successor.key(), successor);

                else if (successor.f < openSet[successor.key()].f)
                {
                    openSet[successor.key()] = successor;
                }
            }

            AStarCell tempCur = cur;
            cur = null;
            // find node with least f
            foreach (AStarCell ac in openSet.Values)
            {
                if (cur == null || ac.f < cur.f)
                {
                    cur = ac;
                }
            }

        }
    }

    private Vector3[] constructPath(AStarCell dest, int shopperIndex)
    {
        Vector3[] path = new Vector3[planningWinSize];
        string[] tPath = new string[(int)dest.pos.z + 1];
        AStarCell cur = dest;
        for (int i = 0; i <= dest.pos.z; i++)
        {
            cur.reservedBy = shopperIndex;
            if (reservationTable.ContainsKey(cur.key()))
            {
                Debug.Log("sdf");
            }

            reservationTable.Add(cur.key(), cur);

            path[(int)cur.pos.z] = cur.cell.pos;
            tPath[(int)cur.pos.z] = getKey(cur.pos);
            cur = cur.prev;
        }

        cur = dest;
        for (int i = 0; i <= dest.pos.z; i++)
        {
            if (cur.pos.z <= 1) continue;
            Vector3 h2h = new Vector3(cur.pos.x, cur.pos.y, cur.pos.z - 1);
            if (!reservationTable.ContainsKey(getKey(h2h)))
            {
                reservationTable.Add(getKey(h2h), cur);
            }
            cur = cur.prev;
        }

        return path;
    }




    //===============player movement=============
    private void updatePlayerMovement()
    {
        foreach (Shopper s in shoppers)
        {
            if (s.sm.path != null && s.sm.path.Length != 0 && !s.sm.destinationReached)
            {
                if (s.sm.next < s.sm.path.Length)
                {
                    s.shopper.transform.position = s.sm.path[s.sm.next];
                    s.cellPos = new Vector2(s.sm.path[s.sm.next].z, s.sm.path[s.sm.next].x);
                    if (s.cellPos == new Vector2(0, 0))
                    {
                        Debug.Log("f");
                    }
                    s.sm.next++;
                }
                if (s.sm.next == s.sm.path.Length)
                {
                    s.sm.destinationReached = true;
                    if (s.goalReachedInThisPlanningWin)
                    {
                        // set next goal if goal reached
                        selectSingleShopperGoal(s);
                    }
                }
            }
        }

        planningWinTimeCounter++;
        if (planningWinTimeCounter == planningWinSize)
        {
            CancelInvoke("updatePlayerMovement");
            if (testMode)
            {
                testCnt--;
                if (testCnt > 0)
                {
                    pathSearch();
                }

                else
                {

                    Debug.Log("end!   ");
                    int failureCnt = 0;
                    int successTimeStepCnt = 0;
                    int successCnt = 0;
                    foreach (Shopper s in shoppers)
                    {
                        failureCnt += s.failtToFindDestCnt;

                        foreach (int k in s.timeSteps)
                        {
                            successTimeStepCnt += k;
                            successCnt++;
                        }
                    }
                    Debug.Log((successCnt + failureCnt) + " " + failureCnt + "  " + failureCnt / (float)(successCnt + failureCnt) + "  " + successTimeStepCnt / (float)successCnt);
                }
            }
            else pathSearch();
            return;
        }
    }


    public static string getKey(Vector3 v)
    {
        return (int)v.x + "-" + (int)v.y + "-" + (int)v.z;
    }
}

[System.Serializable]
public class ShopperMovement
{
    public Vector3[] path = null;
    public float speed = 1f;
    public int next = 0;
    public bool destinationReached;

}