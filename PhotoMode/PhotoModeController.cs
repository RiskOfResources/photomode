using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using R2API.Utils;
using Rewired;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace PhotoMode;

internal class PhotoModeController : MonoBehaviour {
   private PhotoModeSettings _settings;
   private LocalUser _localUser;
   private List<Transform> _players;
   private PhotoModeCameraState _cameraState;
   private static PhotoModeCameraState _startCamera;
   private static PhotoModeCameraState _endCamera;
   private static readonly List<PhotoModeCameraState> _dollyStates = [];
   private bool _recordEndPosition;

   private InputSource currentInputSource { get; set; }

   private bool gamepad => (int)currentInputSource == 1;

   public event EventHandler OnExit;

   private float _timeScale = 1f;
   private IEnumerator _dollyPlaybackCoroutine;
   private float _rollSum;
   private bool _isArcing;
   private Vector3 _smoothArcOffset = Vector3.zero;
   private Vector3 _arcSmoothPositionVelocity = Vector3.zero;
   private Vector3 _smoothPositionVelocity = Vector3.zero;
   private Vector3 _smoothRotationTarget = Vector3.zero;
   private int _selectedPlayerIndex;
   private Vector3 _arcPreviousPosition;
   private LineRenderer _lineRenderer;
   private bool _initialEnter = true;

   internal void EnterPhotoMode(PhotoModeSettings settings, CameraRigController cameraRigController) {
      _settings = settings;
      _localUser = cameraRigController.localUserViewer;
      
      // create HUD
      var photoModeHud = gameObject.AddComponent<PhotoModeHud>();
      photoModeHud.Init(settings);

      // replay buffer
      var replayBuffer = gameObject.AddComponent<ReplayBuffer>();
      replayBuffer.Init(settings);
      
      // Camera controls
      var cameraControl = gameObject.AddComponent<CameraControl>();
      cameraControl.Init(settings);

      var camera = cameraRigController.sceneCam;
      // create post-processing
      if (camera.gameObject.TryGetComponent<PostProcessLayer>(out var postProcessLayer)) {
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

      var enableDamageNumbers = SettingsConVars.enableDamageNumbers.value;
      SettingsConVars.enableDamageNumbers.SetBool(false);
      var showExpMoney = SettingsConVars.cvExpAndMoneyEffects.value;
      SettingsConVars.cvExpAndMoneyEffects.SetBool(false);
      OnExit += (_, _) => {
         SettingsConVars.enableDamageNumbers.SetBool(enableDamageNumbers);
         SettingsConVars.cvExpAndMoneyEffects.SetBool(showExpMoney);
         cameraRigController.hud.mainContainer.GetComponent<Canvas>().enabled = true;
      };

      cameraRigController.hud.mainContainer.GetComponent<Canvas>().enabled = false;
      var isCutscene = cameraRigController.isCutscene;
      cameraRigController.isCutscene = true;
      SetIndicatorsVisible(false, cameraRigController);
      OnExit += (_, _) => {
         SetIndicatorsVisible(true, cameraRigController);
         cameraRigController.isCutscene = isCutscene;
      };
 
      Player inputPlayer = cameraRigController.localUserViewer.inputPlayer;
      inputPlayer.controllers.AddLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
      OnLastActiveControllerChanged(inputPlayer, inputPlayer.controllers.GetLastActiveController());

      var globalPostProcess = cameraRigController.sceneCam.GetComponentInChildren<PostProcessVolume>();
      if (globalPostProcess) {
         globalPostProcess.enabled = !settings.BreakBeforeColorGrading.Value;
      }

      _cameraState = gameObject.AddComponent<CameraUpdater>().Init(cameraRigController, _settings);
   }

   private void OnDisable()
   {
      _localUser.inputPlayer.controllers.RemoveLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
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

   private void SetIndicatorsVisible(bool visible, CameraRigController cameraRigController)
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
      Player inputPlayer = _localUser.inputPlayer;
      if (inputPlayer.GetButton(25)) {
         Destroy(gameObject);
         return;
      }

      // hack
      if (_initialEnter) {
         _initialEnter = false;
         Time.timeScale = 0f;
      }

      float xAxis = inputPlayer.GetAxisRaw(2);
      float yAxis = inputPlayer.GetAxisRaw(3);
      var sensitivity = _settings.CameraSensitivity.Value;
      var rolling = (gamepad && inputPlayer.GetButton(10)) || Input.GetMouseButton(2);
      var zooming = (gamepad && inputPlayer.GetButton(9)) || Input.GetMouseButton(1);

      float scroll = Input.mouseScrollDelta.y;
      if (Mathf.Abs(scroll) > 0) {
         var focusDistance = Mathf.Max(0, _settings.PostProcessFocusDistance.Value + scroll * _settings.PostProcessingFocusDistanceStep.Value);
         _settings.PostProcessFocusDistance.Value = focusDistance;
         _cameraState.FocusDistance = focusDistance;
         DisplayAndFadeOutText($"Focus Distance: {_settings.PostProcessFocusDistance.Value}");
      }
	
      CheckTimeScaleChanged();

      if (Input.GetKeyDown(_settings.DollyPlaybackKey.Value.MainKey)) {
         _recordEndPosition = false;
         var dollyStates = new List<PhotoModeCameraState> { _startCamera };
         dollyStates.AddRange(_dollyStates);
         dollyStates.Add(_endCamera);
         var dollyService = new DollyService(_settings.SmoothDolly.Value, _settings.DollyCamSpeed.Value, _settings.DollyEasingFunction.Value);

         if (dollyStates.Count > 2) {
            var curve = SmoothCurve.GenerateSmoothCurve(_lineRenderer, dollyStates, (int) _settings.NumberOfDollyPoints.Value, _settings.SmoothDolly.Value);
            _dollyPlaybackCoroutine = dollyService.MultiPointDollyPlayback(curve);
         }
         else {
            _dollyPlaybackCoroutine = dollyService.DollyPlayback(dollyStates);
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
         var rollAmount = Time.unscaledDeltaTime * xAxis * _settings.CameraRollSensitivity.Value;
         _rollSum += rollAmount;
         _rollSum = Mathf.Repeat(_rollSum, 360);
         var roll = Quaternion.Euler(0, 0, -rollAmount);
         _cameraState.rotation *= roll;

         if (_settings.SnapRollEnabled.Value && _rollSum < 2 || _rollSum > 358) {
            var rollEuler = _cameraState.rotation.eulerAngles;
            _cameraState.rotation = Quaternion.Euler(rollEuler.x, rollEuler.y, 0);
         }

         DisplayAndFadeOutText($"New Rotation: {_rollSum}");
      }
      else if (zooming) {
         _cameraState.fov = Mathf.Clamp(_cameraState.fov + Time.unscaledDeltaTime * yAxis * _settings.CameraFovSensitivity.Value, _settings.CameraMinFov.Value, _settings.CameraMaxFov.Value);
         DisplayAndFadeOutText($"FOV: {_cameraState.fov}");
      }
	
      UserProfile userProfile = _localUser.userProfile;
      float mouseLookSensitivity = userProfile.mouseLookSensitivity;
      float mouseLookScaleX = userProfile.mouseLookScaleX;
      float mouseLookScaleY = userProfile.mouseLookScaleY;
      float leftValue = rolling || zooming ? 0 : mouseLookScaleX * mouseLookSensitivity * xAxis * sensitivity;
      float upValue = rolling || zooming ? 0 : mouseLookScaleY * mouseLookSensitivity * yAxis * sensitivity;
      ConditionalNegate(ref leftValue, userProfile.mouseLookInvertX);
      ConditionalNegate(ref upValue, userProfile.mouseLookInvertY);
      var up = Quaternion.AngleAxis(upValue, Vector3.left);
      var left = Quaternion.AngleAxis(leftValue, Vector3.up);
      var val = new Vector3(
         CameraControl.Instance.GetCameraSpeed(inputPlayer.GetAxis(0) * _settings.CameraPanSpeed.Value), 
         0f,
         CameraControl.Instance.GetCameraSpeed(inputPlayer.GetAxis(1) * _settings.CameraPanSpeed.Value)
      );

      if (val.magnitude > 0) {
         _recordEndPosition = true;
      }
		
      if ((gamepad && inputPlayer.GetButton(7)) || Input.GetKey(_settings.LowerCameraKey.Value.MainKey)) {
         val.y -= CameraControl.Instance.GetCameraSpeed(_settings.CameraElevationSpeed.Value);
      }
      else if ((gamepad && inputPlayer.GetButton(8)) || Input.GetKey(_settings.RaiseCameraKey.Value.MainKey)) {
         val.y += CameraControl.Instance.GetCameraSpeed(_settings.CameraElevationSpeed.Value);
      }

      var originalDirection = _cameraState.rotation;
      var withRollEuler = originalDirection.eulerAngles;
      var unrolled = left * Quaternion.Euler(withRollEuler.x, withRollEuler.y, 0) * up;
      var newPosition = unrolled * val * (Time.unscaledDeltaTime * _settings.CameraPanSpeed.Value);
      ComputeNewCameraState();

      void ComputeNewCameraState() {
         Quaternion GetSmoothRotation(float left, float up, Quaternion currentRotation) {
            var maxSpeed = _settings.MaxSmoothRotationSpeed.Value;
            var decay = Mathf.Min(_settings.RotationSmoothDecay.Value, maxSpeed);
	
            if (Mathf.Approximately(left, 0)) {
               _smoothRotationTarget.x -= Mathf.Sign(_smoothRotationTarget.x) * decay * Time.unscaledDeltaTime - left;
            }
            else {
               _smoothRotationTarget.x += left;
            }

            if (Mathf.Approximately(up, 0)) {
               _smoothRotationTarget.y -= Mathf.Sign(_smoothRotationTarget.y) * decay * Time.unscaledDeltaTime - up;
            }
            else {
               _smoothRotationTarget.y += up;
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
            return leftRot * currentRotation * upRot;
         }
 
         Quaternion GetRotation(Quaternion left, Quaternion up, Quaternion currentRotation) {
            var newRotation = left * currentRotation * up;
            var downDiff = Vector3.Angle(Vector3.down, currentRotation * Vector3.forward);
            var upDiff = Vector3.Angle(Vector3.up, currentRotation * Vector3.forward);
            var goingDown = upValue < 0;
            var goingUp = upValue > 0;

            if (goingDown && downDiff < 2 || goingUp && upDiff < 2 || Mathf.Approximately(newRotation.eulerAngles.z, 180)) {
               up = Quaternion.identity;
            }

            return left * currentRotation * up;
         }
 
         if (_settings.SmoothCamera.Value) {
            _cameraState.rotation = GetSmoothRotation(leftValue * Time.unscaledDeltaTime, upValue * Time.unscaledDeltaTime, _cameraState.rotation);
            var panningSmooth = _settings.PanningSmooth.Value;
            var panningSmoothTime = _settings.PanningSmoothTime.Value;
            var nextPosition = _cameraState.rotation * val * (Time.unscaledDeltaTime * _settings.CameraPanSpeed.Value);
            _cameraState.position = Vector3.SmoothDamp(_cameraState.position, _cameraState.position + (nextPosition * panningSmooth), ref _smoothPositionVelocity, panningSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime);
         }
         else {
            _cameraState.rotation = GetRotation(left, up, _cameraState.rotation);
            _cameraState.position += newPosition;
         }
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

      CameraUpdater.UpdateCameraState(new CameraStateUpdateMessage {
         CameraState = _cameraState
      });
   }

   private void CheckTimeScaleChanged() {
      if (Input.GetKeyDown(_settings.IncreaseTimeScaleKey.Value.MainKey)) {
         Time.timeScale += _settings.TimeScaleStep.Value;
         _timeScale = Time.timeScale;
         DisplayAndFadeOutText($"Current Time Scale: {Time.timeScale:F2}");
      }
      else if (Input.GetKeyDown(_settings.DecreaseTimeScaleKey.Value.MainKey)) {
         var newTime = Time.timeScale - _settings.TimeScaleStep.Value;
         Time.timeScale = Mathf.Max(newTime, 0);
         _timeScale = Time.timeScale;
         DisplayAndFadeOutText($"Current Time Scale: {Time.timeScale:F2}");
      }
      else if (Input.GetKeyDown(_settings.ToggleTimePausedKey.Value.MainKey)) {
         if (Time.timeScale > 0) {
            Time.timeScale = 0;
         }
         else {
            Time.timeScale = _timeScale;
         }
         DisplayAndFadeOutText($"Current Time Scale: {Time.timeScale:F2}");
      }
   }

   private void ConditionalNegate(ref float value, bool condition)
   {
      value = (condition ? (0f - value) : value);
   }

   private void DisplayAndFadeOutText(string message) {
      if (PhotoModeHud.Instance) {
         PhotoModeHud.Instance.DisplayAndFadeOutText(message);
      }
      else {
         Logger.Log($"Photo Mode HUD missing? Couldn't display message {message}");
      }
   }
}
