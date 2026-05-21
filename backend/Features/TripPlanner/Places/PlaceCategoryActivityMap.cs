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

                // Sport — split by typical indoor/outdoor nature
                { "sport.dive_centre",                      Activity.Outdoor },
                { "sport.dojo",                             Activity.Indoor  },
                { "sport.fishing",                          Activity.Outdoor },
                { "sport.fitness",                          Activity.Indoor  },
                { "sport.fitness.fitness_centre",           Activity.Indoor  },
                { "sport.fitness.fitness_station",          Activity.Outdoor },
                { "sport.fitness.gym",                      Activity.Indoor  },
                { "sport.golf_course",                      Activity.Outdoor },
                { "sport.horse_riding",                     Activity.Outdoor },
                { "sport.ice_rink",                         Activity.Indoor  },
                { "sport.pitch",                            Activity.Outdoor },
                { "sport.shooting",                         Activity.Both    },
                { "sport.skateboard",                       Activity.Outdoor },
                { "sport.sports_centre",                    Activity.Both    },
                { "sport.sports_hall",                      Activity.Indoor  },
                { "sport.stadium",                          Activity.Outdoor },
                { "sport.swimming_pool",                    Activity.Both    },
                { "sport.track",                            Activity.Outdoor },

                // Tourism — mostly outdoor / mixed sights
                { "tourism.attraction",                     Activity.Outdoor },
                { "tourism.attraction.artwork",             Activity.Outdoor },
                { "tourism.attraction.viewpoint",           Activity.Outdoor },
                { "tourism.attraction.fountain",            Activity.Outdoor },
                { "tourism.sights",                         Activity.Outdoor },
                { "tourism.sights.castle",                  Activity.Outdoor },
                { "tourism.sights.archaeological_site",     Activity.Outdoor },
                { "tourism.sights.monastery",               Activity.Outdoor },
                { "tourism.sights.ruines",                  Activity.Outdoor },
                { "tourism.sights.tower",                   Activity.Outdoor },
                { "tourism.sights.lighthouse",              Activity.Outdoor },
                { "tourism.sights.windmill",                Activity.Outdoor },
                { "tourism.sights.memorial",                Activity.Outdoor },
                { "tourism.sights.place_of_worship",        Activity.Indoor  },
                { "tourism.sights.conference_centre",       Activity.Indoor  },

                // Camping / beach — outdoor
                { "camping",                                Activity.Outdoor },
                { "camping.camp_site",                      Activity.Outdoor },
                { "camping.caravan_site",                   Activity.Outdoor },
                { "camping.summer_camp",                    Activity.Outdoor },
                { "beach",                                  Activity.Outdoor },
                { "beach.beach_resort",                     Activity.Outdoor },

                // Ski — outdoor
                { "ski",                                    Activity.Outdoor },
                { "ski.lift",                               Activity.Outdoor },
                { "ski.lift.cable_car",                     Activity.Outdoor },
                { "ski.lift.chair_lift",                    Activity.Outdoor },
                { "ski.lift.gondola",                       Activity.Outdoor },

                // Rental — follows activity context
                { "rental.bicycle",                         Activity.Outdoor },
                { "rental.boat",                            Activity.Outdoor },
                { "rental.ski",                             Activity.Outdoor },

                // Activity (clubs / community)
                { "activity.community_center",              Activity.Indoor  },
                { "activity.events_venue",                  Activity.Indoor  },
                { "activity.sport_club",                    Activity.Both    },
                { "activity.hackerspace",                   Activity.Indoor  },

                // National park — outdoor
                { "national_park",                          Activity.Outdoor },
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
                    "national_park",
                    // leisure outdoor sub-groups (spa is indoor so use leaf keys)
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
                    // sport outdoor
                    "sport.dive_centre",
                    "sport.fishing",
                    "sport.fitness.fitness_station",
                    "sport.golf_course",
                    "sport.horse_riding",
                    "sport.pitch",
                    "sport.skateboard",
                    "sport.stadium",
                    "sport.track",
                    // tourism outdoor
                    "tourism.attraction",
                    "tourism.sights",
                    // camping & beach
                    "camping",
                    "beach",
                    // ski
                    "ski",
                    // rental outdoor
                    "rental.bicycle",
                    "rental.boat",
                    "rental.ski",
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
                    // sport indoor
                    "sport.dojo",
                    "sport.fitness.fitness_centre",
                    "sport.fitness.gym",
                    "sport.ice_rink",
                    "sport.sports_hall",
                    // tourism indoor sights
                    "tourism.sights.place_of_worship",
                    "tourism.sights.conference_centre",
                    // activity (clubs / venues)
                    "activity.community_center",
                    "activity.events_venue",
                    "activity.hackerspace",
                ],

                [Activity.Both] =
                [
                    "entertainment",
                    "leisure",
                    "natural",
                    "sport",
                    "tourism",
                    "camping",
                    "beach",
                    "ski",
                    "activity",
                ]
            };
    }
}
