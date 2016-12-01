/*
    Copyright GisSense 2016
    This file is part of ShowJSON.

    ShowJSON is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ShowJSON is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with ShowJSON.If not, see<http://www.gnu.org/licenses/>.
*/
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowJSON
{
    public class JsonHelper
    {
        String orgtext = null;
        String message = "";
        List<JObject> myGeometries = null;
        IGraphicsShow showpro = null;

        public JsonHelper(string jsontext, IGraphicsShow grsw)
        {
            this.orgtext = jsontext.Trim();
            this.showpro = grsw;
        }

        public async Task<Boolean> ShowGraphics()
        {
            return await QueuedTask.Run(() =>
            {
                bool retval = false;

                myGeometries = GetJsonGeometries();
                bool failure = false;
                this.message = "JSON successfully converted and drawn";
                if (myGeometries.Count == 0)
                {
                    this.message = "Nothing to do...";
                    return failure;
                }
                foreach (JObject myGeom in myGeometries)
                {
                    ArcGIS.Core.Geometry.Geometry myGeometry =
                        this.createGeometryFromJson(myGeom.ToString()).Result;
                    if (myGeometry != null)
                    {
                        //this.showpro.addGraphic(myGeometry, this.OverlaySymbol.Result);
                        this.showpro.addGraphic(myGeometry, this.OverlaySymbolX(myGeometry.GeometryType).Result);
                    }
                    else
                    {
                        //failure
                        failure = true;
                        break;
                    }
                }
                if (failure)
                {
                    this.message = "I can't create a geometry! Check your input.";
                }
                else
                {
                    retval = true;
                    if (myGeometries != null)
                    {
                        String res = this.showpro.zoomToGeometries();
                        if (!res.Equals(""))
                        {
                            this.message = res;
                            retval = false;
                        }
                    }
                }

                return retval;
            });
        }

        private List<JObject> GetJsonGeometries()
        {
            List<JObject> retval = new List<JObject>();
            this.message = "This is not a JSON object";
            string adjustedJson = null;

            if (this.orgtext == string.Empty)
            {
                this.message = "Textbox is empty";
                return retval;
            }

            if (this.orgtext.Contains("{") || this.orgtext.Contains("["))
            {
                //It's JSON
                int acco = this.orgtext.IndexOf("{");
                int hook = this.orgtext.IndexOf("[");
                bool accoPresent = acco >= 0;
                bool hookPresent = hook >= 0;
                int startpos = -1;
                int lastpos = -1;
                string end = "}";
                if (accoPresent && hookPresent)
                {
                    startpos = acco;
                    if (acco > hook)
                    {
                        startpos = hook;
                        end = "]";
                    }
                }
                if (hookPresent && !accoPresent)
                {
                    startpos = hook;
                    end = "]";
                }
                if (accoPresent && !hookPresent)
                {
                    startpos = acco;
                }
                if (!accoPresent && !hookPresent)
                {
                    //really no JSON
                    return retval;
                }

                lastpos = this.orgtext.LastIndexOf(end);
                if (lastpos > 0)
                {
                    //start and end are known
                    int lengthJson = lastpos - startpos + 1;
                    adjustedJson = this.orgtext.Substring(startpos, lengthJson);
                    this.message = "";
                }
            }

            //check the json on features
            JObject input = JObject.Parse(adjustedJson);
            JToken featuresToken = input["features"];
            JToken sr = input["spatialReference"];
            //Featureset
            if (featuresToken != null && sr != null)
            {
                //We have a featureset
                if (featuresToken.Type == JTokenType.Array)
                {
                    JArray features = (JArray)featuresToken;
                    foreach (JObject feat in features.Select(c => (JObject)c))
                    {
                        //Add the SR and add the geometry to the list
                        JObject myGeom = (JObject)feat["geometry"];
                        myGeom.Add("spatialReference", sr);
                        retval.Add(myGeom);
                    }
                }
            }
            else if (input["x"] != null ||
                     input["paths"] != null ||
                     input["rings"] != null ||
                     input["points"] != null ||
                     input["xmin"] != null)
            {
                //It's already a geometry-object
                retval.Add(input);
            }

            return retval;
        }

        private Task<ArcGIS.Core.Geometry.Geometry> createGeometryFromJson(string json)
        {
            return QueuedTask.Run(() =>
            {
                ArcGIS.Core.Geometry.Geometry retGeom = null;
                //{"xmin":1,"ymin":2,"xmax":3,"ymax":4,"spatialReference":{"wkid":4326}}
                try
                {
                    retGeom = GeometryEngine.ImportFromJSON(JSONImportFlags.jsonImportDefaults, json);

                    switch (retGeom.GeometryType)
                    {
                        case GeometryType.Polygon:
                            break;
                        case GeometryType.Envelope:
                            retGeom = PolygonBuilder.CreatePolygon(retGeom as Envelope);
                            break;
                        case GeometryType.Point:
                            retGeom = MapPointBuilder.CreateMapPoint(retGeom as MapPoint);
                            break;
                        case GeometryType.Multipoint:
                            retGeom = MultipointBuilder.CreateMultipoint(retGeom as Multipoint);
                            break;
                        case GeometryType.Polyline:
                            retGeom = PolylineBuilder.CreatePolyline(retGeom as Polyline);
                            break;
                        default:
                            retGeom = null;
                            break;
                    }

                }
                catch
                {
                    this.message = "I can't create a geometry...";
                }

                return retGeom;
            });
        }

        private Task<CIMSymbolReference> OverlaySymbolX(GeometryType gt)
        {
            return QueuedTask.Run(() =>
            {
                CIMSymbolReference retval = null;

                switch (gt)
                {
                    case GeometryType.Polygon:
                    case GeometryType.Envelope:
                        CIMStroke outline = SymbolFactory.ConstructStroke(ColorFactory.RedRGB, 2.0, SimpleLineStyle.Solid);
                        CIMPolygonSymbol fillWithOutline =
                            SymbolFactory.ConstructPolygonSymbol(ColorFactory.RedRGB, SimpleFillStyle.Null, outline);
                        retval = fillWithOutline.MakeSymbolReference();
                        break;
                    case GeometryType.Point:
                    case GeometryType.Multipoint:
                        CIMPointSymbol ps = SymbolFactory.ConstructPointSymbol(ColorFactory.RedRGB, 5.0, SimpleMarkerStyle.Circle);
                        retval = ps.MakeSymbolReference();
                        break;
                    case GeometryType.Polyline:
                        CIMLineSymbol ls = SymbolFactory.ConstructLineSymbol(ColorFactory.RedRGB, 2.0, SimpleLineStyle.Solid);
                        retval = ls.MakeSymbolReference();
                        break;
                    default:
                        break;
                }

                return retval;
            });
        }

        public string Message
        {
            get
            {
                return this.message;
            }
        }

    }
}
