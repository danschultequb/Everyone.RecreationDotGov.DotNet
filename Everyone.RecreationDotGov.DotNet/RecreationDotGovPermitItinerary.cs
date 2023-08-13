using System.Collections.Generic;
using System.Diagnostics;

namespace Everyone
{
    [DebuggerDisplay($"{nameof(Id)}: {{{nameof(Id)}}}, {nameof(Name)}: {{{nameof(Name)}}}")]
    public class RecreationDotGovPermitItinerary
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public IEnumerable<RecreationDotGovDivision>? Divisions { get; set; }
    }
}
