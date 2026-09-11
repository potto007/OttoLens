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
        }

        Write(profile, OttoLensPlugin.RemoveLensDirt.Value ? 0f : original);
    }

    // Measured on 1.0.7, profile 'ingame': lens dirt 10.4 (Unity's default is 3). Grain,
    // vignette, eye adaptation, SSR and dithering are already off in the profile. Chromatic
    // aberration and motion blur follow the vanilla graphics menu, and colour grading (ACES,
    // contrast 1.2, temperature -8) is Iron Gate's look, so lens dirt is the one hidden effect.

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
