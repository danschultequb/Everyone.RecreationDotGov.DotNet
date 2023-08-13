using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Everyone
{
    public class RecreationDotGovClient : Disposable
    {
        private readonly HttpClient httpClient;

        private RecreationDotGovClient()
        {
            this.httpClient = new HttpClient();
        }

        public static RecreationDotGovClient Create()
        {
            return new RecreationDotGovClient();
        }

        public bool Disposed { get; private set; }

        public bool Dispose()
        {
            bool result = !this.Disposed;
            if (result)
            {
                this.httpClient.Dispose();
            }
            return result;
        }

        public async Task<RecreationDotGovPermitItinerary?> GetPermitItineraryAsync(string permitItineraryId)
        {
            using (HttpResponseMessage response = await this.httpClient.GetAsync($"https://www.recreation.gov/api/permitcontent/{permitItineraryId}"))
            {
                response.EnsureSuccessStatusCode();

                JObject? content = JObject.Parse(await response.Content.ReadAsStringAsync());
                return RecreationDotGovClient.ParsePermitItinerary(content?.GetValue("payload") as JObject);
            }
        }

        public async Task<RecreationDotGovDivisionAvailability?> GetDivisionAvailabilityAsync(string permitItineraryId, string divisionId, int month, int year)
        {
            using (HttpResponseMessage response = await this.httpClient.GetAsync($"https://www.recreation.gov/api/permititinerary/{permitItineraryId}/division/{divisionId}/availability/month?month={month}&year={year}"))
            {
                response.EnsureSuccessStatusCode();

                JObject? content = JObject.Parse(await response.Content.ReadAsStringAsync());
                return RecreationDotGovClient.ParseDivisionAvailability(content?.GetValue("payload") as JObject);
            }
        }

        public static RecreationDotGovPermitItinerary? ParsePermitItinerary(JObject? json)
        {
            RecreationDotGovPermitItinerary? result = null;

            if (json != null)
            {
                result = new RecreationDotGovPermitItinerary
                {
                    Id = (string?)json.GetValue("id"),
                    Name = (string?)json.GetValue("name"),
                };

                JObject? divisionsJson = json.GetValue("divisions") as JObject;
                if (divisionsJson != null)
                {
                    List<RecreationDotGovDivision> divisions = new List<RecreationDotGovDivision>();
                    foreach (JProperty propertyJson in divisionsJson.Properties())
                    {
                        RecreationDotGovDivision? division = RecreationDotGovClient.ParseDivision(propertyJson.Value as JObject);
                        if (division != null)
                        {
                            divisions.Add(division);
                        }
                    }
                    result.Divisions = divisions;
                }
            }

            return result;
        }

        public static RecreationDotGovDivision? ParseDivision(JObject? json)
        {
            RecreationDotGovDivision? result = null;

            if (json != null)
            {
                result = new RecreationDotGovDivision
                {
                    Id = (string?)json.GetValue("id"),
                    Name = (string?)json.GetValue("name"),
                    Type = (string?)json.GetValue("type"),
                    District = (string?)json.GetValue("district"),
                };
            }

            return result;
        }

        public static RecreationDotGovDivisionAvailability? ParseDivisionAvailability(JObject? json)
        {
            RecreationDotGovDivisionAvailability? result = null;

            if (json != null)
            {
                result = new RecreationDotGovDivisionAvailability();

                JObject? boolsJson = json.GetValue("bools") as JObject;
                IDictionary<string, bool> boolsDictionary = new Dictionary<string,bool>();
                if (boolsJson != null)
                {
                    boolsDictionary = boolsJson.Properties().ToDictionary(
                        keySelector: (JProperty property) => property.Name,
                        elementSelector: (JProperty property) => (bool)property.Value);
                }

                JArray? rulesJson = json.GetValue("rules") as JArray;
                if (rulesJson != null)
                {
                    foreach (JObject? rule in rulesJson)
                    {
                        if (rule != null)
                        {
                            string? ruleName = (string?)rule.GetValue("name");
                            switch (ruleName)
                            {
                                case "MinGroupSize":
                                    result.MinimumGroupSize = (int?)rule.GetValue("value");
                                    break;

                                case "MaxGroupSize":
                                    result.MaximumGroupSize = (int?)rule.GetValue("value");
                                    break;
                            }
                        }
                    }
                }

                JObject? quotaTypeMapsJson = json.GetValue("quota_type_maps") as JObject;
                JObject? constantQuotaUsageDaily = quotaTypeMapsJson?.GetValue("QuotaUsageByMemberDaily") as JObject;
                if (constantQuotaUsageDaily != null)
                {
                    List<RecreationDotGovDivisionDayAvailability> dayAvailabilities = new List<RecreationDotGovDivisionDayAvailability>();
                    foreach (JProperty propertyJson in constantQuotaUsageDaily.Properties())
                    {
                        DateTime date = DateTime.Parse(propertyJson.Name);
                        JObject? propertyValue = propertyJson.Value as JObject;
                        if (propertyValue != null)
                        {
                            RecreationDotGovDivisionDayAvailability? dayAvailability = new RecreationDotGovDivisionDayAvailability
                            {
                                Year = date.Year,
                                Month = date.Month,
                                Day = date.Day,
                                TotalSpots = (int?)propertyValue?.GetValue("total"),
                            };

                            if (boolsDictionary.TryGetValue(propertyJson.Name, out bool available) && available)
                            {
                                dayAvailability.Walkup = (bool?)propertyValue?.GetValue("show_walkup");
                                dayAvailability.ReservationsRemaining = (int?)propertyValue?.GetValue("remaining");
                            }
                            else
                            {
                                dayAvailability.Walkup = false;
                                dayAvailability.ReservationsRemaining = 0;
                            }

                            dayAvailabilities.Add(dayAvailability);
                        }
                    }
                    result.DayAvailabilities = dayAvailabilities;
                }
            }

            return result;
        }
    }
}