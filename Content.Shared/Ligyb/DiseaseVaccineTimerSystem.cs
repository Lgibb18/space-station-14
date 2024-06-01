using Robust.Shared.Configuration;
namespace Content.Shared.Ligyb;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

public sealed class DiseaseVaccineTimerSystem : SharedSickSystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DiseaseVaccineTimerComponent, ComponentShutdown>(OnShut);
        SubscribeLocalEvent<DiseaseVaccineTimerComponent, ComponentInit>(OnInit);
    }
    public void OnInit(EntityUid uid, DiseaseVaccineTimerComponent component, ComponentInit args)
    {
        component.ReadyAt = _gameTiming.CurTime + component.delay;
        if(TryComp<MovementSpeedModifierComponent>(uid, out var speed))
        {
            component.SpeedBefore = speed.BaseSprintSpeed;
            _movementSpeed.ChangeBaseSpeed(uid, speed.BaseWalkSpeed, speed.BaseSprintSpeed / 2, speed.Acceleration, speed);
        }
    }
    public void OnShut(EntityUid uid, DiseaseVaccineTimerComponent component, ComponentShutdown args)
    {
        if(component.SpeedBefore != 0)
        {
            if(TryComp<MovementSpeedModifierComponent>(uid, out var speed))
            {
                _movementSpeed.ChangeBaseSpeed(uid, speed.BaseWalkSpeed, component.SpeedBefore, speed.Acceleration, speed);
            }
        }
        
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<DiseaseVaccineTimerComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_gameTiming.CurTime >= component.ReadyAt)
            {
                if (!HasComp<SickComponent>(uid))
                {
                    RemComp<DiseaseVaccineTimerComponent>(uid);
                    return;
                }

                RemComp<SickComponent>(uid);

                if (component.Immune)
                {
                    EnsureComp<DiseaseImmuneComponent>(uid);
                }
                RemComp<DiseaseVaccineTimerComponent>(uid);
            }
        }
    }

}
