using System.Collections;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace PhotoMode;

public class PhotoModeHud : MonoBehaviour {
   public void Init(PhotoModeSettings settings) {
      _settings = settings;
      // hud (with canvas)
      //    - image (layout group, content size fitter)
      //       - text
      //    - image (layout group, content size fitter)
      //       - text
      _hud = new GameObject("PhotoModeHUD");
      Canvas canvas = _hud.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _hud.AddComponent<CanvasScaler>().scaleFactor = (float)CanvasScaler.ScaleMode.ScaleWithScreenSize;
      
      // bottom left image
      _popupText = new GameObject("Popup Text Background Image");
      _popupText.transform.SetParent(_hud.transform);
      _popupText.SetActive(false);

      var imageLayout = _popupText.AddComponent<VerticalLayoutGroup>();
      imageLayout.padding = new RectOffset(4, 4, 4, 4);
      imageLayout.childControlWidth = true;
      imageLayout.childControlHeight = true;

      var sizeFitter = _popupText.AddComponent<ContentSizeFitter>();
      sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      var image = _popupText.AddComponent<Image>();
      image.color = new Color(0f, 0f, 0f, 0.86f);
      
      // align bottom-left corner of popup with bottom-left corner of screen
      var rectTransform = _popupText.GetComponent<RectTransform>();
      rectTransform.anchorMin = Vector2.zero;
      rectTransform.anchorMax = Vector2.zero;
      rectTransform.offsetMin = new Vector2(8, 8);
      rectTransform.offsetMax = new Vector2(8, 8);
      rectTransform.pivot = Vector2.zero;

      var textGo = new GameObject("PhotoModeHUD Popup Text");
      textGo.transform.SetParent(image.transform);
      var text = textGo.AddComponent<Text>();
      text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
      text.color = Color.white;
      text.lineSpacing = 1.2f;

      _helpText = Instantiate(_popupText, _hud.transform);
      _helpText.name = "Help Text Background Image";
      
      // align middle-right edge of help with middle-right edge of screen
      var helpRect = _helpText.GetComponent<RectTransform>();
      helpRect.anchorMin = new Vector2(1, .5f);
      helpRect.anchorMax = new Vector2(1, .5f);
      helpRect.pivot = new Vector2(1, .5f);
      helpRect.offsetMin = new Vector2(-8, 0);
      helpRect.offsetMax = new Vector2(-8, 0);
      _helpText.SetActive(false);

      if (_settings.ShowHelp.Value) {
         ToggleHelpText();
      }
   }

   public void DisplayAndFadeOutText(string message)
   {
      if (_hudTextFadeCoroutine != null) {
         StopCoroutine(_hudTextFadeCoroutine);
      }

      var time = _settings.TextFadeTime.Value;
      if (Mathf.Approximately(time, 0)) {
         return;
      }
 
      var text = _popupText.GetComponentInChildren<Text>();
      text.text = message;
      _hudTextFadeCoroutine = FadeTextToZeroAlpha(time , text);
      StartCoroutine(_hudTextFadeCoroutine);
   }
	
   private IEnumerator FadeTextToZeroAlpha(float time, Text i)
   {
      _popupText.SetActive(true);
      i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
      while (i.color.a > 0.0f)
      {
         i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.unscaledDeltaTime / time));
         yield return null;
      }

      _popupText.SetActive(false);
   }

   private void Update() {
      if (Input.GetKeyDown(_settings.DisplayHelpText.Value.MainKey)) {
         ToggleHelpText();
      }
   }

   private void ToggleHelpText()
   {
      var text = _helpText.GetComponentInChildren<Text>();

      string message = """
                       Pan Camera: WASD
                       Change Roll: M3 + Mouse X
                       Change FOV: M2 + Mouse Y
                       Change Focus Distance: Scroll Wheel
                       """;
      foreach (var setting in _settings.Settings) {
         if(setting is PhotoModeSetting<KeyboardShortcut> keySetting) {
            message += $"\n{keySetting.Name}: {keySetting.Value.MainKey.ToString()}";
         }
      }

      message += "\n\nYou can disable this showing by default in settings";
      text.text = message;
      _helpText.SetActive(!_helpText.activeInHierarchy);
   }

   private void OnDestroy() {
      Destroy(_hud);
   }

   private GameObject _popupText;
   private GameObject _helpText;
   private GameObject _hud;
   private PhotoModeSettings _settings;
   private IEnumerator _hudTextFadeCoroutine;
}
