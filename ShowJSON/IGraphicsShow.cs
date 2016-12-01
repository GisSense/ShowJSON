using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowJSON
{
    public interface IGraphicsShow
    {
        void addGraphic(Geometry geom, CIMSymbolReference sbl);
        String zoomToGeometries();
    }
}
