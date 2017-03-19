using Fhir.Metrics;
using FM = Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spark.Search;
using Spark.Engine.Model;
using Hl7.Fhir.Introspection;

namespace Spark.Engine.Extensions
{
    public static class RatioExtensions
    {
        public static Expression ToExpression(this FM.Ratio ratio)
        {
            var values = new List<ValueExpression>();
            values.Add(new IndexValue("n_value", new NumberValue(ratio.Numerator.Value.Value)));
            values.Add(new IndexValue("n_unit", new StringValue(ratio.Numerator.Code)));
            values.Add(new IndexValue("d_value", new NumberValue(ratio.Denominator.Value.Value)));
            values.Add(new IndexValue("d_unit", new StringValue(ratio.Denominator.Code)));

            return new CompositeValue(values);
        }
        public static Expression ToExpression(this FM.Timing value)
        {
            var values = new List<ValueExpression>();
            if (value.Code != null)
                values.Add(new IndexValue("code", new StringValue(value.Code.Coding.FirstOrDefault()?.Code)));

            return new CompositeValue(values);
        }

        public static Expression ToExpression(this FM.Location.PositionComponent value)
        {
            var values = new List<ValueExpression>();
            values.Add(new IndexValue("latitude", new NumberValue(value.Latitude.Value)));
            values.Add(new IndexValue("longitude", new NumberValue(value.Longitude.Value)));

            return new CompositeValue(values);
        }
    }
}
