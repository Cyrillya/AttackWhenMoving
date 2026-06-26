namespace AttackWhenMoving.Config;

internal class GenericModConfigMenuIntegration
{
    public static void Init() {
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
        
        configMenu.AddSectionTitle(
            mod: modManifest,
            text: () => Localization.Config_HeaderCommonSettings()
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
        
        configMenu.AddSectionTitle(
            mod: modManifest,
            text: () => Localization.Config_HeaderControllerSettings()
        );
        
        configMenu.AddTextOption(
            mod: modManifest,
            name: () => Localization.Config_ControllerAttackMode_Name(),
            tooltip: () => Localization.Config_ControllerAttackMode_Tooltip(),
            getValue: () => config.ControllerAttackMode,
            setValue: value => config.ControllerAttackMode = value,
            allowedValues: new[] { "mouse", "direction", "edge" },
            formatAllowedValue: value => {
                return value switch {
                    "mouse" => Localization.Config_ControllerAttackMode_Mouse(),
                    "direction" => Localization.Config_ControllerAttackMode_Direction(),
                    "edge" => Localization.Config_ControllerAttackMode_Edge(),
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
            }
        );
        
        configMenu.AddNumberOption(
            mod: modManifest,
            name: () => Localization.Config_ControllerAimDeadZone_Name(),
            tooltip: () => Localization.Config_ControllerAimDeadZone_Tooltip(),
            getValue: () => config.ControllerAimDeadZone,
            setValue: value => config.ControllerAimDeadZone = value,
            min: 0f,
            max: 1f,
            interval: 0.01f,
            formatValue: value => value.ToString("P0")
        );
        
        configMenu.AddNumberOption(
            mod: modManifest,
            name: () => Localization.Config_ControllerAttackDeadZone_Name(),
            tooltip: () => Localization.Config_ControllerAttackDeadZone_Tooltip(),
            getValue: () => config.ControllerAttackDeadZone,
            setValue: value => config.ControllerAttackDeadZone = value,
            min: 0f,
            max: 1f,
            interval: 0.01f,
            formatValue: value => value.ToString("P0")
        );
    }
}