# Risk of Rain 2 Photo Mode
## This is an extremely complex and configurable mod for people looking to create cinematographic footage

## How to install
1. Go [here](https://github.com/RiskOfResources/photomode/releases) for the latest release
2. Download PhotoMode.zip
3. In the r2modman settings select "Import local mod" and choose the zip file you just downloaded.

[Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) is not required but highly recommended.

### Notes

#### Broke your config?
Inasmuch as we wanted to keep the mod settings flexible — and there have been a few emergent combinations of settings
that have been used to create interesting shots — it's possible to compose settings in a way that create undesirable effects.
If this happens just delete the `com.riskofresources.discohatesme.photomode.cfg` config file and the settings will reset to
default.

#### Time scale
We provide the ability to modify the game's time scale while in photo mode with:
* Page Up to increase the time scale by the time scale step
* Page Down to decrease the time scale by the time scale step
* Toggle between the previous time scale and time scale `0` with the Pause key

The default step is `0.1` so after entering photo mode and pressing Page Up you'll be at time scale `0.1` where everything
moves at 10% speed. Importantly: when you exit photo mode after having modified the time scale we try to keep the previous 
time scale that you set if you modified it, otherwise it goes back to `1`. This is useful for when you want to modify 
settings for a shot because you can:
1. Enter photo mode
2. Press Page Down to set the time scale to `0`
3. Set up a shot/position
4. Press `r` to set a dolly point (just a single point to save your position)
5. Exit photo mode — now the game's time scale is `0` as well
6. Open the settings and modify what you want
7. Re-enter photo mode
8. Press `p` to jump back to your previous dolly position and orientation

However, the only way to set your time scale back to normal is to go back into photo mode and use the hotkeys to set it
back to `1` or use a mod like [DebugToolkit](https://thunderstore.io/package/IHarbHD/DebugToolkit/) and execute `time_scale 1`
in the console.

### Key Features
#### Smooth (gimbal style) camera by default
Smooths out both the camera movements and rotations akin to a gimbal.
Press `g` to toggle off if you're looking to set up a static shot or dolly path.

Important Settings overview:
* Camera Sensitivity
  * This is the base mouse sensitivity for all camera movements. It *should* match your in-game sensitivity by default at `1`.
* Camera Smooth Pan Speed
   * This is the max speed the camera can move with the movement keys (WASD).
* Camera Panning Smoothing Time
  * This is the time it takes the camera to reach its max speed after pressing a movement key.
  * It's also the amount of time to come to a stop after releasing all movement keys.
  * By default, this is set to 1.5 which means there's 1.5 seconds of inertia in both directions.
* Smooth Rotation Max Speed
  * An arbitrary value that determines how quickly the smooth camera can rotate with the mouse. Setting this to 0 would prevent all mouse movements.
  * The value is quite high by default (depending on your base camera sensitivity) so you'll see effect at or around 0.
  * Set a value very close to 0 if you're looking for extremely smooth movements as that will prevent abrupt changes in rotation.
* Smooth Rotation Decay
  * How quickly the rotation slows after letting go of the mouse. Setting this to 0 means the camera will always continue rotating at the speed it was
  rotating when you let go of the mouse.

If you want a smoother camera than default try increasing the `Camera Panning Smoothing Time` and decreasing both the
`Smooth Rotation Max Speed` and `Smooth Rotation Decay`. For a snappier camera do the opposite.

#### Depth of field (blurry foreground/background)
Allows configuring:
* Focus Distance (adjustable via the mouse scroll wheel)
* Focal Length
* Aperture

By default, this uses a relatively neutral "portrait" style depth of field where the subject is in focus and only background
element rather far away are blurred. If you want a shallower depth of field you can decrease the aperture or increase the
focal length. Because this isn't a real camera there's no functional difference to changing these settings besides modifying
the depth of field.

Default Settings
![default settings](screenshots/default.webp)

Shallow DOF (aperture 2.5)
![shallow depth of field](screenshots/shallow-f-2-5.webp)

Wide DOF (aperture 8)
![wide depth of field](screenshots/wide-f-8.webp)
 
#### Dolly camera
Lets you set checkpoints for your camera that you can smoothly play back
1. Press `r` to set a starting dolly position
2. Move to another position with a rotation/roll/zoom (also focus distance if you enable dolly auto-focus in the settings)
   * Add more intermediate points with `t` and the dolly will try and transition between them
3. Hold `p` to for dolly playback
 
Dolly endpoint is implicitly set by your camera's current position so after leaving photo mode and returning you can play back the same dolly path as before
* If you make any movement with WASD it will set your camera the new dolly endpoint

#### Arc (follow) camera
Rotates towards the targeted player and follows their position as they move. Arrow keys left/right jump between different targets.

Set the `Smooth Arc Camera Speed` to 0 to just follow the target's position instead of looking towards them.
* The arc camera will also respect any inputs you make so you can use this in conjunction with the smooth camera.