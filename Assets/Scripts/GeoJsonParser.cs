using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

// Handles the parsing of a GEOjson file into Building objects (defined below)
// To generate GEOjson files go to https://overpass-turbo.eu/
// GEOjson documentation: https://tools.ietf.org/html/rfc7946
public class GeoJsonParser
{
    public TextAsset geojson;
    private FeatureCollection parsedData;
    // origin describes some point in the middle of Klarabergsgatan
    private const double ORIGIN_LONGITUDE = 18.062910d;
    private const double ORIGIN_LATITUDE = 59.332349d;


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
                Building building = new Building();
                building.id = feature.id;
                building.buildingLevels = feature.properties.buildingLevels;
                List<List<double>> outerPolygon = feature.geometry.coordinates[0].ToObject<List<List<double>>>(); // Only take first polygon, the rest describe the polygon's holes
                building.position = GetBuildingPosition(outerPolygon[0][0], outerPolygon[0][1]);
                TranslateCoordsToBuildingShape(outerPolygon);
                building.basePolygon = outerPolygon;
                buildings.Add(building);
            }
        }
        return buildings;
    }


    // Translates a list of GEOjson coordinates into a list of distances in meters relative to the first coordinate of the list.
    // Therefore the first coordinate will be translated to (0,0) and the rest translated to their distance in meters from this "origin".
    // Uses the approximation that 1degree latitude is 111km and 1degree longitude is 111km * cos(latitude) when distances are small
    private void TranslateCoordsToBuildingShape(List<List<double>> latLonCoordList)
    {
        latLonCoordList.RemoveAt(latLonCoordList.Count - 1); // Last element in GEOjson coordinates is always first coordinate repeated
        double buildingLonOrigin = latLonCoordList[0][0];
        double buildingLatOrigin = latLonCoordList[0][1];

        for (int i = 0; i < latLonCoordList.Count; i++)
        {
            double changeInLongitude = (latLonCoordList[i][0] - buildingLonOrigin);
            double changeInLatitude = (latLonCoordList[i][1] - buildingLatOrigin);
            latLonCoordList[i][0] = changeInLongitude * 111316 * System.Math.Cos(buildingLatOrigin * System.Math.PI / 180d);
            latLonCoordList[i][1] = changeInLatitude * 111111d;
        }
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
        public int buildingLevels { get; set; }
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
    public string id { get; set; }
    public int buildingLevels { get; set; }
    public List<List<double>> basePolygon { get; set; } // set of points describing polygon shape of building's base
    public Vector3 position { get; set; } // position of first point in basePolygon in relation to (ORIGIN_LATITUDE, ORIGIN_LONGITUDE) (in meters)
}