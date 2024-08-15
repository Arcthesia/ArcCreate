using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmySingleton]
    public class PostProcessing : MonoBehaviour
    {
        [SerializeField] private AutoExposureController autoExposure;
        [SerializeField] private BloomController bloom;
        [SerializeField] private ChromaticAberrationController chromaticAberration;
        [SerializeField] private ColorGradingController colorGrading;
        [SerializeField] private DepthOfFieldController depthOfField;
        [SerializeField] private GrainController grain;
        [SerializeField] private LensDistortionController lensDistortion;
        [SerializeField] private MotionBlurController motionBlur;
        [SerializeField] private VignetteController vignette;

        [SerializeField] private PostProcessLayer layer;
        [SerializeField] private PostProcessVolume volume;

        public AutoExposureController AutoExposure
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(autoExposure);
                EnablePostProcess();
                return autoExposure;
            }
        }

        public BloomController Bloom
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(bloom);
                EnablePostProcess();
                return bloom;
            }
        }

        public ChromaticAberrationController ChromaticAberration
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(chromaticAberration);
                EnablePostProcess();
                return chromaticAberration;
            }
        }

        public ColorGradingController ColorGrading
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(colorGrading);
                EnablePostProcess();
                return colorGrading;
            }
        }

        public DepthOfFieldController DepthOfField
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(depthOfField);
                EnablePostProcess();
                return depthOfField;
            }
        }

        public GrainController Grain
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(grain);
                EnablePostProcess();
                return grain;
            }
        }

        public LensDistortionController LensDistortion
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(lensDistortion);
                EnablePostProcess();
                return lensDistortion;
            }
        }

        public MotionBlurController MotionBlur
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(motionBlur);
                EnablePostProcess();
                return motionBlur;
            }
        }

        public VignetteController Vignette
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(vignette);
                EnablePostProcess();
                return vignette;
            }
        }

        [MoonSharpHidden]
        public ISceneController CreateFromTypeName(string type)
        {
            switch (type)
            {
                case "autoExposure":
                    return AutoExposure;
                case "bloom":
                    return Bloom;
                case "chromaticAberration":
                    return ChromaticAberration;
                case "colorGrading":
                    return ColorGrading;
                case "depthOfField":
                    return DepthOfField;
                case "grain":
                    return Grain;
                case "lensDistortion":
                    return LensDistortion;
                case "motionBlur":
                    return MotionBlur;
                case "vignette":
                    return Vignette;
                default:
                    return null;
            }
        }

        [MoonSharpHidden]
        public void EnablePostProcess()
        {
            layer.enabled = true && !Settings.DisableAdvancedGraphics.Value;
            volume.enabled = true && !Settings.DisableAdvancedGraphics.Value;
        }

        [MoonSharpHidden]
        public void DisablePostProcess()
        {
            layer.enabled = false;
            volume.enabled = false;
        }

        private void Awake()
        {
            autoExposure.SerializedType = "autoExposure";
            bloom.SerializedType = "bloom";
            chromaticAberration.SerializedType = "chromaticAberration";
            colorGrading.SerializedType = "colorGrading";
            depthOfField.SerializedType = "depthOfField";
            grain.SerializedType = "grain";
            lensDistortion.SerializedType = "lensDistortion";
            motionBlur.SerializedType = "motionBlur";
            vignette.SerializedType = "vignette";
        }
    }
}