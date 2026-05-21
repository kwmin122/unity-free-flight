namespace MINgo.Landing
{
    public enum SurfaceKind
    {
        Unknown,
        Runway,
        Road,
        Field,
        Ridge,
        CanyonFloor,
        Water
    }

    public enum LandingOutcome
    {
        None,
        Clean,
        Rough,
        Damaged,
        Crashed,
        Submerged
    }

    public enum LandingContext
    {
        None,
        RunwayLanding,
        RoadLanding,
        FieldLanding,
        RidgeLanding,
        CanyonFloorLanding,
        RoughLanding,
        ShortTakeoff,
        EmergencyLanding,
        Submerged,
        RestrictedAirspace
    }
}
