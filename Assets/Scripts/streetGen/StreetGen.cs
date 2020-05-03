using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using GSD.Roads;
// Proper automation flow:
    // 1. Make sure opt_bAllowRoadUpdates in the scene's GSDRoadSystem is set to FALSE.
    // 2. Create your roads programmatically via CreateRoad_Programmatically (pass it the road, and then the points in a list)
    //      a. Optionally you can do it via CreateNode_Programmatically and InsertNode_Programmatically
    // 3. Call CreateIntersections_ProgrammaticallyForRoad for each road to create intersections automatically at intersection points.
    // 4. Set opt_bAllowRoadUpdates in the scene's GSDRoadSystem is set to TRUE.
    // 5. Call GSDRoadSystem.UpdateAllRoads();
    // 6. Call GSDRoadSystem.UpdateAllRoads(); after step #5 completes.
    //
    // See "GSDUnitTests.cs" for an example on automation (ignore unit test #3).
public class StreetGen : MonoBehaviour {
    [ExecuteInEditMode]
    // Get building data from GEOjson file
    public TextAsset geojsonData;
    public float laneWidthPercent;
    public float shoulderWidthPercent;
    void Start(){
    }

    void Update()
    {
        
    }
    public void makeRoad() {
        GeoJsonParser p = new GeoJsonParser(geojsonData);
        List<Road> roads = p.GetRoads();
        GSDRoadSystem RoadSystem;
        GameObject rc;
        if(!GameObject.Find("RoadContainer")) {
            rc = new GameObject("RoadContainer");
        }

        // do road stuff
        foreach(Road road in roads) {
            if(GameObject.Find(road.id)) {
                // it already exists
            } else {
                GameObject tRoadSystemObj = new GameObject(road.id);
                tRoadSystemObj.transform.parent = GameObject.Find("RoadContainer").transform;
                RoadSystem = tRoadSystemObj.AddComponent<GSDRoadSystem>(); 	//Add road system component.
                RoadSystem.opt_bAllowRoadUpdates = false;
                GSDRoad firstroad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref road.nodes);
                // GSDRoadAutomation.CreateIntersections_ProgrammaticallyForRoad(firstroad, GSDRoadIntersection.iIntersectionTypeEnum.None, GSDRoadIntersection.RoadTypeEnum.NoTurnLane);
                firstroad.opt_LaneWidth = laneWidthPercent / 100f * 5f;
                firstroad.opt_ShoulderWidth = shoulderWidthPercent / 100f * 3f;
                // firstroad
                RoadSystem.opt_bAllowRoadUpdates = true;
                RoadSystem.UpdateAllRoads();
            }
        }

    }
    public void removeRoads() {
        GameObject roadContainer = GameObject.Find("RoadContainer");
        if(roadContainer) {
            Transform rc = roadContainer.transform;
            int children = rc.childCount;
            for (int i = 0; i < children; i++)
            {
                DestroyImmediate(rc.GetChild(0).gameObject);
            }
        }
    }
}