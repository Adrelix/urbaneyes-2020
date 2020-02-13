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
    public List<Building> GetBuildings(){
        List<Building> buildings = new List<Building>();

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
                
                Building building = new Building(id, outerPolygon, buildingHeight, buildingLevels, buildingMinLevel);
                buildings.Add(building);
            }
        }
        return buildings;
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
    }

    private class Geometry
    {
        public string type { get; set; }
        public List<dynamic> coordinates { get; set; } // If is a building's coordinates will dynamically match type List<List<List<double>>>
    }
}


// Structure for holding data of parsed buildings
public class Building
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

    public Building(string buildingID, List<double[]> roofVertexList, float? buildingHeight, int buildingLevels, int buildingMinLevel){
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

    public Building(CompleteWay buildingWay)
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
        return vertices;
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