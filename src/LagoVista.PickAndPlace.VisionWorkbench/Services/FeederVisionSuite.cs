using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.VisionWorkbench.Services;

/// <summary>
/// Versioned engineering baseline. Tests never read the user's working profile or session.
/// Update this file in Git when intentionally changing the expected behavior.
/// Images are versioned separately as repository fixtures.
/// </summary>
public static class FeederVisionSuite
{
    public const string Version = "feeder1-baseline-v1";
    public const double HolePitchMm = 4.0;
    public const double CanonicalExpectedSpacingPx = 202.03;
    public const double CanonicalSpacingTolerancePx = 8.0;

    public sealed record Scenario(string Name, string FileName, int? ExpectedCircles, int? ExpectedAccepted);
    public static readonly IReadOnlyList<Scenario> Scenarios = new[]
    {
        new Scenario("000 Canonical", "Feeder1-000-Canonical.png", 3, 3),
        new Scenario("010 Low light", "Feeder1-010-LowLight.png", null, null),
        new Scenario("020 High exposure", "Feeder1-020-HighExposure.png", null, null),
        new Scenario("030 Rotated", "Feeder1-030-Rotated.png", null, null),
        new Scenario("040 Extra circle", "Feeder1-040-ExtraCircle.png", null, null)
    };

    public static VisionProfile CreateProfile() => new()
    {
        Name = "Feeder1 regression baseline",
        Key = VisionProfile.VisionProfile_Custom,
        Id = "feeder1-regression-baseline-v1",
        FindCircles = true,
        FindRectangles = false,
        ApplyThreshold = false,
        Invert = false,
        UseBlurredImage = false,
        PixelsPerMM = 50,
        HoughCirclesMinRadius = 0.6,
        HoughCirclesMaxRadius = 1.1,
        HoughCirclesMinDistance = 3,
        HoughCirclesDP = 1,
        HoughCirclesParam1 = 93,
        HoughCirclesParam2 = 30
    };
}
