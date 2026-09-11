using UnityEngine.PostProcessing;

namespace OttoLens;

/// Bloom lens dirt on the game camera. Vanilla only toggles bloom.enabled
/// (CameraEffects.SetBloom) and never writes the lens dirt settings, so one write per camera
/// holds until OttoLens writes again. BloomModel.settings returns a struct copy: read it,
/// change the copy, write it back.
internal static class LensDirt
{
    // Profiles are shared ScriptableObjects that outlive a camera. A second camera on the same
    // profile would read back the zero OttoLens wrote, so each profile's first value is kept.
    private static readonly Dictionary<PostProcessingProfile, float> Originals = new();

    internal static void Apply(CameraEffects? effects)
    {
        if (effects == null)
        {
            return;
        }

        PostProcessingBehaviour? behaviour = effects.GetComponent<PostProcessingBehaviour>();
        PostProcessingProfile? profile = behaviour != null ? behaviour.profile : null;
        if (profile == null || profile.bloom == null)
        {
            return;
        }

        if (!Originals.TryGetValue(profile, out float original))
        {
            original = profile.bloom.settings.lensDirt.intensity;
            Originals[profile] = original;
            LogProfile(profile);
        }

        Write(profile, OttoLensPlugin.RemoveLensDirt.Value ? 0f : original);
    }

    // TEMPORARY: one line per profile so the effects outside the vanilla graphics menu can be
    // judged from real values. Remove once the camera defaults are settled.
    private static void LogProfile(PostProcessingProfile p)
    {
        var c = System.Globalization.CultureInfo.InvariantCulture;
        GrainModel.Settings g = p.grain.settings;
        VignetteModel.Settings v = p.vignette.settings;
        EyeAdaptationModel.Settings e = p.eyeAdaptation.settings;
        ColorGradingModel.Settings cg = p.colorGrading.settings;
        OttoLensPlugin.Log.LogInfo(string.Format(c,
            "Camera profile '{0}': bloom on={1} intensity={2} lensDirt={3}; grain on={4} intensity={5} size={6} colored={7}; " +
            "vignette on={8} mode={9} intensity={10} smoothness={11} roundness={12} opacity={13}; chromaticAberration on={14} intensity={15}; " +
            "eyeAdaptation on={16} type={17} min={18} max={19} key={20} up={21} down={22}; " +
            "colorGrading on={23} tonemapper={24} exposure={25} saturation={26} contrast={27} temperature={28}; " +
            "dof on={29}; motionBlur on={30}; ao on={31}; ssr on={32}; fog on={33}; dithering on={34}; userLut on={35}",
            p.name, p.bloom.enabled, p.bloom.settings.bloom.intensity, p.bloom.settings.lensDirt.intensity,
            p.grain.enabled, g.intensity, g.size, g.colored,
            p.vignette.enabled, v.mode, v.intensity, v.smoothness, v.roundness, v.opacity,
            p.chromaticAberration.enabled, p.chromaticAberration.settings.intensity,
            p.eyeAdaptation.enabled, e.adaptationType, e.minLuminance, e.maxLuminance, e.keyValue, e.speedUp, e.speedDown,
            p.colorGrading.enabled, cg.tonemapping.tonemapper, cg.basic.postExposure, cg.basic.saturation, cg.basic.contrast, cg.basic.temperature,
            p.depthOfField.enabled, p.motionBlur.enabled, p.ambientOcclusion.enabled, p.screenSpaceReflection.enabled,
            p.fog.enabled, p.dithering.enabled, p.userLut.enabled));
    }

    /// A setting flip at runtime: the live camera, when there is one.
    internal static void ApplyCurrent() => Apply(CameraEffects.instance);

    /// Plugin unload: every touched profile gets its own value back.
    internal static void Restore()
    {
        foreach (KeyValuePair<PostProcessingProfile, float> pair in Originals)
        {
            if (pair.Key != null && pair.Key.bloom != null)
            {
                Write(pair.Key, pair.Value);
            }
        }

        Originals.Clear();
    }

    private static void Write(PostProcessingProfile profile, float intensity)
    {
        BloomModel.Settings settings = profile.bloom.settings;
        if (Mathf.Approximately(settings.lensDirt.intensity, intensity))
        {
            return;
        }

        settings.lensDirt.intensity = intensity;
        profile.bloom.settings = settings;
    }
}
