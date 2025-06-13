using System;
using On.RoR2.UI;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace PhotoMode;

public class PhotoModePostProcessing : MonoBehaviour {
   private PhotoModeSettings _settings;
   private static GameObject _postProcessGameObject;
   private DepthOfField _depthOfField;
   private PostProcessVolume _quickVolume;
   private Vignette _vignette;
   private Texture3D _lutRef;
   private bool _basePostProcessingEnabled;
   private PostProcessLayer.Antialiasing _antiAliasing;
   private PostProcessLayer _postProcessLayer;

   public void Init(PhotoModeSettings settings, PostProcessLayer postProcessLayer) {
      _settings = settings;
      _postProcessLayer = postProcessLayer;
      var postProcessingEnabled = settings.PostProcessing.Value;
 
      // use global post process layer
      var layer = LayerMask.NameToLayer("PostProcess");
      layer = layer == -1 ? 20 : layer;

      if (_postProcessGameObject == null) {
         _postProcessGameObject = new GameObject("Photo Mode Post Processing") {
            name = "Photo Mode Post Processing",
            layer = layer,
            hideFlags = HideFlags.HideAndDontSave
         };
      }
 
      // vignette
      _vignette = ScriptableObject.CreateInstance<Vignette>();
      _vignette.enabled.value = postProcessingEnabled && settings.PostProcessVignette.Value;
      _vignette.color.Override(settings.PostProcessVignetteColor.Value);
      _vignette.intensity.Override(settings.PostProcessVignetteIntensity.Value);
      _vignette.smoothness.Override(settings.PostProcessVignetteSmoothness.Value);
      _vignette.roundness.Override(settings.PostProcessVignetteRoundness.Value);
      _vignette.rounded.Override(settings.PostProcessVignetteRounded.Value);
      
      // depth of field
      _depthOfField = ScriptableObject.CreateInstance<DepthOfField>();
      _depthOfField.enabled.value = postProcessingEnabled && settings.PostProcessDepth.Value;
      _depthOfField.focusDistance.Override(settings.PostProcessFocusDistance.Value);
      _depthOfField.focalLength.Override(settings.PostProcessFocalLength.Value);
      _depthOfField.aperture.Override(settings.PostProcessAperture.Value);
      var colorGrading = ScriptableObject.CreateInstance<ColorGrading>();
      colorGrading.enabled.value = settings.PostProcessColorGrading.Value;

      // import LUT if specified
      if (settings.PostProcessColorGrading.Value && _lutRef is null && !string.IsNullOrEmpty(settings.LutName.Value)) {
         var filePath = System.IO.Path.Combine(Application.dataPath, settings.LutName.Value);
         _lutRef = CubeLutImporter.ImportCubeLut(filePath);

         if (_lutRef is not null) {
            colorGrading.externalLut.Override(_lutRef);
            colorGrading.gradingMode.Override(GradingMode.External);
         }
      }

      _postProcessGameObject.TryGetComponent(out _quickVolume);
      if (!_quickVolume) {
         _quickVolume = _postProcessGameObject.AddComponent<PostProcessVolume>();
      }
 
      _quickVolume.priority = 1000;
      PostProcessProfile profile = _quickVolume.profile;
      profile.AddSettings(_depthOfField);
      _quickVolume.isGlobal = true;
      _quickVolume.weight = 1;
      _quickVolume.enabled = settings.PostProcessing.Value;
   }

   private void Update() {
      if (_settings.PostProcessing.Value) {
         _quickVolume.enabled = false;
         _depthOfField.enabled.Override(_settings.PostProcessDepth.Value);
         _depthOfField.focusDistance.Override(_settings.PostProcessFocusDistance.Value);
         _depthOfField.focalLength.Override(_settings.PostProcessFocalLength.Value);
         _depthOfField.aperture.Override(_settings.PostProcessAperture.Value);
         _vignette.enabled.Override(_settings.PostProcessVignette.Value);
         _vignette.color.Override(_settings.PostProcessVignetteColor.Value);
         _vignette.intensity.Override(_settings.PostProcessVignetteIntensity.Value);
         _vignette.smoothness.Override(_settings.PostProcessVignetteSmoothness.Value);
         _vignette.roundness.Override(_settings.PostProcessVignetteRoundness.Value);
         _vignette.rounded.Override(_settings.PostProcessVignetteRounded.Value);
         _postProcessLayer.antialiasingMode = _settings.PostProcessingAntiAliasing.Value ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing : PostProcessLayer.Antialiasing.None;
         _quickVolume.enabled = true;
      }
      else {
         _postProcessLayer.enabled = false;
      }
   }

   private void OnDestroy() {
      // don't destroy: something is holding a reference to the volumes and throws an NRE
      _quickVolume.enabled = false;
   }
}