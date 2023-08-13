using System.Collections.Generic;

namespace Everyone
{
    public class RecreationDotGovDivisionAvailability
    {
        public int? MinimumGroupSize { get; set; }

        public int? MaximumGroupSize { get; set; }

        public IEnumerable<RecreationDotGovDivisionDayAvailability>? DayAvailabilities { get; set; }
    }
}
