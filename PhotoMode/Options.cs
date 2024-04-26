using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Events;

namespace PhotoMode;

using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;


public static class Options {
	public static readonly bool HasRiskOfOptions = Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	public static void AddOption<T>(ConfigEntry<T> entry, PhotoModeSetting<T> setting) {
		if (setting is FloatPhotoModeSetting floatSetting && entry is ConfigEntry<float> floatEntry) {
			ModSettingsManager.AddOption(new StepSliderOption(floatEntry, new StepSliderConfig() {min = floatSetting.Min, max = floatSetting.Max, increment = floatSetting.Increment} ));
			return;
		}
	
		switch (entry) {
			case ConfigEntry<bool> boolEntry:
				ModSettingsManager.AddOption(new CheckBoxOption(boolEntry));
				break;
			case ConfigEntry<KeyboardShortcut> keyEntry:
				ModSettingsManager.AddOption(new KeyBindOption(keyEntry));
				break;
			case ConfigEntry<Color> colorEntry:
				ModSettingsManager.AddOption(new ColorOption(colorEntry));
				break;
			case ConfigEntry<string> stringEntry:
				ModSettingsManager.AddOption(new StringInputFieldOption(stringEntry));
				break;
			case ConfigEntry<float> defaultFloatEntry:
				ModSettingsManager.AddOption(new StepSliderOption(defaultFloatEntry));
				break;
			case ConfigEntryBase baseEntry:
				ModSettingsManager.AddOption(new ChoiceOption(baseEntry));
				break;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	public static void AddWebUIAction(UnityAction action) {
		// ModSettingsManager.AddOption(new GenericButtonOption("Open Web UI", "Experimental", action));
	}
}