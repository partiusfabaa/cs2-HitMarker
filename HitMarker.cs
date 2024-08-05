using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace HitMarker;

public class Config : IBasePluginConfig
{
    public string Particle { get; set; } = "particles/upkk/normalshot.vpcf";
    public int Version { get; set; } = 1;
}

public class HitMarker : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleAuthor => "thesamefabius";
    public override string ModuleName => "Hit Marker";
    public override string ModuleVersion => "v1.0.0";

    public Config Config { get; set; } = null!;

    public void OnConfigParsed(Config config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnServerPrecacheResources>(manifest => manifest.AddResource(Config.Particle));
        RegisterEventHandler<EventPlayerHurt>(EventPlayerHurt);
    }

    private HookResult EventPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        if (attacker is null) return HookResult.Continue;

        var player = @event.Userid;
        if (player == attacker || player?.Team == attacker.Team) 
            return HookResult.Continue;

        var effectName = Config.Particle;
        if (string.IsNullOrEmpty(effectName)) 
            return HookResult.Continue;

        var particle = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system");
        if (particle == null) 
            return HookResult.Continue;

        particle.EffectName = effectName;
        particle.DispatchSpawn();

        Server.NextFrame(() => particle.FireInput("Start", "!activator", attacker.PlayerPawn.Value));

        AddTimer(0.2f, () =>
        {
            if (particle.IsValid)
            {
                particle.Remove();
            }
        });

        return HookResult.Continue;
    }
}