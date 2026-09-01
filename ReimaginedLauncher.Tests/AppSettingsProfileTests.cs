using ReimaginedLauncher.Utilities;
using Xunit;

namespace ReimaginedLauncher.Tests;

public sealed class AppSettingsProfileTests
{
    [Fact]
    public void EnsureProfilesCreatesOneProfilePerInstallationType()
    {
        var settings = new AppSettings();

        settings.EnsureProfiles();

        Assert.Equal(
            Enum.GetValues<InstallationType>().Length,
            settings.Profiles.Select(profile => profile.Type).Distinct().Count());
    }

    [Fact]
    public void EnsureProfilesAppendsLutrisWithoutMovingExistingProfiles()
    {
        // A settings file written before the Lutris type existed. Indexes 0-2
        // must keep pointing at the same installations after the upgrade.
        var settings = new AppSettings
        {
            Profiles =
            [
                new InstallationProfile { Type = InstallationType.BattleNet, InstallDirectory = "/bnet" },
                new InstallationProfile { Type = InstallationType.Steam, InstallDirectory = "/steam" },
                new InstallationProfile { Type = InstallationType.D2RMM, InstallDirectory = "/d2rmm" }
            ],
            SelectedProfileIndex = 1
        };

        settings.EnsureProfiles();

        Assert.Equal(4, settings.Profiles.Count);
        Assert.Equal("/bnet", settings.Profiles[0].InstallDirectory);
        Assert.Equal("/steam", settings.Profiles[1].InstallDirectory);
        Assert.Equal("/d2rmm", settings.Profiles[2].InstallDirectory);
        Assert.Equal(InstallationType.Lutris, settings.Profiles[3].Type);
        Assert.Equal(InstallationType.Steam, settings.CurrentProfile.Type);
    }

    [Fact]
    public void EnsureProfilesIsIdempotent()
    {
        var settings = new AppSettings();

        settings.EnsureProfiles();
        settings.EnsureProfiles();

        Assert.Equal(Enum.GetValues<InstallationType>().Length, settings.Profiles.Count);
    }

    [Fact]
    public void GetProfileIndexFindsTypesRegardlessOfOrder()
    {
        var settings = new AppSettings
        {
            Profiles =
            [
                new InstallationProfile { Type = InstallationType.D2RMM },
                new InstallationProfile { Type = InstallationType.BattleNet }
            ]
        };

        Assert.Equal(0, settings.GetProfileIndex(InstallationType.D2RMM));
        Assert.Equal(1, settings.GetProfileIndex(InstallationType.BattleNet));
        Assert.Equal(InstallationType.Lutris, settings.GetProfile(InstallationType.Lutris).Type);
    }

    [Fact]
    public void NewLutrisProfileKeepsAutomaticBackupsOn()
    {
        var settings = new AppSettings();
        settings.EnsureProfiles();

        Assert.True(settings.GetProfile(InstallationType.Lutris).AutomaticBackupsEnabled);
        Assert.False(settings.GetProfile(InstallationType.D2RMM).AutomaticBackupsEnabled);
    }
}
