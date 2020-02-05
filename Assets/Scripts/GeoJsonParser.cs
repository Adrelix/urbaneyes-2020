﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

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

    // Gets a list of the perimeter coordinates for each building parsed
    // TODO: Convert GEOjson latitude/longitude to meters
    public List<List<Vector2>> GetBuildingPerimeterVertices(){
        List<List<Vector2>> buildingVerts = new List<List<Vector2>>();

        foreach (Feature feature in parsedData.features)
        {
            // If 3D building
            if (feature.properties.building != null && (feature.geometry.type == "Polygon" || feature.geometry.type == "MultiPolygon"))
            {
                foreach (Newtonsoft.Json.Linq.JArray coords in feature.geometry.coordinates)
                {
                    List<Vector2> polygonVerts = new List<Vector2>();
                    foreach (List<double> coord in coords.ToObject<List<List<double>>>())
                    {
                        polygonVerts.Add(new Vector2((float)coord[0], (float)coord[1])); // WARNING: Vector2 only has floating point precision which isnt enough for GEOjson's latitude and longitude values
                    }
                    buildingVerts.Add(polygonVerts);
                }
            }
        }
        return buildingVerts;
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
        public int buildinglevels { get; set; }
        public string type { get; set; }
    }

    private class Geometry
    {
        public string type { get; set; }
        public List<dynamic> coordinates { get; set; } // If a building coordinates will dynamically match type List<List<List<double>>>
    }
}
