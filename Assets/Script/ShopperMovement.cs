using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopperMovement : MonoBehaviour {
    public Vector3[] path = null;
    public float speed = 1f;
    public int cur = 0;
    public bool destinationReached;
    //private Cell[,] map;

    // Use this for initialization
    void Start () {
        speed = 1f;
        destinationReached = false;
        //map = GameObject.Find("Mall").GetComponent<MallManager>().map;
    }
	
	// Update is called once per frame
	void Update () {
        
        if (path == null || path.Length <= cur) return;
        if (Mathf.Abs(transform.position.z - path[cur].z) >= 0.05 || Mathf.Abs(transform.position.x - path[cur].x) >= 0.05)
        {
            Vector3 pos = Vector3.MoveTowards(transform.position, path[cur], speed * Time.deltaTime);
            gameObject.transform.position = pos;
        }
        else if (cur < path.Length - 1)
        {
            cur++;

        }
        else
        {
            destinationReached = true;

        }
    }
}
