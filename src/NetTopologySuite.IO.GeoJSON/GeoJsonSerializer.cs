﻿using System;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Json Serializer with support for GeoJson object structure.
    /// </summary>
    public class GeoJsonSerializer : JsonSerializer
    {
        /// <summary>
        /// A default output dimension value
        /// </summary>
        internal const int DefaultDimension = 3;

        /// <summary>
        /// Gets a default GeometryFactory
        /// </summary>
        internal static GeometryFactory Wgs84Factory { get; } = new GeometryFactory(new PrecisionModel(), 4326);

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>Calls <see cref="GeoJsonSerializer.CreateDefault()"/> internally</remarks>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public new static JsonSerializer Create()
        {
            return CreateDefault();
        }


        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>
        /// <see cref="GeoJsonSerializer.Wgs84Factory"/> is used.</remarks>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public new static JsonSerializer CreateDefault()
        {
            var s = JsonSerializer.CreateDefault();
            s.NullValueHandling = NullValueHandling.Ignore;

            AddGeoJsonConverters(s, GeoJsonSerializer.Wgs84Factory, DefaultDimension);
            return s;
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>
        /// <see cref="GeoJsonSerializer.Wgs84Factory"/> is used.</remarks>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public new static JsonSerializer CreateDefault(JsonSerializerSettings settings)
        {
            var s = JsonSerializer.Create(settings);
            AddGeoJsonConverters(s, GeoJsonSerializer.Wgs84Factory, DefaultDimension);

            return s;
        }
        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>
        /// Creates a serializer using <see cref="GeoJsonSerializer.Create(GeometryFactory,int)"/> internally.
        /// </remarks>
        /// <param name="factory">A factory to use when creating geometries. The factories <see cref="PrecisionModel"/>
        /// is also used to format <see cref="Coordinate.X"/> and <see cref="Coordinate.Y"/> of the coordinates.</param>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(GeometryFactory factory)
        {
            return Create(factory, DefaultDimension);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>
        /// Creates a serializer using <see cref="GeoJsonSerializer.Create(JsonSerializerSettings,GeometryFactory,int)"/> internally.
        /// </remarks>
        /// <param name="factory">A factory to use when creating geometries. The factories <see cref="PrecisionModel"/>
        /// is also used to format <see cref="Coordinate.X"/> and <see cref="Coordinate.Y"/> of the coordinates.</param>
        /// <param name="dimension">A number of dimensions that are handled. Valid inputs are 2 and 3.</param>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(GeometryFactory factory, int dimension)
        {
            return Create(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }, factory, dimension);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer using the provider serializer settings and geometry factory
        /// </summary>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, GeometryFactory factory)
        {
            return Create(settings, factory, DefaultDimension);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer using the provider serializer settings and geometry factory
        /// </summary>
        /// <param name="settings">Serializer settings</param>
        /// <param name="factory">The factory to use when creating a new geometry</param>
        /// <param name="dimension">The number of ordinates to handle</param>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, GeometryFactory factory, int dimension)
        {
            if (dimension < 2 || dimension > 3)
                throw new ArgumentException("Invalid number of ordinates. Must be in range [2,3]", nameof(dimension));

            var s = JsonSerializer.Create(settings);
            AddGeoJsonConverters(s, factory, dimension);
            return s;
        }

        private static void AddGeoJsonConverters(JsonSerializer s, GeometryFactory factory, int dimension)
        {
            if (factory.SRID != 4326)
                Trace.WriteLine($"Factory with SRID of unsupported coordinate reference system.");

            var c = s.Converters;
#pragma warning disable 618
            c.Add(new ICRSObjectConverter());
#pragma warning restore 618
            c.Add(new FeatureCollectionConverter());
            c.Add(new FeatureConverter());
            c.Add(new AttributesTableConverter());
            c.Add(new GeometryConverter(factory, dimension));
            c.Add(new GeometryArrayConverter(factory, dimension));
            c.Add(new CoordinateConverter(factory.PrecisionModel, dimension));
            c.Add(new EnvelopeConverter());

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonSerializer"/> class.
        /// </summary>
        [Obsolete("Use GeoJsonSerializer.Create...() functions")]
        public GeoJsonSerializer() : this(Wgs84Factory) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonSerializer"/> class.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        [Obsolete("Use GeoJsonSerializer.Create...() functions")]
        public GeoJsonSerializer(GeometryFactory geometryFactory)
        {
            base.Converters.Add(new ICRSObjectConverter());
            base.Converters.Add(new FeatureCollectionConverter());
            base.Converters.Add(new FeatureConverter());
            base.Converters.Add(new AttributesTableConverter());
            base.Converters.Add(new GeometryConverter(geometryFactory));
            base.Converters.Add(new GeometryArrayConverter(geometryFactory));
            base.Converters.Add(new CoordinateConverter(geometryFactory.PrecisionModel));
            base.Converters.Add(new EnvelopeConverter());
        }
    }
}
