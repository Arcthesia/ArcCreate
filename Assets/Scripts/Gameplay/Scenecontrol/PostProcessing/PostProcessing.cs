using System;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

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

        public AutoExposureController AutoExposure
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(autoExposure);
                return autoExposure;
            }
        }

        public BloomController Bloom
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(bloom);
                return bloom;
            }
        }

        public ChromaticAberrationController ChromaticAberration
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(chromaticAberration);
                return chromaticAberration;
            }
        }

        public ColorGradingController ColorGrading
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(colorGrading);
                return colorGrading;
            }
        }

        public DepthOfFieldController DepthOfField
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(depthOfField);
                return depthOfField;
            }
        }

        public GrainController Grain
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(grain);
                return grain;
            }
        }

        public LensDistortionController LensDistortion
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(lensDistortion);
                return lensDistortion;
            }
        }

        public MotionBlurController MotionBlur
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(motionBlur);
                return motionBlur;
            }
        }

        public VignetteController Vignette
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(vignette);
                return vignette;
            }
        }

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