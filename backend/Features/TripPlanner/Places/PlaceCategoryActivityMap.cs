using familly_trip_advisor.Features.TripPlanner.Models;

namespace familly_trip_advisor.Features.TripPlanner.Places
{
    public static class PlaceCategoryActivityMap
    {
        /// <summary>
        /// Leaf-level category → activity type mapping.
        /// Used for client-side classification of returned Geoapify places.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, Activity> Categories =
            new Dictionary<string, Activity>
            {
                // Entertainment — mixed, must be specified at leaf level
                { "entertainment.activity_park",            Activity.Outdoor },
                { "entertainment.activity_park.climbing",   Activity.Outdoor },
                { "entertainment.activity_park.trampoline", Activity.Indoor  },
                { "entertainment.amusement_arcade",         Activity.Indoor  },
                { "entertainment.aquarium",                 Activity.Indoor  },
                { "entertainment.bowling_alley",            Activity.Indoor  },
                { "entertainment.cinema",                   Activity.Indoor  },
                { "entertainment.culture",                  Activity.Indoor  },
                { "entertainment.culture.arts_centre",      Activity.Indoor  },
                { "entertainment.culture.gallery",          Activity.Indoor  },
                { "entertainment.culture.theatre",          Activity.Indoor  },
                { "entertainment.escape_game",              Activity.Indoor  },
                { "entertainment.flying_fox",               Activity.Outdoor },
                { "entertainment.miniature_golf",           Activity.Outdoor },
                { "entertainment.museum",                   Activity.Indoor  },
                { "entertainment.planetarium",              Activity.Indoor  },
                { "entertainment.theme_park",               Activity.Outdoor },
                { "entertainment.water_park",               Activity.Outdoor },
                { "entertainment.zoo",                      Activity.Outdoor },

                // Leisure — split: outdoor sub-groups vs indoor spa sub-group
                { "leisure.park",                           Activity.Outdoor },
                { "leisure.park.garden",                    Activity.Outdoor },
                { "leisure.park.nature_reserve",            Activity.Outdoor },
                { "leisure.picnic",                         Activity.Outdoor },
                { "leisure.picnic.bbq",                     Activity.Outdoor },
                { "leisure.picnic.picnic_site",             Activity.Outdoor },
                { "leisure.picnic.picnic_table",            Activity.Outdoor },
                { "leisure.playground",                     Activity.Outdoor },
                { "leisure.spa",                            Activity.Indoor  },
                { "leisure.spa.public_bath",                Activity.Indoor  },
                { "leisure.spa.sauna",                      Activity.Indoor  },

                // Natural — entirely outdoor, parent key covers all children
                { "natural.coastal",                        Activity.Outdoor },
                { "natural.desert",                         Activity.Outdoor },
                { "natural.forest",                         Activity.Outdoor },
                { "natural.heath_moor",                     Activity.Outdoor },
                { "natural.mountain",                       Activity.Outdoor },
                { "natural.mountain.cave_entrance",         Activity.Outdoor },
                { "natural.mountain.cliff",                 Activity.Outdoor },
                { "natural.mountain.fell",                  Activity.Outdoor },
                { "natural.mountain.glacier",               Activity.Outdoor },
                { "natural.mountain.hill",                  Activity.Outdoor },
                { "natural.mountain.peak",                  Activity.Outdoor },
                { "natural.mountain.rock",                  Activity.Outdoor },
                { "natural.mountain.volcano",               Activity.Outdoor },
                { "natural.protected_area",                 Activity.Outdoor },
                { "natural.sand",                           Activity.Outdoor },
                { "natural.sand.dune",                      Activity.Outdoor },
                { "natural.water",                          Activity.Outdoor },
                { "natural.water.bay",                      Activity.Outdoor },
                { "natural.water.geyser",                   Activity.Outdoor },
                { "natural.water.hot_spring",               Activity.Outdoor },
                { "natural.water.reef",                     Activity.Outdoor },
                { "natural.water.river_system",             Activity.Outdoor },
                { "natural.water.sea",                      Activity.Outdoor },
                { "natural.water.spring",                   Activity.Outdoor },
                { "natural.water.whitewater",               Activity.Outdoor },
                { "natural.wetland",                        Activity.Outdoor },
            };

        /// <summary>
        /// Optimised Geoapify API category strings per activity type.
        /// Groups that are entirely one activity use their parent key (shorter URL).
        /// Mixed groups (entertainment) are enumerated at leaf level.
        /// </summary>
        public static readonly IReadOnlyDictionary<Activity, IReadOnlyList<string>> ApiCategories =
            new Dictionary<Activity, IReadOnlyList<string>>
            {
                [Activity.Outdoor] =
                [
                    // natural — 100% outdoor, one parent key covers everything
                    "natural",
                    // leisure outdoor sub-groups — parent keys cover all children
                    "leisure.park",
                    "leisure.picnic",
                    "leisure.playground",
                    // entertainment outdoor leaf keys (group is mixed)
                    "entertainment.activity_park",
                    "entertainment.activity_park.climbing",
                    "entertainment.flying_fox",
                    "entertainment.miniature_golf",
                    "entertainment.theme_park",
                    "entertainment.water_park",
                    "entertainment.zoo",
                ],

                [Activity.Indoor] =
                [
                    // leisure.spa — 100% indoor, one parent key covers all children
                    "leisure.spa",
                    // entertainment indoor leaf keys (group is mixed)
                    "entertainment.activity_park.trampoline",
                    "entertainment.amusement_arcade",
                    "entertainment.aquarium",
                    "entertainment.bowling_alley",
                    "entertainment.cinema",
                    "entertainment.culture",
                    "entertainment.escape_game",
                    "entertainment.museum",
                    "entertainment.planetarium",
                ],

                [Activity.Both] =
                [
                    "entertainment",
                    "leisure",
                    "natural",
                ]
            };
    }
}
