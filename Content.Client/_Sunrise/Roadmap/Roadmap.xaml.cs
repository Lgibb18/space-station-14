using Content.Shared._Sunrise.Roadmap;
using Content.Shared._Sunrise.SunriseCCVars;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._Sunrise.Roadmap
{
    [GenerateTypedNameReferences]
    public sealed partial class Roadmap : Control
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;

        public Roadmap()
        {
            IoCManager.InjectDependencies(this);
            RobustXamlLoader.Load(this);

            var roadmapId = _cfg.GetCVar(SunriseCCVars.RoadmapId);
            if (!_prototype.TryIndex<RoadmapVersionsPrototype>(roadmapId, out var roadmapVersions))
                return;

            PopulateRoadmap(roadmapVersions);
        }

        private void PopulateRoadmap(RoadmapVersionsPrototype roadmapVersions)
        {
            var msg = new FormattedMessage();
            var headerLocale = Loc.GetString("ui-roadmap-header");
            msg.AddMarkupOrThrow($"[font size=24]{headerLocale} [bold]{roadmapVersions.Fork}[/bold][/font]");
            Header.SetMessage(msg);

            foreach (var roadmapVersion in roadmapVersions.Versions)
            {
                var version = new BoxContainer()
                {
                    Orientation = BoxContainer.LayoutOrientation.Vertical,
                    Margin = new Thickness(0, 0, 20, 0),
                    HorizontalExpand = true,
                };

                var versionHeader = new RoadmapVersionHeader()
                {
                    Text = roadmapVersion.Value.Name,
                };

                version.AddChild(versionHeader);

                foreach (var goal in roadmapVersion.Value.Goals)
                {
                    var roadmapItem = new RoadmapItem()
                    {
                        HeaderText = goal.Value.Name,
                        Text = goal.Value.Desc,
                        ItemState = goal.Value.State,
                    };

                    version.AddChild(roadmapItem);
                }

                Versions.AddChild(version);
            }
        }
    }
}
