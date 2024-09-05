using System.Linq;
using Content.Client.GameTicking.Managers;
using Content.Shared.Roles;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client._Sunrise.Latejoin;

[GenerateTypedNameReferences]
public sealed partial class StationsListControl : BoxContainer
{

    private ClientGameTicker _gameTicker;

    public Comparison<NetEntity>? Comparison;

    public NetEntity? Selected
    {
        get
        {
            var i = Enumerable.FirstOrDefault<ItemList.Item>(StationsItemList.GetSelected());
            if (i is null)
                return null;

            return (NetEntity) i.Metadata!;
        }
    }

    private IReadOnlyDictionary<NetEntity, Dictionary<ProtoId<JobPrototype>, int?>>? _lastJobState;

    public StationsListControl()
    {
        _gameTicker = EntitySystem.Get<ClientGameTicker>();
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);
        _gameTicker.LobbyJobsAvailableUpdated += UpdateUi;
        Comparison = DefaultComparison;

        UpdateUi(_gameTicker.JobsAvailable);

        FilterLineEdit.OnTextChanged += _ =>
        {
            if (_lastJobState != null)
                UpdateUi(_lastJobState);
        };
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _gameTicker.LobbyJobsAvailableUpdated -= UpdateUi;
    }

    private int DefaultComparison(NetEntity x, NetEntity y)
    {
        var xContainsSR = _gameTicker.JobsAvailable[x].ContainsKey("StationRepresentative");
        var yContainsSR = _gameTicker.JobsAvailable[y].ContainsKey("StationRepresentative");

        var xContainsSheriff = _gameTicker.JobsAvailable[x].ContainsKey("Sheriff");
        var yContainsSheriff = _gameTicker.JobsAvailable[y].ContainsKey("Sheriff");

        var xContainsPirateCaptain = _gameTicker.JobsAvailable[x].ContainsKey("PirateCaptain");
        var yContainsPirateCaptain = _gameTicker.JobsAvailable[y].ContainsKey("PirateCaptain");

        // Prioritize "StationRepresentative"
        switch (xContainsSR)
        {
            case true when !yContainsSR:
                return -1;
            case false when yContainsSR:
                return 1;
        }

        // If both or neither contain "StationRepresentative", prioritize "Sheriff"
        switch (xContainsSheriff)
        {
            case true when !yContainsSheriff:
                return -1;
            case false when yContainsSheriff:
                return 1;
        }

        // If both or neither contain "StationRepresentative", "Sheriff" prioritize "PirateCaptain"
        switch (xContainsPirateCaptain)
        {
            case true when !yContainsPirateCaptain:
                return -1;
            case false when yContainsPirateCaptain:
                return 1;
        }

        // If both or neither contain "StationRepresentative" and "Sheriff", sort by jobCountComparison
        var jobCountComparison = -(int) (_gameTicker.JobsAvailable[x].Values.Sum(a => a ?? 0) -
                                         _gameTicker.JobsAvailable[y].Values.Sum(b => b ?? 0));
        var nameComparison = string.Compare(_gameTicker.StationNames[x], _gameTicker.StationNames[y], StringComparison.Ordinal);

        // Combine the comparisons
        return jobCountComparison != 0 ? jobCountComparison : nameComparison;
    }

    private void Sort()
    {
        if (Comparison != null)
            StationsItemList.Sort((a, b) => Comparison((NetEntity) a.Metadata!, (NetEntity) b.Metadata!));
    }

    private void UpdateUi(IReadOnlyDictionary<NetEntity, Dictionary<ProtoId<JobPrototype>, int?>> updatedJobs)
    {
        StationsItemList.Clear();

        foreach (var (key, name) in _gameTicker.StationNames)
        {
            if (Enumerable.Any<ItemList.Item>(StationsItemList, x => ((NetEntity) x.Metadata!) == key))
                continue;

            var jobsAvailable = _gameTicker.JobsAvailable[key].Values.Sum(a => a ?? 0);
            var item = new ItemList.Item(StationsItemList)
            {
                Metadata = key,
                Text = name + $" ({jobsAvailable})"
            };
            if (!string.IsNullOrEmpty(FilterLineEdit.Text) &&
                !name.ToLowerInvariant().Contains((string)FilterLineEdit.Text.Trim().ToLowerInvariant()))
            {
                continue;
            }

            StationsItemList.Add(item);
        }

        _lastJobState = updatedJobs;
        Sort();

        if (Selected == null && StationsItemList.Count > 0)
        {
            Enumerable.First(StationsItemList).Selected = true;
        }
    }
}
