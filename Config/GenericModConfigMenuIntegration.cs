namespace AttackWhenMoving.Config;

internal class GenericModConfigMenuIntegration
{
    public static void Init()
    {
        // 兼容Generic Mod Config Menu
        var helper = ModEntry.Instance.Helper;
        var modManifest = ModEntry.Instance.ModManifest;
        var config = ModEntry.Config;
        var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        configMenu.Register(
            mod: modManifest,
            reset: () => ModEntry.Config = new ModConfig(),
            save: () => helper.WriteConfig(ModEntry.Config)
        );

        configMenu.AddBoolOption(
            mod: modManifest,
            name: () => Localization.Config_FaceMouse_Name(),
            tooltip: () => Localization.Config_FaceMouse_Tooltip(),
            getValue: () => config.FaceMouseWhenAttack,
            setValue: value => config.FaceMouseWhenAttack = value
        );

        configMenu.AddBoolOption(
            mod: modManifest,
            name: () => Localization.Config_WeaponAutoswing_Name(),
            tooltip: () => Localization.Config_WeaponAutoswing_Tooltip(),
            getValue: () => config.WeaponAutoswing,
            setValue: value => config.WeaponAutoswing = value
        );

        configMenu.AddBoolOption(
            mod: modManifest,
            name: () => Localization.Config_EnableWeapons_Name(),
            tooltip: () => Localization.Config_EnableWeapons_Name(),
            getValue: () => config.EnableForWeapons,
            setValue: value => config.EnableForWeapons = value
        );
        configMenu.AddBoolOption(
            mod: modManifest,
            name: () => Localization.Config_EnableTools_Name(),
            tooltip: () => Localization.Config_EnableTools_Tooltip(),
            getValue: () => config.EnableForTools,
            setValue: value => config.EnableForTools = value
        );
    }
}
