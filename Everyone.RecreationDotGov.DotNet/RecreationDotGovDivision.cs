using System.Diagnostics;

namespace Everyone
{
    [DebuggerDisplay($"{nameof(Id)}: {{{nameof(Id)}}}, {nameof(Name)}: {{{nameof(Name)}}}, {nameof(Type)}: {{{nameof(Type)}}}, {nameof(District)}: {{{nameof(District)}}}")]
    public class RecreationDotGovDivision
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Type { get; set; }

        public string? District { get; set; }
    }
}
