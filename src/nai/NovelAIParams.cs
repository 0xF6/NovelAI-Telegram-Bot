﻿using nai;
using Newtonsoft.Json;

public record NovelAIParams
{
    private NovelAIParams(){}

    public int width { get; set; } = 512;
    public int height { get; set; } = 768;
    public int scale { get; set; } = 11;
    public string sampler { get; set; }
    public int steps { get; set; } = 28;
    public long seed { get; set; } = -1;
    public int n_samples { get; set; } = 1;
    public int ucPreset { get; set; } = 0;
    public bool qualityToggle { get; set; } = true;
    public bool sm { get; set; } = false;
    public bool sm_dyn { get; set; } = false;
    public bool dynamic_thresholding { get; set; } = false;
    public int controlnet_strength { get; set; }
    public bool legacy { get; set; } = false;
    public bool add_original_image { get; set; }
    public string negative_prompt { get; set; }





    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string image { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float? strength { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float? noise { get; set; }


    public static NovelAIParams Create(NaiSettings settings, long seed)
    {
        return new NovelAIParams()
        {
            steps = settings.defaultStep,
            add_original_image = false,
            controlnet_strength = settings.controlnet_strength,
            dynamic_thresholding = settings.dynamic_thresholding,
            n_samples = 1,
            negative_prompt = settings.Engine.isActual
                ? settings.DefaultNegateTags.Actual
                : settings.DefaultNegateTags.Legacy,
            qualityToggle = settings.qualityToggle,
            sampler = settings.SelectedSampler,
            scale = settings.Guidance,
            sm = settings.SMEA,
            sm_dyn = settings.DYN,
            ucPreset = settings.ucPreset,
            legacy = false,
            seed = seed
        };
    }
}