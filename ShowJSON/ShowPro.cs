using System;
using System.Collections.Generic;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;
using System.Linq;

namespace ShowJSON
{
    internal class ShowPro : Module, IGraphicsShow
    {
        private static ShowPro _this = null;
        private static IList<IDisposable> _addOns = new List<IDisposable>();
        private static IList<Geometry> _geoms = new List<Geometry>();
        private static Envelope totalExtent = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static ShowPro Current
        {
            get
            {
                return _this ?? (_this = (ShowPro)FrameworkApplication.FindModule("ShowJSON_Module"));
            }
        }

        public static Boolean ClearGraphics()
        {
            Boolean retval = false;
            
            foreach (IDisposable dp in _addOns)
            {
                dp.Dispose();
            }
            _addOns.Clear();
            _geoms.Clear();
            totalExtent = null;
            retval = true;

            return retval;
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        public void addGraphic(Geometry geom, CIMSymbolReference sbl)
        {
            _addOns.Add(MapView.Active.AddOverlay(geom, sbl));
            _geoms.Add(geom);
        }

        public string zoomToGeometries()
        {
            String retval = "";

            IEnumerable<int> uniqueSR = _geoms.Select(c => c.SpatialReference.Wkid).Distinct();
            if (uniqueSR.Count() > 1)
            {
                //Too many different SR's
                retval = "Unable to zoom: different SR's are being used";
                return retval;
            }

            if (totalExtent == null && _geoms.Count > 0)
            {
                if (_geoms[0].GeometryType == GeometryType.Point)
                {
                    totalExtent = _geoms[0].Extent.Expand(10, 10, false);
                }
                else
                {
                    totalExtent = _geoms[0].Extent;
                }
            }

            if (_geoms.Count > 0)
            {
                QueuedTask.Run(() => {
                    try
                    {
                        foreach (Geometry geom in _geoms)
                        {
                            if (geom.GeometryType == GeometryType.Point)
                            {
                                totalExtent = totalExtent.Union(geom.Extent.Expand(10, 10, true));
                            }
                            else
                            {
                                totalExtent = totalExtent.Union(geom.Extent);
                            }
                        }

                        totalExtent = totalExtent.Expand(1.1, 1.1, true);
                        MapView.Active.ZoomTo(totalExtent);
                    }
                    catch
                    {
                        retval = "Unable to zoom to all geometries";
                    }
                });
            }

            return retval;
        }

        #endregion Overrides

    }
}
