using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq; // for from clauses
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEditor;

using OsmSharp.Streams;
using OsmSharp.Complete;

// TODO: create function which gets the osm data of streets
// TODO: Handle osm geo information which describe building polygons counter-clockwise

// Handles the parsing of Open Street Maps XML files.
// OsmSharp repo: https://github.com/OsmSharp/core
// Osm XML representation: https://wiki.openstreetmap.org/wiki/OSM_XML
// Osm features: https://wiki.openstreetmap.org/wiki/Map_Features
public class OsmParser
{
    public TextAsset osmData;

    public OsmParser(TextAsset osmXMLData)
    {
        osmData = osmXMLData;
    }

    // TODO: improve building filtering (miss buildings described by relations)
    // Parses osmData and builds an array of CompleteWay objects with a "building" tag. Each object contains all the
    // OSM data related to a certain building in its fields.
    public CompleteWay[] GetBuildingWays(){
        using (var fileStream = new FileInfo(AssetDatabase.GetAssetPath(osmData)).OpenRead())
        {
            XmlOsmStreamSource source = new XmlOsmStreamSource(fileStream);

            // Get all building ways and nodes 
            var buildings = from osmGeo in source
                            where osmGeo.Type == OsmSharp.OsmGeoType.Node ||
                            (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.ContainsKey("building"))
                            select osmGeo;

            // Should filter before calling ToComplete() to reduce memory usage
            var completes = buildings.ToComplete(); // Create Complete objects (for Ways gives them a list of Node objects)w
            var ways = from osmGeo in completes
                       where osmGeo.Type == OsmSharp.OsmGeoType.Way
                       select osmGeo;
            CompleteWay[] completeWays = ways.Cast<CompleteWay>().ToArray();
            return completeWays;
        }
    }
}
