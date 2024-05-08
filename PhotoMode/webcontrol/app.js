const testData = {
  "CameraSensitivity": {
    "Name": "Camera Sensitivity",
    "Category": {"Section": "General"},
    "DefaultValue": 1.0,
    "Description": "Sensitivity of the camera movements.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 1.0
  },
  "CameraPanSpeed": {
    "Name": "Camera Pan Speed",
    "Category": {"Section": "General"},
    "DefaultValue": 10.0,
    "Description": "Speed of the camera pan.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 10.0
  },
  "CameraRaiseSpeed": {
    "Name": "Camera Raise Speed",
    "Category": {"Section": "General"},
    "DefaultValue": 1.0,
    "Description": "Speed of the camera while raising.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 1.0
  },
  "CameraLowerSpeed": {
    "Name": "Camera Lower Speed",
    "Category": {"Section": "General"},
    "DefaultValue": 1.0,
    "Description": "Speed of the camera while lowering.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 1.0
  },
  "CameraRollSensitivity": {
    "Name": "Camera Roll Sensitivity",
    "Category": {"Section": "General"},
    "DefaultValue": 10.0,
    "Description": "Sensitivity of the camera roll.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 10.0
  },
  "CameraFovSensitivity": {
    "Name": "Camera FOV Sensitivity",
    "Category": {"Section": "General"},
    "DefaultValue": 10.0,
    "Description": "Sensitivity of the camera FOV speed.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 10.0
  },
  "CameraSprintMultiplier": {
    "Name": "Camera Sprint Multiplier",
    "Category": {"Section": "General"},
    "DefaultValue": 5.0,
    "Description": "Multiplier for how much the camera increases in speed when held.",
    "Min": 1.0,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 5.0
  },
  "CameraSlowMultiplier": {
    "Name": "Camera Slow Multiplier",
    "Category": {"Section": "General"},
    "DefaultValue": 0.3,
    "Description": "Multiplier for how much the camera decreases in speed when held.",
    "Min": 0.01,
    "Max": 1.0,
    "Increment": 0.01,
    "Value": 0.3
  },
  "SnapRollEnabled": {
    "Name": "Snap Roll Enabled",
    "Category": {"Section": "General"},
    "DefaultValue": true,
    "Description": "If the roll should snap back to 0 when close to 0 roll angle.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": true
  },
  "CameraMinFov": {
    "Name": "Camera Min FOV",
    "Category": {"Section": "General"},
    "DefaultValue": 4.0,
    "Description": "Minimum camera field of view.",
    "Min": 0.0,
    "Max": 180.0,
    "Increment": 0.01,
    "Value": 4.0
  },
  "CameraMaxFov": {
    "Name": "Camera Max FOV",
    "Category": {"Section": "General"},
    "DefaultValue": 120.0,
    "Description": "Maximum camera field of view.",
    "Min": 0.0,
    "Max": 180.0,
    "Increment": 0.01,
    "Value": 120.0
  },
  "TimeScaleStep": {
    "Name": "Time Scale Step",
    "Category": {"Section": "General"},
    "DefaultValue": 0.1,
    "Description": "Step value to increase/decrease time scale when key is pressed.",
    "Min": 0.01,
    "Max": 10.0,
    "Increment": 0.01,
    "Value": 0.1
  },
  "SmoothCamera": {
    "Name": "Smooth Camera",
    "Category": {"Section": "General"},
    "DefaultValue": false,
    "Description": "Check to smooth the free-cam.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": false
  },
  "PanningSmooth": {
    "Name": "Camera Smooth Pan Speed",
    "Category": {"Section": "General"},
    "DefaultValue": 10.0,
    "Description": "How fast the smooth pan camera can move.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 10.0
  },
  "PanningSmoothTime": {
    "Name": "Camera Panning Smoothing Time",
    "Category": {"Section": "General"},
    "DefaultValue": 1.0,
    "Description": "How many seconds to smooth the camera position while panning (inertia after releasing panning keys)",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 1.0
  },
  "MaxSmoothRotationSpeed": {
    "Name": "Smooth Rotation Max Speed",
    "Category": {"Section": "General"},
    "DefaultValue": 5.0,
    "Description": "How fast the smooth camera can rotate",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 5.0
  },
  "RotationSmoothDeadzone": {
    "Name": "Smooth Camera Rotation Deadzone",
    "Category": {"Section": "General"},
    "DefaultValue": 0.001,
    "Description": "How slowly the camera can move before stopping",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 0.001
  },
  "RotationSmoothDecay": {
    "Name": "Smooth Rotation Decay",
    "Category": {"Section": "General"},
    "DefaultValue": 0.25,
    "Description": "How much to decay the rotation speed after stopping mouse movement",
    "Min": 0.0,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 0.25
  },
  "DollyEasingFunction": {
    "Name": "Dolly Easing Function",
    "Category": {"Section": "General"},
    "DefaultValue": "Linear",
    "Description": "(Not implemented yet). How the dolly cam transitions between states. In means start slow and end fast, out means start fast and end slow.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": "Linear"
  },
  "DollyCamSpeed": {
    "Name": "Dolly Cam Speed",
    "Category": {"Section": "General"},
    "DefaultValue": 2.0,
    "Description": "Speed at which the dolly cam pans/rotates",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 2.0
  },
  "RaiseCameraKey": {
    "Name": "Raise Camera",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "E", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "E", "Modifiers": []}
  },
  "LowerCameraKey": {
    "Name": "Lower Camera",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "Q", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "Q", "Modifiers": []}
  },
  "CameraSprintKey": {
    "Name": "Speed Up Camera",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "LeftControl", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "LeftControl", "Modifiers": []}
  },
  "CameraSlowKey": {
    "Name": "Slow Down Camera",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "LeftShift", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "LeftShift", "Modifiers": []}
  },
  "ArcCameraKey": {
    "Name": "Arc Camera On Player",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "Space", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "Space", "Modifiers": []}
  },
  "ToggleRecordingKey": {
    "Name": "Toggle Recording Key",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "R", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "R", "Modifiers": []}
  },
  "DollyPlaybackKey": {
    "Name": "Dolly Playback Key",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "P", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "P", "Modifiers": []}
  },
  "DollyCheckpointKey": {
    "Name": "Add Dolly Checkpoint",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "T", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "T", "Modifiers": []}
  },
  "IncreaseTimeScaleKey": {
    "Name": "Increase Time Scale",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "PageUp", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "PageUp", "Modifiers": []}
  },
  "DecreaseTimeScaleKey": {
    "Name": "Decrease Time Scale",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "PageDown", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "PageDown", "Modifiers": []}
  },
  "ToggleTimePausedKey": {
    "Name": "Toggle Pause Key",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "Pause", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "Pause", "Modifiers": []}
  },
  "ToggleSmoothCameraKey": {
    "Name": "Toggle Smooth Camera",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "G", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "G", "Modifiers": []}
  },
  "NextPlayerKey": {
    "Name": "Arc Focus Previous Player",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "LeftArrow", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "LeftArrow", "Modifiers": []}
  },
  "PrevPlayerKey": {
    "Name": "Arc Focus Next Player",
    "Category": {"Section": "Key Bindings"},
    "DefaultValue": {"MainKey": "RightArrow", "Modifiers": []},
    "Description": "",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": {"MainKey": "RightArrow", "Modifiers": []}
  },
  "SmoothArcCamera": {
    "Name": "Smooth Arc",
    "Category": {"Section": "Arc Camera"},
    "DefaultValue": true,
    "Description": "Smoothly rotate/pan around the player instead of snapping",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": true
  },
  "ArcPanningSmoothTime": {
    "Name": "Arc Panning Smooth Time",
    "Category": {"Section": "Arc Camera"},
    "DefaultValue": 1.0,
    "Description": "How many seconds to smooth the camera position when the target of an arc camera moves",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 1.0
  },
  "SmoothArcCamSpeed": {
    "Name": "Smooth Arc Camera Speed",
    "Category": {"Section": "Arc Camera"},
    "DefaultValue": 1.0,
    "Description": "Amount of smoothing for the arc camera",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 1.0
  },
  "RestrictArcPlayers": {
    "Name": "Arc Around Players Only",
    "Category": {"Section": "Arc Camera"},
    "DefaultValue": true,
    "Description": "Whether to arc around players or any model in the scene",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": true
  },
  "PostProcessing": {
    "Name": "Enable Post Processing",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": false,
    "Description": "Enable post processing effects",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": false
  },
  "PostProcessingAntiAliasing": {
    "Name": "Anti-aliasing",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": true,
    "Description": "Enable anti-aliasing",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": true
  },
  "PostProcessDepth": {
    "Name": "Depth of Field",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": false,
    "Description": "Enable depth of field",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": false
  },
  "PostProcessFocusDistance": {
    "Name": "Focus Distance",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": 10.0,
    "Description": "Distance to the point of focus. Adjustable on the fly with the scroll wheel",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 10.0
  },
  "PostProcessFocalLength": {
    "Name": "Focal Length",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": 50.0,
    "Description": "Set the distance between the lens and the film. The larger the value is, the shallower the depth of field is.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 50.0
  },
  "PostProcessAperture": {
    "Name": "Aperture",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": 5.6,
    "Description": "Set the ratio of the aperture (known as f-stop or f-number). The smaller the value is, the shallower the depth of field is.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 5.6
  },
  "PostProcessingFocusDistanceStep": {
    "Name": "Focus Distance Step",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": 0.1,
    "Description": "Step value to increase/decrease depth of field focus distance as you scroll",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 0.1
  },
  "PostProcessVignette": {
    "Name": "Vignette",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": false,
    "Description": "Enable Vignette",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": false
  },
  "PostProcessVignetteIntensity": {
    "Name": "Vignette Intensity",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": 0.35,
    "Description": "Set the amount of vignetting on screen.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 0.35
  },
  "PostProcessVignetteSmoothness": {
    "Name": "Vignette Smoothness",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": 0.5,
    "Description": "Set the smoothness of the Vignette borders.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 0.5
  },
  "PostProcessVignetteRoundness": {
    "Name": "Vignette Roundness",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": 1.0,
    "Description": "Set the value to round the Vignette. Lower values will make a more squared vignette.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 1.0
  },
  "PostProcessVignetteRounded": {
    "Name": "Vignette Rounded",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": false,
    "Description": "Enable this checkbox to make the vignette perfectly round. When disabled, the Vignette effect is dependent on the current aspect ratio.",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": false
  },
  "BreakBeforeColorGrading": {
    "Name": "Break Before Color Grading",
    "Category": {"Section": "Post Processing"},
    "DefaultValue": false,
    "Description": "Stop applying post-process effects before color grading for exporting to external color grading",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": false
  },
  "TextFadeTime": {
    "Name": "Text Fade Out Time",
    "Category": {"Section": "UI"},
    "DefaultValue": 1.5,
    "Description": "How long to show text before fading out",
    "Min": 0.01,
    "Max": 100.0,
    "Increment": 0.01,
    "Value": 1.5
  }
};
const header = document.getElementById("header");
const res = await fetch(`/settings`);
let settings;
try {
  settings = await res.json();
  header.innerText = "Photo Mode Web Control";
}
catch(e) {
  settings = testData;
  header.innerText = "Mod not connected: Using test data";
}


class ControlBoard extends HTMLElement {
  connectedCallback() {
    if(settings != null) {
      this.#populateSettings(settings);
    }
    else {
      throw new Error("Settings unavailable?")
    }
  }

  #populateSettings() {
    /**
     * @type {HTMLTemplateElement}
     */
    const sliderTemplate = document.getElementById("slider-settings-template");
    const checkboxTemplate = document.getElementById("checkbox-settings-template");
    const entries = Object.entries(settings);

    for(const [settingName, settingValue] of entries) {
      if(typeof settingValue.Value === "number") {
        const clone = sliderTemplate.content.cloneNode(true);
        const settingElement = clone.querySelector("photo-setting");

        settingElement.setAttribute("type", typeof settingValue.Value);
        settingElement.setAttribute("control", settingValue.Name)
        settingElement.setAttribute("value", settingValue.Value);
        settingElement.setAttribute("min", settingValue.Min);
        settingElement.setAttribute("max", settingValue.Max);
        settingElement.setAttribute("step", settingValue.Increment);
        this.append(clone);
      }
      else if(typeof settingValue.Value === "boolean") {
        const clone = checkboxTemplate.content.cloneNode(true);
        const settingElement = clone.querySelector("photo-setting");
        settingElement.setAttribute("control", settingValue.Name)
        settingElement.setAttribute("value", settingValue.Value);
        settingElement.setAttribute("type", typeof settingValue.Value);

        const slider = settingElement.querySelector(".control");
        slider.value = settingValue;
        this.append(clone);
      }
    }
  }
}

class PhotoSetting extends HTMLElement {
  connectedCallback() {
    if(this.hasAttribute("control")) {
      this.#setupControl(this.getAttribute("control"));
    }
    else {
      throw new Error("Need to add control attribute to hook up the api endpoint")
    }
  }

  disconnectedCallback() {
    console.log("Custom element removed from page.");
  }

  attributeChangedCallback(name, oldValue, newValue) {
    console.log(`Attribute ${name} has changed from ${oldValue} to ${newValue}`);
  }

  /**
   * Hook up the control to the api
   *
   * @param apiEndpoint {string} the api endpoint to update
   */
  #setupControl(apiEndpoint) {
    const type = this.getAttribute("type");
    const currentValue = this.getAttribute("value");
    const controlName = this.getAttribute("control");

    const mainControl = this.querySelector(".control");

    const settingNameElement = this.querySelector(".control-name");
    settingNameElement.innerText = controlName;

    // number has 2 inputs for slider/text input based editing
    if(type === "number") {
      const settingValueElement = this.querySelector("input.control-value");
      settingValueElement.value = currentValue;

      const inputDisplay = this.querySelector(".control-value");
      mainControl.value = currentValue;
      mainControl.setAttribute("min", this.getAttribute("min"));
      mainControl.setAttribute("max", this.getAttribute("max"));
      mainControl.setAttribute("step", this.getAttribute("step"));
      inputDisplay.value = currentValue;
      inputDisplay.setAttribute("min", this.getAttribute("min"));
      inputDisplay.setAttribute("max", this.getAttribute("max"));
      inputDisplay.setAttribute("step", this.getAttribute("step"));

      inputDisplay.addEventListener("input", e => {
        mainControl.value = e.currentTarget.value;
        fetch("/" + apiEndpoint, {
          method: "POST",
          body: JSON.stringify({Value: parseFloat(e.currentTarget.value)})
        });
      });
      mainControl.addEventListener("input", e => {
        inputDisplay.value = e.currentTarget.value;
        fetch("/" + apiEndpoint, {
          method: "POST",
          body: JSON.stringify({Value: parseFloat(e.currentTarget.value)})
        });
      });
    }
    else if(type === "boolean") {
      mainControl.checked = currentValue === "true";
      mainControl.addEventListener("input", e => {
        fetch("/" + apiEndpoint, {
          method: "POST",
          body: JSON.stringify({Value: e.currentTarget.checked})
        });
      });
    }
  }
}

export class SaveButton extends HTMLButtonElement {
  connectedCallback() {
    this.onclick = () => {
      fetch("/save")
    };
  }
}

customElements.define("save-button", SaveButton, {extends: "button"});
customElements.define("control-board", ControlBoard);
customElements.define("photo-setting", PhotoSetting);

