using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using OsmSharp.Complete;

// Handles the parsing of a GEOjson file into Building objects (defined below)
// To generate GEOjson files go to https://overpass-turbo.eu/
// GEOjson documentation: https://tools.ietf.org/html/rfc7946
public class GeoJsonParser
{
    public TextAsset geojson;
    private FeatureCollection parsedData;


    // Parses GEOjson data into a FeatureCollection structure (defined below)
    public GeoJsonParser(TextAsset geojson)
    {
        var text = geojson.text;
        parsedData = JsonConvert.DeserializeObject<FeatureCollection>(text);

    }


    // Creates a list of Building objects from data in parsedData field
    public List<BuildingData> GetBuildings(){
        List<BuildingData> buildings = new List<BuildingData>();

        foreach (Feature feature in parsedData.features)
        {
            if (feature.properties.building != null && feature.geometry.type == "Polygon") // TODO: Handle when geometry.type == "Multipolygon"
            {
                string id = feature.id;
                int buildingLevels = feature.properties.buildingLevels != null ? (int)feature.properties.buildingLevels : 1;
                int buildingMinLevel = feature.properties.buildingMinLevel != null ? (int)feature.properties.buildingMinLevel : 0;
                
                // Parse height into nullable float
                string heightString = feature.properties.buildingHeight == null ? "" : feature.properties.buildingHeight;
                heightString = Regex.Replace(heightString, "[A-Za-z ]", ""); // Remove unit labels
                float temp;
                float? buildingHeight = float.TryParse(heightString, out temp) ? float.Parse(heightString): (float?)null;

                List<double[]> outerPolygon = feature.geometry.coordinates[0].ToObject<List<double[]>>(); // Only take first polygon, the rest describe the polygon's holes
                
                BuildingData building = new BuildingData(id, outerPolygon, buildingHeight, buildingLevels, buildingMinLevel);
                buildings.Add(building);
            }
        }
        return buildings;
    }

    // TODO get all roads with nodes in common and make intersections based on that
    // TODO filter out irrelevant nodes (highway: elevators) like surface level etc.
    public List<Road> GetRoads(){
        List<Road> roads = new List<Road>();

        foreach (Feature feature in parsedData.features)
        {
            if (feature.geometry.type == "LineString" && feature.properties.surface == "asphalt" && !(feature.properties.layer < 0) && !(feature.properties.layer > 1)) // Check if it's a "way"
            {
                string id = feature.id;
                
                Road road = new Road(id);
                List<double[]> roadNodes = new List<double[]>();
                //road.addNodes(feature.geometry.coordinates.Cast<List<double[]>>().ToList());
                foreach(dynamic coord in feature.geometry.coordinates) {
                    roadNodes.Add(coord.ToObject<double[]>());
                }
                road.addNode(roadNodes);
                roads.Add(road);
            }
        }
        return roads;
    }

// ------------------------------------------------- Defining GEOjson structure -----------------------------------------------------------------------------
// If field isnt defined then JsonConvert.DeserializeObject will ignore it
    

    private class FeatureCollection
    {
        //public string type { get; set; }
        //public string generator { get; set; }
        //public string copyright { get; set; }
        //public string timestamp { get; set; }
        public List<Feature> features { get; set; }
    }
    

    private class Feature
    {
        public string type { get; set; }
        public Property properties { get; set; }
        public Geometry geometry { get; set; }
        public string id { get; set; }
    }

    private class Property
    {
        [JsonProperty("@id")]
        public string id { get; set; }
        public string building { get; set; }
        [JsonProperty("building:levels")]
        public int? buildingLevels { get; set; }
        [JsonProperty("building:min_level")]
        public int? buildingMinLevel { get; set; }
        [JsonProperty("building:height")]
        public string buildingHeight { get; set; }
        public string type { get; set; }
        public string surface { get; set; }
        public int layer { get; set; }
    }

    private class Geometry
    {
        public string type { get; set; }
        public List<dynamic> coordinates { get; set; } // If is a building's coordinates will dynamically match type List<List<List<double>>>
    }
}

public class Road {

    private const double ORIGIN_LONGITUDE = 18.062910d;
    private const double ORIGIN_LATITUDE = 59.332349d;
    public List<Vector3> nodes;
    public string id;

    private Vector3 position;

    public Road(string id) {
        this.nodes = new List<Vector3>();
        this.id = id;
    }

    public void addNode(List<double[]> longLatNodes){
        this.position = GetRoadPosition(longLatNodes[0][0], longLatNodes[0][1]);
        this.nodes = TranslateCoordsToVertices(longLatNodes);
    }

    // Takes a coordinate and returns a vector representing the coordinate's direction in meters from the origin (ORIGIN_LATITUDE, ORIGIN_LONGITUDE) 
    private Vector3 GetRoadPosition(double lon, double lat){
        double changeInLongitude = lon - ORIGIN_LONGITUDE;
        double changeInLatitude = lat - ORIGIN_LATITUDE;
        double avgLat = (lat + ORIGIN_LATITUDE) / 2;

        double distFromOriginX = changeInLongitude * 111316 * System.Math.Cos(avgLat * System.Math.PI / 180d);
        double distFromOriginZ = changeInLatitude * 111111d;
        return new Vector3((float)distFromOriginX, 0, (float)distFromOriginZ);
    }
    private List<Vector3> TranslateCoordsToVertices(List<double[]> latLonCoordList)
    {
        List<Vector3> vertices = new List<Vector3>(); // One less element since last element in GEOjson coordinates is always first coordinate repeated
        double roadLonOrigin = latLonCoordList[0][0];
        double roadLatOrigin = latLonCoordList[0][1];

        for (int i = 0; i < latLonCoordList.Count-1; i++) 
        {
            double changeInLongitude = (latLonCoordList[i][0] - roadLonOrigin);
            double changeInLatitude = (latLonCoordList[i][1] - roadLatOrigin);

            double xRelative = changeInLongitude * 111316 * System.Math.Cos(roadLatOrigin * System.Math.PI / 180d);
            double zRelative = changeInLatitude * 111111d;
            // yval will always be 0 since the world is flat
            vertices.Add(new Vector3((float)xRelative + this.position.x, 0 ,(float)zRelative + this.position.z));
        }
        return vertices;
    }
}
// Structure for holding data of parsed buildings
public class BuildingData
{
    // origin describes some point in the middle of Klarabergsgatan
    private const double ORIGIN_LONGITUDE = 18.062910d;
    private const double ORIGIN_LATITUDE = 59.332349d;
    private const float FLOOR_HEIGHT = 5f;
    public string id { get; set; }
    public int levels { get; set; } // Number of floors
    public int minLevel { get; set; } // Minimum floor level of this building piece
    public float height { get; set; }
    public Vector3 position { get; set; } // position of first point in basePolygon in relation to (ORIGIN_LATITUDE, ORIGIN_LONGITUDE) (in meters)
    public Vector3[] basePolygon { get; set; } // set of vertices describing polygon shape of building's base
    public Vector3[] roofPolygon { get; set; } // set of vertices describing polygon shape of building's roof

    public BuildingData(string buildingID, List<double[]> roofVertexList, float? buildingHeight, int buildingLevels, int buildingMinLevel){
        id = buildingID;
        levels = buildingLevels;
        minLevel = buildingMinLevel;
        height = buildingHeight != null ? (float)buildingHeight : (FLOOR_HEIGHT * levels) + 2; 
        position = GetBuildingPosition(roofVertexList[0][0], roofVertexList[0][1]); // Relative position from origin to first vertex
        basePolygon = TranslateCoordsToBuildingVertices(roofVertexList);

        // Create roof vertices from basePolygon
        roofPolygon = new Vector3[basePolygon.Length];
        for(int i = 0; i<basePolygon.Length; i++)
        {
            roofPolygon[i] = new Vector3(basePolygon[i].x, height, basePolygon[i].z);
        }
    }

    public BuildingData(CompleteWay buildingWay)
    {
        id = buildingWay.Id.ToString();
        levels = buildingWay.Tags.ContainsKey("building:levels") ? int.Parse(buildingWay.Tags.GetValue("building:levels")) : 1;
        minLevel = buildingWay.Tags.ContainsKey("building:min_level") ? int.Parse(buildingWay.Tags.GetValue("building:min_level")) : 0;

        // Parse height into float
        string heightString = buildingWay.Tags.ContainsKey("building:height") ? buildingWay.Tags.GetValue("building:height") : "";
        heightString = Regex.Replace(heightString, "[A-Za-z ]", ""); // Remove unit labels
        float temp;
        height = float.TryParse(heightString, out temp) ? float.Parse(heightString): FLOOR_HEIGHT * levels + 3;

        // Create building polygons
        List<double[]> coordList = buildingWay.Nodes.ToList().ConvertAll<double[]>(node => new double[2] {(double)node.Longitude, (double)node.Latitude});
        position = GetBuildingPosition(coordList[0][0], coordList[0][1]); // Relative position from origin to first vertex
        basePolygon = TranslateCoordsToBuildingVertices(coordList);
        roofPolygon = Array.ConvertAll(basePolygon, (baseVector => new Vector3(baseVector.x, height, baseVector.z)));
    }

    // Translates a list of GEOjson coordinates into an array of distances (in meters) relative to the first GEOjson coordinate.
    // Therefore the first coordinate will be translated to (0,0) and the rest translated to their distance in meters from this "origin".
    // Uses the approximation that 1degree latitude is 111km and 1degree longitude is 111km * cos(latitude) when distances are small
    // Approximation taken from https://stackoverflow.com/a/39540339
    private Vector3[] TranslateCoordsToBuildingVertices(List<double[]> latLonCoordList)
    {
        Vector3[] vertices = new Vector3[latLonCoordList.Count-1]; // One less element since last element in GEOjson coordinates is always first coordinate repeated
        double buildingLonOrigin = latLonCoordList[0][0];
        double buildingLatOrigin = latLonCoordList[0][1];

        for (int i = 0; i < vertices.Length; i++) 
        {
            double changeInLongitude = (latLonCoordList[i][0] - buildingLonOrigin);
            double changeInLatitude = (latLonCoordList[i][1] - buildingLatOrigin);

            double xRelative = changeInLongitude * 111316 * System.Math.Cos(buildingLatOrigin * System.Math.PI / 180d);
            double zRelative = changeInLatitude * 111111d;
            float yRelative = minLevel * FLOOR_HEIGHT;
            vertices[i] = new Vector3((float)xRelative, yRelative ,(float)zRelative);
        }
        return SimplifyBuildingShapes(vertices);
    }

    // Takes an array of vertices that describe a polygon and removes the vertices whose angle approximately
    // describes a straight line. This makes buildings have less vertices overall and makes it more likely
    // that walls will be created as one piece.
    private Vector3[] SimplifyBuildingShapes(Vector3[] vertices)
    {
        List<Vector3> necessaryVerts = new List<Vector3>();
        necessaryVerts.Add(vertices[0]);

        for (int i = 1; i < vertices.Length; i++)
        {
            float prevChangeInX = vertices[i].x - vertices[i-1].x;
            float prevChangeInZ = vertices[i].z - vertices[i-1].z;
            float nextChangeInX = vertices[i].x - vertices[(i+1) % vertices.Length].x;
            float nextChangeInZ = vertices[i].z - vertices[(i+1) % vertices.Length].z;
            float angle = Vector3.Angle(new Vector2(prevChangeInX, prevChangeInZ), new Vector2(nextChangeInX, nextChangeInZ));
            
            // Deem nearly straight angles to be unnecessary (Vector3.angle is always between 180 and 0)
            if (!(angle > 177 || angle < 3))
            {
                necessaryVerts.Add(vertices[i]);
            }
        }
        return necessaryVerts.ToArray();
    }


    // Takes a coordinate and returns a vector representing the coordinate's direction in meters from the origin (ORIGIN_LATITUDE, ORIGIN_LONGITUDE) 
    private Vector3 GetBuildingPosition(double lon, double lat){
        double changeInLongitude = lon - ORIGIN_LONGITUDE;
        double changeInLatitude = lat - ORIGIN_LATITUDE;
        double avgLat = (lat + ORIGIN_LATITUDE) / 2;

        double distFromOriginX = changeInLongitude * 111316 * System.Math.Cos(avgLat * System.Math.PI / 180d);
        double distFromOriginZ = changeInLatitude * 111111d;
        return new Vector3((float)distFromOriginX, 0, (float)distFromOriginZ);
    }
}