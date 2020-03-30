using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class addStreetObjects : MonoBehaviour
{
    /*
     *  INSTRUCTIONS:
     *  1. Create an empty game-object in the scene.
     *  2. Assign this script too it.
     *  3. Follow the parameter guideline below.
     *      3.1. If you want a custom object place it in the resources folder to make it visible.
     *  4. The objects are rendered after you enter scene mode.
     *  
     *  
     * - pointA: Start point of object placement.
     * - pointB: End point of object placement.
     * - bush: true if you want to bushes
     * - tree: true if you want trees.
     * - bench: true if you want benches.
     * - NumberOf: Amount of objects (custom or benches, bushes is 4 times the amount).
     * - benchSideRight: check true if you want left rotation
     * - in street direction otherwise right
     * 
     * Custom Objects can be added if you place them inside of the
     * resources-folder.
     * 
     * - customObject: Check True ONLY if you provide a custom object name.
     * - customObjectName: Give the name of the object inside of the resources folder.
     * - scaleCustomObject: If you need to change the scale of the object, provide the scale factor.
     *                      Else if you dont need to scale, leave as 0.
     * 
     * 
     * OBS! Can not provide both benches and custom-objects. The Custom objects replace benches.
     * 
     */

    public Vector3 PointA; 
    public Vector3 PointB; 
    public int NumberOf;
    public bool tree;
    public bool bush;
    public bool bench;
    public bool benchSideRight;
    public bool customObject;
    public string customObjectName;
    public float scaleCustomObject;


    // Start is called before the first frame update
    void Start()
    {

        for (int i = 0; i < NumberOf; i++)
        {
            Vector3 pos = PointA + i*((PointB - PointA)/NumberOf);

            if (customObject)
            {
                GameObject streetObject = (GameObject)Instantiate(Resources.Load(customObjectName), pos, Quaternion.identity);
                if (scaleCustomObject != 0)
                {
                    Vector3 scaleChangeCustomObject = new Vector3(scaleCustomObject, scaleCustomObject, scaleCustomObject);
                    streetObject.transform.localScale = scaleChangeCustomObject;
                }
            }

            if (bench && !customObject)
            {
                Vector3 scaleChange = new Vector3(0.6f, 0.6f, 0.6f);
                GameObject bench = (GameObject)Instantiate(Resources.Load("bench"), pos, Quaternion.identity);
                bench.transform.localScale = scaleChange;
                if (benchSideRight && bench)
                {
                    Vector3 v = PointB - PointA;
                    bench.transform.rotation = Quaternion.FromToRotation(Vector3.left, v);
                }
                else if(bench)
                {
                    Vector3 v = PointB - PointA;
                    bench.transform.rotation = Quaternion.FromToRotation(Vector3.right, v);
                }
            }
          
            if (tree)
            {
                pos += ((PointB - PointA) / NumberOf) / 2;
                if (i % 2 == 0)
                {
                    GameObject tree = (GameObject)Instantiate(Resources.Load("Birch_9"), pos, Quaternion.identity);
                }
                else
                {
                    GameObject tree = (GameObject)Instantiate(Resources.Load("Birch_3"), pos, Quaternion.identity);
                }
            }

            if (bush)
            {
                if (tree)
                {
                    pos -= ((PointB - PointA) / NumberOf) / 2;
                }

                for (int m = 1; m < 5; m++)
                {
                    Vector3 scaleChangeBush = new Vector3(0.1f, 0.1f, 0.1f);
                    pos += ((PointB - PointA) / NumberOf)/5;
                    if (m % 2 == 0)
                    {
                        GameObject bush = (GameObject)Instantiate(Resources.Load("bush1"), pos, Quaternion.identity);
                        bush.transform.localScale = scaleChangeBush;
                    }
                    else
                    {
                        GameObject bush = (GameObject)Instantiate(Resources.Load("bush2"), pos, Quaternion.identity);
                        bush.transform.localScale = scaleChangeBush;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
