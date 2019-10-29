using System.Linq;

namespace K2Bridge.Visitors
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Models.Request.Aggregations.DateHistogram dateHistogram)
        {
            dateHistogram.KQL = $"{dateHistogram.Metric} by {dateHistogram.FieldName} = ";
            if (!string.IsNullOrEmpty(dateHistogram.Interval))
            {
                var period = dateHistogram.Interval[dateHistogram.Interval.Length - 1];
                switch (period)
                {
                    case 'w':
                        dateHistogram.KQL += $"startofweek({dateHistogram.FieldName})";
                        break;
                    case 'M':
                        dateHistogram.KQL += $"startofmonth({dateHistogram.FieldName})";
                        break;
                    case 'y':
                        dateHistogram.KQL += $"startofyear({dateHistogram.FieldName})";
                        break;
                    default:
                        // todatetime is redundent but we'll keep it for now
                        dateHistogram.KQL += $"bin(todatetime({dateHistogram.FieldName}), {dateHistogram.Interval})";
                        break;
                }
            }
            else
            {
                dateHistogram.KQL += dateHistogram.FieldName;
            }

            // todatetime is redundent but we'll keep it for now
            dateHistogram.KQL += $" | order by todatetime({dateHistogram.FieldName}) asc";
        }
    }
}
