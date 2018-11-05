﻿using GeoJSON.Net.Feature;
using WB.Core.BoundedContexts.Headquarters.Clustering;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class MapReportView
    {
        public GeoBounds InitialBounds { get; set; }
        public FeatureCollection FeatureCollection { get; set; }
        public int TotalPoint { get; set; }
    }
}
