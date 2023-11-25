using nai.nai;

public record NovelAIinput(string model, NovelAIParams parameters, string input)
{
    public NovelAIinput(NovelAIEngine engine, NovelAIParams parameters, string input) : this(engine.key, parameters, input) { }
    public string action { get; set; } = "generate";
}

// V3
/*
 *
 *{
   "input": "A white-skinned girl, long white hair, red eyes, white eyelashes, a black studded crown with a bright scarlet ruby on her head, an intimidating look, Time-Lapse, Photojournalism, Wide Angle, Perspective, Double-Exposure, Light, Tones of Black in Background, Ultra-HD, Super-Resolution, Massive Scale, Perfectionism, Soft Lighting, Ray Tracing Global Illumination, Translucidluminescence, Crystalline, Lumen Reflections, in a symbolic and meaningful style --q 5 --s 4975 --chaos 15 --ar 21, best quality, amazing quality, very aesthetic, absurdres:9",
   "model": "nai-diffusion-3",
   "action": "generate",
   "parameters": {
   "width": 832,
   "height": 1216,
   "scale": 5,
   "sampler": "k_euler",
   "steps": 28,
   "seed": 4261913886,
   "n_samples": 1,
   "ucPreset": 0,
   "qualityToggle": true,
   "sm": false,
   "sm_dyn": false,
   "dynamic_thresholding": false,
   "controlnet_strength": 1,
   "legacy": false,
   "add_original_image": false,
   "uncond_scale": 1,
   "cfg_rescale": 0,
   "noise_schedule": "native",
   "negative_prompt": "nsfw, lowres, {bad}, error, fewer, extra, missing, worst quality, jpeg artifacts, bad quality, watermark, unfinished, displeasing, chromatic aberration, signature, extra digits, artistic error, username, scan, [abstract]"
   }
   }
 */


/* V2

{
   "input": "very aesthetic, best quality, absurdres, A white-skinned girl, long white hair, red eyes, white eyelashes, a black studded crown with a bright scarlet ruby on her head, an intimidating look, Time-Lapse, Photojournalism, Wide Angle, Perspective, Double-Exposure, Light, Tones of Black in Background, Ultra-HD, Super-Resolution, Massive Scale, Perfectionism, Soft Lighting, Ray Tracing Global Illumination, Translucidluminescence, Crystalline, Lumen Reflections, in a symbolic and meaningful style --q 5 --s 4975 --chaos 15 --ar 21:9",
   "model": "nai-diffusion-2",
   "action": "generate",
   "parameters": {
   "width": 832,
   "height": 1216,
   "scale": 10,
   "sampler": "k_euler_ancestral",
   "steps": 28,
   "seed": 600333141,
   "n_samples": 1,
   "ucPreset": 0,
   "qualityToggle": true,
   "sm": false,
   "sm_dyn": false,
   "dynamic_thresholding": false,
   "controlnet_strength": 1,
   "legacy": false,
   "add_original_image": false,
   "uncond_scale": 1,
   "negative_prompt": "nsfw, lowres, bad, text, error, missing, extra, fewer, cropped, jpeg artifacts, worst quality, bad quality, watermark, displeasing, unfinished, chromatic aberration, scan, scan artifacts"
   }
   }

*/


/*
 *
 *
 *{
   "input": "very aesthetic, best quality, absurdres, A white-skinned girl, long white hair, red eyes, white eyelashes, a black studded crown with a bright scarlet ruby on her head, an intimidating look, Time-Lapse, Photojournalism, Wide Angle, Perspective, Double-Exposure, Light, Tones of Black in Background, Ultra-HD, Super-Resolution, Massive Scale, Perfectionism, Soft Lighting, Ray Tracing Global Illumination, Translucidluminescence, Crystalline, Lumen Reflections, in a symbolic and meaningful style --q 5 --s 4975 --chaos 15 --ar 21:9",
   "model": "nai-diffusion-2",
   "action": "img2img",
   "parameters": {
   "width": 1280,
   "height": 1856,
   "scale": 10,
   "sampler": "k_euler_ancestral",
   "steps": 28,
   "seed": 694732333,
   "n_samples": 1,
   "strength": 0.5,
   "noise": 0,
   "ucPreset": 0,
   "qualityToggle": true,
   "sm": false,
   "sm_dyn": false,
   "dynamic_thresholding": false,
   "controlnet_strength": 1,
   "legacy": false,
   "add_original_image": false,
   "uncond_scale": 1,
    "image": "BASE64 !!!",
   "extra_noise_seed": 694732333,
   "negative_prompt": "nsfw, lowres, bad, text, error, missing, extra, fewer, cropped, jpeg artifacts, worst quality, bad quality, watermark, displeasing, unfinished, chromatic aberration, scan, scan artifacts"
   }
   }
 *
 */