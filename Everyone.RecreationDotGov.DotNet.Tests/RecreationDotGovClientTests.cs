using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Everyone
{
    public static class RecreationDotGovClientTests
    {
        private const string WonderlandTrailPermitItineraryId = "4675317";

        public static void Test(TestRunner runner)
        {
            runner.TestGroup(typeof(RecreationDotGovClient), () =>
            {
                runner.Test("Create()", (Test test) =>
                {
                    using (RecreationDotGovClient client = RecreationDotGovClient.Create())
                    {
                        test.AssertNotNull(client);
                    }
                });

                runner.TestGroup("GetPermitItineraryAsync(long)", () =>
                {
                    runner.Test($"with {RecreationDotGovClientTests.WonderlandTrailPermitItineraryId}", (Test test) =>
                    {
                        using (RecreationDotGovClient client = RecreationDotGovClient.Create())
                        {
                            RecreationDotGovPermitItinerary? permitItinerary = client.GetPermitItineraryAsync(RecreationDotGovClientTests.WonderlandTrailPermitItineraryId).Await();
                            test.AssertNotNull(permitItinerary);
                            test.AssertEqual(RecreationDotGovClientTests.WonderlandTrailPermitItineraryId, permitItinerary.Id);
                            test.AssertEqual("Mount Rainier National Park Wilderness and Climbing Permits", permitItinerary.Name);
                            test.AssertEqual(190, permitItinerary.Divisions?.Count());
                        }
                    });
                });

                runner.TestGroup("GetDivisionAvailability(string,string,int,int)", () =>
                {
                    runner.Test($"with {Language.AndList(RecreationDotGovClientTests.WonderlandTrailPermitItineraryId, 46753170001, 7, 2023)}", (Test test) =>
                    {
                        using (RecreationDotGovClient client = RecreationDotGovClient.Create())
                        {
                            RecreationDotGovDivisionAvailability? divisionAvailability = client.GetDivisionAvailabilityAsync(
                                permitItineraryId: RecreationDotGovClientTests.WonderlandTrailPermitItineraryId,
                                divisionId: "46753170001",
                                month: 7,
                                year: 2023)
                                .Await();
                            test.AssertNotNull(divisionAvailability);
                            test.AssertEqual(1, divisionAvailability.MinimumGroupSize);
                            test.AssertEqual(5, divisionAvailability.MaximumGroupSize);
                            test.AssertNull(divisionAvailability.DayAvailabilities);
                        }
                    });

                    runner.Test($"with {Language.AndList(RecreationDotGovClientTests.WonderlandTrailPermitItineraryId, 46753170001, 8, 2023)}", (Test test) =>
                    {
                        using (RecreationDotGovClient client = RecreationDotGovClient.Create())
                        {
                            RecreationDotGovDivisionAvailability? divisionAvailability = client.GetDivisionAvailabilityAsync(
                                permitItineraryId: RecreationDotGovClientTests.WonderlandTrailPermitItineraryId,
                                divisionId: "46753170001",
                                month: 8,
                                year: 2023)
                                .Await();
                            test.AssertNotNull(divisionAvailability);
                            test.AssertEqual(1, divisionAvailability.MinimumGroupSize);
                            test.AssertEqual(5, divisionAvailability.MaximumGroupSize);
                            test.AssertNotNull(divisionAvailability.DayAvailabilities);
                        }
                    });

                    runner.Test($"with {Language.AndList(RecreationDotGovClientTests.WonderlandTrailPermitItineraryId, 46753170001, 9, 2023)}", (Test test) =>
                    {
                        using (RecreationDotGovClient client = RecreationDotGovClient.Create())
                        {
                            RecreationDotGovDivisionAvailability? divisionAvailability = client.GetDivisionAvailabilityAsync(
                                permitItineraryId: RecreationDotGovClientTests.WonderlandTrailPermitItineraryId,
                                divisionId: "46753170001",
                                month: 9,
                                year: 2023)
                                .Await();
                            test.AssertNotNull(divisionAvailability);
                            test.AssertEqual(1, divisionAvailability.MinimumGroupSize);
                            test.AssertEqual(5, divisionAvailability.MaximumGroupSize);
                            test.AssertNotNull(divisionAvailability.DayAvailabilities);
                        }
                    });
                });

                runner.Test("Create availability document", (Test test) =>
                {
                    const string permitItineraryId = RecreationDotGovClientTests.WonderlandTrailPermitItineraryId;
                    const int month = 8;
                    const int year = 2023;

                    using (RecreationDotGovClient client = RecreationDotGovClient.Create())
                    {
                        RecreationDotGovPermitItinerary? pi = client.GetPermitItineraryAsync(permitItineraryId).Await();
                        test.AssertNotNull(pi);
                        test.AssertNotNull(pi.Name);

                        using (TextWriter writer = File.CreateText($"C:\\Users\\dansc\\OneDrive\\Desktop\\Availability-{month}-{year}.csv"))
                        {
                            writer.WriteLine($"Name:,{pi.Name}");
                            
                            writer.WriteLine();

                            if (pi.Divisions != null)
                            {
                                ISet<string> hikingDistricts = new HashSet<string>
                                {
                                    "Carbon River Area Camps",
                                    "Longmire Area Camps",
                                    "White River Area Camps",
                                };

                                List<RecreationDotGovDivision> hikingDivisions = pi.Divisions
                                    .Where(division => !string.IsNullOrEmpty(division.District) && hikingDistricts.Contains(division.District))
                                    .OrderBy(division => $"{division.District} {division.Name}")
                                    .ToList();

                                bool wroteHeader = false;
                                foreach (RecreationDotGovDivision division in hikingDivisions)
                                {
                                    RecreationDotGovDivisionAvailability? divisionAvailability = client.GetDivisionAvailabilityAsync(
                                        permitItineraryId: permitItineraryId,
                                        divisionId: division.Id!,
                                        month: month,
                                        year: year).Await();
                                    IEnumerable<RecreationDotGovDivisionDayAvailability>? divisionDayAvailabilities = divisionAvailability?.DayAvailabilities;
                                    if (divisionDayAvailabilities != null)
                                    {
                                        if (!wroteHeader)
                                        {
                                            wroteHeader = true;

                                            writer.Write($"{nameof(division.District)},{nameof(division.Name)},{nameof(division.Id)}");
                                            foreach (RecreationDotGovDivisionDayAvailability dayAvailability in divisionDayAvailabilities)
                                            {
                                                writer.Write($",{dayAvailability.Month}/{dayAvailability.Day}");
                                            }
                                            writer.WriteLine();
                                        }

                                        writer.Write($"{division.District},{division.Name},{division.Id}");
                                        foreach (RecreationDotGovDivisionDayAvailability dayAvailability in divisionDayAvailabilities)
                                        {
                                            string availability;
                                            if (dayAvailability.Walkup == true)
                                            {
                                                availability = "W";
                                            }
                                            else if (dayAvailability.ReservationsRemaining != null && dayAvailability.ReservationsRemaining > 0)
                                            {
                                                availability = dayAvailability.ReservationsRemaining.ToString()!;
                                            }
                                            else
                                            {
                                                availability = "";
                                            }

                                            writer.Write($",{availability}");
                                        }
                                        writer.WriteLine();
                                    }
                                }
                            }
                        }
                    }
                });
            });
        }
    }
}