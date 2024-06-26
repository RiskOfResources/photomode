using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using R2API.Utils;
using Rewired;
using RoR2;
using RoR2.UI;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace PhotoMode;

internal class PhotoModeController : MonoBehaviour, ICameraStateProvider {
   public CameraRigController cameraRigController;
   private PhotoModeSettings _settings;
   private List<Transform> _players;
   private PhotoModeCameraState _cameraState;
   private static PhotoModeCameraState _startCamera;
   private static PhotoModeCameraState _endCamera;
   private static readonly List<PhotoModeCameraState> _dollyStates = new();
   private bool _recordEndPosition;

   private Camera Camera => cameraRigController.sceneCam;

   private InputSource currentInputSource { get; set; }

   private bool gamepad => (int)currentInputSource == 1;

   public event EventHandler OnExit;

   private float _timeScale = 1f;
   private IEnumerator _dollyPlaybackCoroutine;
   private float _camSpeed;
   private float _rollSum;
   private bool _isArcing;
   private Vector3 _smoothArcOffset = Vector3.zero;
   private Vector3 _arcSmoothPositionVelocity = Vector3.zero;
   private Vector3 _smoothPositionVelocity = Vector3.zero;
   private Vector3 _smoothRotationTarget = Vector3.zero;
   private int _selectedPlayerIndex;
   private Vector3 _arcPreviousPosition;
   private LineRenderer _lineRenderer;
   private PhotoModeHud _photoModeHud;

   internal void EnterPhotoMode(PhotoModeSettings settings, CameraRigController cameraRigController) {
      _settings = settings;
      this.cameraRigController = cameraRigController;
      
      // create HUD
      _photoModeHud = gameObject.AddComponent<PhotoModeHud>();
      _photoModeHud.Init(settings);

      var replayBuffer = gameObject.AddComponent<ReplayBuffer>();
      replayBuffer.Init(settings);

      // create post-processing
      if (Camera.gameObject.TryGetComponent<PostProcessLayer>(out var postProcessLayer)) {
         gameObject.AddComponent<PhotoModePostProcessing>().Init(settings, postProcessLayer);
      }
      else {
         Logger.Log("Post-process layer not found. No post-processing enabled.");
      }
      
      var dollyPath = new GameObject("PhotoMode Dolly Path Visualizer");
      dollyPath.transform.SetParent(gameObject.transform);
      _lineRenderer = dollyPath.AddComponent<LineRenderer>();
      _lineRenderer.enabled = settings.ShowDollyPath.Value;

      var onlyPlayers = settings.RestrictArcPlayers.Value;
      if (onlyPlayers) {
         var characterModels = FindObjectsOfType<CharacterModel>();
         _players = characterModels != null ? characterModels.Select(m => m.transform).ToList() : new List<Transform>();
      }
      else {
         var modelLocators = FindObjectsOfType<ModelLocator>();
         _players = modelLocators != null ? modelLocators.Select(m => m.transform).ToList() : new List<Transform>();
      }
      Logger.Log("Entering photo mode");
      OnExit += (_, _) => {
         if (cameraRigController) {
            cameraRigController.enableFading = true;
            cameraRigController.SetOverrideCam(null);
         }

         Time.timeScale = _timeScale;
         var cameraTransform = Camera.transform;
         cameraTransform.localPosition = Vector3.zero;
         cameraTransform.localRotation = Quaternion.identity;
      };

      if (this.cameraRigController)
      {
         this.cameraRigController.SetOverrideCam(this, 0f);
         this.cameraRigController.enableFading = false;
      }

      var cameraTransform = Camera.transform;
      _cameraState.position = cameraTransform.position;
      _cameraState.rotation = Quaternion.LookRotation(cameraTransform.rotation * Vector3.forward);
      _cameraState.fov = Camera.fieldOfView;

      _timeScale = Time.timeScale;
      Time.timeScale = 0f;
      var enableDamageNumbers = SettingsConVars.enableDamageNumbers.value;
      SettingsConVars.enableDamageNumbers.SetBool(false);
      var showExpMoney = SettingsConVars.cvExpAndMoneyEffects.value;
      SettingsConVars.cvExpAndMoneyEffects.SetBool(false);
      OnExit += (_, _) => {
         SettingsConVars.enableDamageNumbers.SetBool(enableDamageNumbers);
         SettingsConVars.cvExpAndMoneyEffects.SetBool(showExpMoney);
         this.cameraRigController.hud.mainContainer.GetComponent<Canvas>().enabled = true;
      };

      this.cameraRigController.hud.mainContainer.GetComponent<Canvas>().enabled = false;

      SetIndicatorsVisible(false);
      OnExit += (_, _) => SetIndicatorsVisible(true);
 
      Player inputPlayer = this.cameraRigController.localUserViewer.inputPlayer;
      inputPlayer.controllers.AddLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
      OnLastActiveControllerChanged(inputPlayer, inputPlayer.controllers.GetLastActiveController());

      var globalPostProcess = cameraRigController.sceneCam.GetComponentInChildren<PostProcessVolume>();
      if (globalPostProcess) {
         globalPostProcess.enabled = !settings.BreakBeforeColorGrading.Value;
      }
   }

   private void OnDisable()
   {
      cameraRigController.localUserViewer.inputPlayer.controllers.RemoveLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
   }

   private void OnDestroy() {
      // save all modified settings
      foreach (var setting in _settings.Settings) {
         setting.Save();
      }

      OnExit?.Invoke(this, null);
   }

   private void OnLastActiveControllerChanged(Player player, Controller controller)
   {
      if (controller != null)
      {
         ControllerType type = controller.type;
         switch ((int)type)
         {
            case 0:
               currentInputSource = 0;
               break;
            case 1:
               currentInputSource = 0;
               break;
            case 2:
               currentInputSource = (InputSource)1;
               break;
         }
      }
   }

   private void SetIndicatorsVisible(bool visible)
   {
      Type nestedType = typeof(Indicator).GetNestedType("IndicatorManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      if (nestedType == null)
      {
         Logger.Log("Failed to find indicatorManagerTypeInfo");
      }
      else
      {
         FieldInfo field = nestedType.GetField("runningIndicators", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
         if (field == null)
         {
            Logger.Log("Failed to find runningIndicatorsFieldInfo");
         }
         else
         {
            Logger.Log(field.ToString());
            object value = field.GetValue(null);
            if (value is List<Indicator>)
            {
               ((List<Indicator>)value).ForEach(delegate(Indicator indicator)
               {
                  indicator.SetVisualizerInstantiated(visible);
               });
            }
            if (!visible)
            {
               Reflection.GetFieldValue<ParticleSystem>(DamageNumberManager.instance, "ps").Clear(true);
            }
         }
      }
      cameraRigController.hud.combatHealthBarViewer.enabled = visible;

      var list = new List<PingIndicator>(PingIndicator.instancesList);
      list.ForEach(ping => ping.gameObject.SetActive(visible));
      PingIndicator.instancesList = list;

      if (!visible)
      {
         cameraRigController.sprintingParticleSystem.Clear(true);
      }
   }

   private void Update()
   {
      UserProfile userProfile = cameraRigController.localUserViewer.userProfile;
      Player inputPlayer = cameraRigController.localUserViewer.inputPlayer;
      if (inputPlayer.GetButton(25)) {
         Destroy(gameObject);
         return;
      }

      float mouseLookSensitivity = userProfile.mouseLookSensitivity;
      float mouseLookScaleX = userProfile.mouseLookScaleX;
      float mouseLookScaleY = userProfile.mouseLookScaleY;
      float axis = inputPlayer.GetAxis(23);
      float axis2 = inputPlayer.GetAxis(24);
      float scroll = Input.mouseScrollDelta.y;
      var sensitivity = _settings.CameraSensitivity.Value;
      var rolling = (gamepad && inputPlayer.GetButton(10)) || Input.GetMouseButton(2);
      var zooming = (gamepad && inputPlayer.GetButton(9)) || Input.GetMouseButton(1);

      if (Mathf.Abs(scroll) > 0) {
         var focusDistance = Mathf.Max(0, _settings.PostProcessFocusDistance.Value + scroll * _settings.PostProcessingFocusDistanceStep.Value);
         _settings.PostProcessFocusDistance.Value = focusDistance;
         _cameraState.FocusDistance = focusDistance;
         DisplayAndFadeOutText($"Focus Distance: {_settings.PostProcessFocusDistance.Value}");
      }

      if (_settings.DisableAllMovement.Value) {
         return;
      }
	
      if (Input.GetKeyDown(_settings.IncreaseTimeScaleKey.Value.MainKey)) {
         Time.timeScale += _settings.TimeScaleStep.Value;
         _timeScale = Time.timeScale;
         DisplayAndFadeOutText($"Current Time Scale: {Time.timeScale}");
      }
      else if (Input.GetKeyDown(_settings.DecreaseTimeScaleKey.Value.MainKey)) {
         var newTime = Time.timeScale - _settings.TimeScaleStep.Value;
         Time.timeScale = Mathf.Max(newTime, 0);
         _timeScale = Time.timeScale;
         DisplayAndFadeOutText($"Current Time Scale: {Time.timeScale}");
      }
      else if (Input.GetKeyDown(_settings.ToggleTimePausedKey.Value.MainKey)) {
         if (Time.timeScale > 0) {
            Time.timeScale = 0;
         }
         else {
            Time.timeScale = _timeScale;
         }
         DisplayAndFadeOutText($"Current Time Scale: {Time.timeScale}");
      }

      if (Input.GetKeyDown(_settings.ToggleSmoothCameraKey.Value.MainKey)) {
         var newSmooth = !_settings.SmoothCamera.Value;
         _settings.SmoothCamera.Value = newSmooth;
         DisplayAndFadeOutText($"Smooth Camera: {newSmooth}");
      }

      var val = new Vector3(inputPlayer.GetAxis(0) * _settings.CameraPanSpeed.Value, 0f, inputPlayer.GetAxis(1) * _settings.CameraPanSpeed.Value);

      if (val.magnitude > 0) {
         _recordEndPosition = true;
      }
		
      if ((gamepad && inputPlayer.GetButton(7)) || Input.GetKey(_settings.LowerCameraKey.Value.MainKey)) {
         val.y -= _settings.CameraElevationSpeed.Value;
      }
      else if ((gamepad && inputPlayer.GetButton(8)) || Input.GetKey(_settings.RaiseCameraKey.Value.MainKey)) {
         val.y += _settings.CameraElevationSpeed.Value;
      }

      var sprinting = Input.GetKey(_settings.CameraSprintKey.Value.MainKey);
      var slowing = Input.GetKey(_settings.CameraSlowKey.Value.MainKey);
      var sprintMultiplier = _settings.CameraSprintMultiplier.Value;
      var slowMultiplier = _settings.CameraSlowMultiplier.Value;
      _camSpeed = _settings.DollyCamSpeed.Value;

      if (sprinting) {
         val *= sprintMultiplier;
         _camSpeed *= sprintMultiplier;
      }
      else if (slowing) {
         val *= slowMultiplier;
         _camSpeed *= slowMultiplier;
      }

      if (Input.GetKeyDown(_settings.DollyPlaybackKey.Value.MainKey)) {
         _recordEndPosition = false;
         var dollyStates = new List<PhotoModeCameraState> { _startCamera };
         dollyStates.AddRange(_dollyStates);
         dollyStates.Add(_endCamera);

         if (dollyStates.Count > 2) {
            var curve = SmoothCurve.GenerateSmoothCurve(_lineRenderer, dollyStates, (int) _settings.NumberOfDollyPoints.Value, _settings.SmoothDolly.Value);
            _dollyPlaybackCoroutine = MultiPointDollyPlayback(curve);
         }
         else {
            _dollyPlaybackCoroutine = DollyPlayback(dollyStates);
         }
         StartCoroutine(_dollyPlaybackCoroutine);
         return;
      }
      else if (Input.GetKeyUp(_settings.DollyPlaybackKey.Value.MainKey)) {
         if (_dollyPlaybackCoroutine != null) {
            StopCoroutine(_dollyPlaybackCoroutine);
            _dollyPlaybackCoroutine = null;
         }

         return;
      }
      else if (rolling) {
         var rollAmount = Time.unscaledDeltaTime * axis * _settings.CameraRollSensitivity.Value;
         _rollSum += rollAmount;
         _rollSum = Mathf.Repeat(_rollSum, 360);
         var roll = Quaternion.Euler(0, 0, -rollAmount);
         _cameraState.rotation *= roll;

         if (_settings.SnapRollEnabled.Value && _rollSum < 2 || _rollSum > 358) {
            var rollEuler = _cameraState.rotation.eulerAngles;
            _cameraState.rotation = Quaternion.Euler(rollEuler.x, rollEuler.y, 0);
         }

         Camera.transform.rotation = _cameraState.rotation;
         DisplayAndFadeOutText($"New Rotation: {_rollSum}");
      }
      else if (zooming) {
         _cameraState.fov = Mathf.Clamp(_cameraState.fov + Time.unscaledDeltaTime * axis2 * _settings.CameraFovSensitivity.Value, _settings.CameraMinFov.Value, _settings.CameraMaxFov.Value);
         DisplayAndFadeOutText($"FOV: {_cameraState.fov}");
      }
	
      float leftValue = rolling || zooming ? 0 : mouseLookScaleX * mouseLookSensitivity * Time.unscaledDeltaTime * axis * sensitivity;
      float upValue = rolling || zooming ? 0 : mouseLookScaleY * mouseLookSensitivity * Time.unscaledDeltaTime * axis2 * sensitivity;
      ConditionalNegate(ref leftValue, userProfile.mouseLookInvertX);
      ConditionalNegate(ref upValue, userProfile.mouseLookInvertY);
      var up = Quaternion.AngleAxis( upValue * 200, Vector3.left);
      var left = Quaternion.AngleAxis( leftValue * 200, Vector3.up);

      var originalDirection = _cameraState.rotation;
      var withRollEuler = originalDirection.eulerAngles;
      var unrolled = left * Quaternion.Euler(withRollEuler.x, withRollEuler.y, 0) * up;
      var newPosition = unrolled * val * (Time.unscaledDeltaTime * _settings.CameraPanSpeed.Value);
			
      if (_settings.SmoothCamera.Value) {
         var maxSpeed = _settings.MaxSmoothRotationSpeed.Value;
         var decay = Mathf.Min(_settings.RotationSmoothDecay.Value, maxSpeed);
	
         if (Mathf.Approximately(leftValue, 0)) {
            _smoothRotationTarget.x -= Mathf.Sign(_smoothRotationTarget.x) * decay * Time.unscaledDeltaTime;
         }
         else {
            _smoothRotationTarget.x += leftValue;
         }

         if (Mathf.Approximately(upValue, 0)) {
            _smoothRotationTarget.y -= Mathf.Sign(_smoothRotationTarget.y) * decay * Time.unscaledDeltaTime;
         }
         else {
            _smoothRotationTarget.y += upValue;
         }
         _smoothRotationTarget.z = 0;
         _smoothRotationTarget.x = Mathf.Clamp(_smoothRotationTarget.x, -maxSpeed, maxSpeed);
         _smoothRotationTarget.y = Mathf.Clamp(_smoothRotationTarget.y, -maxSpeed, maxSpeed);

         var deadzone = 0.001f;
         var magnitude = _smoothRotationTarget.magnitude;
         if (magnitude < deadzone) {
            _smoothRotationTarget = Vector3.zero;
         }
         
         var upRot = rolling || zooming ? Quaternion.identity : Quaternion.AngleAxis(_smoothRotationTarget.y, Vector3.left);
         var leftRot = rolling || zooming ? Quaternion.identity : Quaternion.AngleAxis( _smoothRotationTarget.x, Vector3.up);
         _cameraState.rotation = leftRot * _cameraState.rotation * upRot;
         var panningSmooth = _settings.PanningSmooth.Value;
         var panningSmoothTime = _settings.PanningSmoothTime.Value;
         var nextPosition = _cameraState.rotation * val * (Time.unscaledDeltaTime * _settings.CameraPanSpeed.Value);
         _cameraState.position = Vector3.SmoothDamp(_cameraState.position, _cameraState.position + (nextPosition * panningSmooth), ref _smoothPositionVelocity, panningSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime);
      }
      else {
         _cameraState.position += newPosition;
         var newRotation = left * _cameraState.rotation * up;
         var downDiff = Vector3.Angle(Vector3.down, _cameraState.rotation * Vector3.forward);
         var upDiff = Vector3.Angle(Vector3.up, _cameraState.rotation * Vector3.forward);
         var goingDown = upValue < 0;
         var goingUp = upValue > 0;

         if (goingDown && downDiff < 2 || goingUp && upDiff < 2 || Mathf.Approximately(newRotation.eulerAngles.z, 180)) {
            up = Quaternion.identity;
         }

         _cameraState.rotation = left * _cameraState.rotation * up;
      }
	
      var arcKeyDown = Input.GetKeyDown(_settings.ArcCameraKey.Value.MainKey);
      if (arcKeyDown && _settings.ToggleArcCamera.Value) {
         _isArcing = !_isArcing;
      }
      else if (!_settings.ToggleArcCamera.Value) {
         _isArcing = Input.GetKey(_settings.ArcCameraKey.Value.MainKey);
      }
 
      if (arcKeyDown && _players.Count > 0) {
         _arcPreviousPosition = _players[_selectedPlayerIndex].position;
         _smoothArcOffset = _cameraState.position - _players[_selectedPlayerIndex].position;
      } else if (_isArcing && _players.Count > 0) {
         Quaternion rotation;
         var player = _players[_selectedPlayerIndex];

         var smoothArc = _settings.SmoothArcCamera.Value;
         if (smoothArc) {
            var speed = _settings.SmoothArcCamSpeed.Value;
            rotation = Quaternion.RotateTowards(_cameraState.rotation, Quaternion.LookRotation(player.transform.position - _cameraState.position), Time.unscaledDeltaTime * speed * 10);
         }
         else {
            rotation = Quaternion.LookRotation(player.transform.position - _cameraState.position);
         }

         if (smoothArc) {
            var arcPanSmoothTime = _settings.ArcPanningSmoothTime.Value;
            _smoothArcOffset += newPosition;
            _cameraState.position = Vector3.SmoothDamp(_cameraState.position, player.position + _smoothArcOffset, ref _arcSmoothPositionVelocity, arcPanSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime);
         }
         else {
            var position = player.position;
            var diff = (position - _arcPreviousPosition);
            _cameraState.position += diff;
            _arcPreviousPosition = position;
         }

         _cameraState.rotation = rotation;
      }
	
      var cameraTransform = Camera.transform;
      cameraTransform.rotation = _cameraState.rotation;
      cameraTransform.position = _cameraState.position;
      Camera.fieldOfView = _cameraState.fov;

      if (_settings.DollyFollowsFocus.Value) {
         _settings.PostProcessFocusDistance.Value = _cameraState.FocusDistance;
      }
		
      if (Input.GetKeyDown(_settings.NextPlayerKey.Value.MainKey) && _players.Count > 0) {
         _selectedPlayerIndex = ((_selectedPlayerIndex + 1) % _players.Count + _players.Count) % _players.Count;
         _arcPreviousPosition = _players[_selectedPlayerIndex].position;
         DisplayAndFadeOutText($"Focused target: {_selectedPlayerIndex}");
      }
      else if (Input.GetKeyDown(_settings.PrevPlayerKey.Value.MainKey) && _players.Count > 0) {
         _selectedPlayerIndex = ((_selectedPlayerIndex - 1) % _players.Count + _players.Count) % _players.Count;
         _arcPreviousPosition = _players[_selectedPlayerIndex].position;
         DisplayAndFadeOutText($"Focused target: {_selectedPlayerIndex}");
      }

      if (Input.GetKeyDown(_settings.ToggleRecordingKey.Value.MainKey)) {
         _dollyStates.Clear();
         _startCamera = _cameraState;
         _recordEndPosition = true;
         DisplayAndFadeOutText("Set dolly start");
      }
      else if (Input.GetKeyDown(_settings.DollyCheckpointKey.Value.MainKey)) {
         _dollyStates.Add(_cameraState);
         DisplayAndFadeOutText("Added dolly checkpoint");
      }
		
      if (_recordEndPosition) {
         _endCamera = _cameraState;
      }
   }

   private IEnumerator MultiPointDollyPlayback(List<PhotoModeCameraState> positionCurve) {
      List<PhotoModeCameraState> states = positionCurve;
      var index = 0;
      var linearCamera = states[0];
      while (index < states.Count - 1) {
         var currentState = states[index];
         var nextState = states[index + 1];
         var totalDistance = Vector3.Distance(currentState.position, nextState.position);
         var cameraTransform = Camera.transform;

         // don't divide by 0
         if (Mathf.Approximately(totalDistance, 0)) {
            _cameraState = nextState;
            cameraTransform.position = _cameraState.position;
            cameraTransform.rotation = _cameraState.rotation;
            Camera.fieldOfView = _cameraState.fov;
            index++;
            continue;
         }
	
         var distance = _camSpeed * Time.unscaledDeltaTime;
         linearCamera.position = Vector3.MoveTowards(linearCamera.position, nextState.position, distance);
         var linearDistance = Vector3.Distance(linearCamera.position, nextState.position);
         var movePct = (totalDistance - linearDistance) / totalDistance;
         _cameraState = PhotoModeCameraState.Lerp(ref currentState, ref nextState, movePct);
 
         if (Mathf.Approximately(linearDistance, 0)) {
            _cameraState = nextState;
            index++;
         }

         // if we're not using a perfectly smooth dolly we can apply the easings to smooth out rotations
         if (!_settings.SmoothDolly.Value) {
            var (currentControlPoint, nextControlPoint) = currentState.ControlPoints;
            var controlPointDistance = Vector3.Distance(currentControlPoint.position, nextControlPoint.position);
            var controlPointPct = (controlPointDistance - Vector3.Distance(linearCamera.position, nextControlPoint.position)) / controlPointDistance;
            var eased = GetEasedRatio(controlPointPct, _settings.DollyEasingFunction.Value);
            eased = Mathf.Clamp01(eased);
            _cameraState.rotation = Quaternion.Slerp(currentControlPoint.rotation, nextControlPoint.rotation, eased);
         }

         cameraTransform.position = _cameraState.position;
         cameraTransform.rotation = _cameraState.rotation;
         Camera.fieldOfView = _cameraState.fov;
         yield return null;
      }
   }

   private IEnumerator DollyPlayback(List<PhotoModeCameraState> dollyStates) {
      var dollyIndex = 0;
      var linearCamera = dollyStates[0];

      while (dollyIndex < dollyStates.Count - 1) {
         var currentState = dollyStates[dollyIndex];
         var nextState = dollyStates[dollyIndex + 1];
         var totalDistance = Vector3.Distance(currentState.position, nextState.position);

         // don't divide by 0
         if (Mathf.Approximately(totalDistance, 0)) {
            _cameraState = nextState;
            yield break;
         }
			
         var distance = _camSpeed * Time.unscaledDeltaTime;
         linearCamera.position = Vector3.MoveTowards(linearCamera.position, nextState.position, distance);
         var linearDistance = Vector3.Distance(linearCamera.position, nextState.position);
         var movePct = (totalDistance - linearDistance) / totalDistance;
         var eased = GetEasedRatio(movePct, _settings.DollyEasingFunction.Value);
         eased = Mathf.Clamp01(eased);
         _cameraState = PhotoModeCameraState.Lerp(ref currentState, ref nextState, eased);
         if (Mathf.Approximately(linearDistance, 0)) {
            _cameraState = nextState;
            dollyIndex++;
         }

         var cameraTransform = Camera.transform;
         cameraTransform.position = _cameraState.position;
         cameraTransform.rotation = _cameraState.rotation;
         Camera.fieldOfView = _cameraState.fov;
         yield return null;
      }
   }

   private float GetEasedRatio(float x, Easing easing) {
      switch (easing) {
         case Easing.Linear:
            return x;
         case Easing.SineIn:
            return 1 - (float)Math.Cos(x * Math.PI / 2);
         case Easing.EaseInOutSine:
            return (float)(-(Math.Cos(Math.PI * x) - 1) / 2);
         case Easing.EaseInOutCubic:
            return (float)(x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2);
         case Easing.EaseInOutQuad:
            return (float)(x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2);
         case Easing.SineOut:
            return (float)Math.Sin(x * Math.PI / 2);
         case Easing.QuadIn:
            return x * x;
         case Easing.QuadOut:
            return 1 - (1 - x) * (1 - x);
         case Easing.CubicIn:
            return x * x * x;
         case Easing.CubicOut:
            return 1 - (1 - x) * (1 - x) * (1 - x);
         case Easing.Reverse:
            return 1 - x;
         default:
            Logger.Log("Invalid easing set, falling back to linear");
            return x;
      }
   }

   public void GetCameraState(CameraRigController _, ref CameraState cameraState)
   {
   }

   private void ConditionalNegate(ref float value, bool condition)
   {
      value = (condition ? (0f - value) : value);
   }

   public bool IsHudAllowed(CameraRigController _)
   {
      return false;
   }

   public bool IsUserControlAllowed(CameraRigController _)
   {
      return false;
   }

   public bool IsUserLookAllowed(CameraRigController _)
   {
      return false;
   }
	
   private void DisplayAndFadeOutText(string message)
   {
      if (_photoModeHud) {
         _photoModeHud.DisplayAndFadeOutText(message);
      }
      else {
         Debug.Log($"Photo Mode HUD missing? Couldn't display message {message}");
      }
   }
}
