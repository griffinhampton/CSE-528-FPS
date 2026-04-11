# Unused Scripts Report

Generated: 2026-04-11 00:16:20
Project: C:\Users\ghamp\CSE-528-PIH-FPS

## How to read this
- **Used (Serialized)**: Script GUID referenced by a scene/prefab/asset m_Script field.
- **Unused (Not Serialized)**: Script GUID not referenced by any m_Script in Assets/. These may still be used dynamically (e.g., AddComponent<T>(), reflection, string-based lookups).
- **Maybe Used in Code**: Not serialized, but its file base name is mentioned somewhere in C# code (heuristic).
- **Editor Scripts**: Not serialized; often only invoked by the Unity Editor and may be safe/unsafe to delete depending on tooling.

## Summary
- Total scripts: 147
- Used (Serialized): 39
- Unused (Not Serialized): 108
  - Maybe Used in Code (heuristic): 38
  - Editor Scripts: 7
  - Probably Safe To Remove (no refs found): 63

## Probably Safe To Remove
- Assets/Scripts/RealScripts(Used)/DeathScreenUI.cs
- Assets/Scripts/RealScripts(Used)/EnemyManager2.cs
- Assets/Scripts/RealScripts(Used)/ScoreTextUI.cs
- Assets/Scripts/RealScripts(Used)/ScoreTMPUI.cs
- Assets/Standard Assets/Cameras/Scripts/AutoCam.cs
- Assets/Standard Assets/Cameras/Scripts/FreeLookCam.cs
- Assets/Standard Assets/Cameras/Scripts/HandHeldCam.cs
- Assets/Standard Assets/Cameras/Scripts/ProtectCameraFromWallClip.cs
- Assets/Standard Assets/Cameras/Scripts/TargetFieldOfView.cs
- Assets/Standard Assets/Characters/FirstPersonCharacter/Scripts/FirstPersonController.cs
- Assets/Standard Assets/Characters/FirstPersonCharacter/Scripts/HeadBob.cs
- Assets/Standard Assets/Characters/RollerBall/Scripts/BallUserControl.cs
- Assets/Standard Assets/Characters/ThirdPersonCharacter/Scripts/AICharacterControl.cs
- Assets/Standard Assets/Characters/ThirdPersonCharacter/Scripts/ThirdPersonUserControl.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/AxisTouchButton.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/ButtonHandler.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/InputAxisScrollbar.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/MobileControlRig.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/TiltInput.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/TouchPad.cs
- Assets/Standard Assets/Environment/Water (Basic)/Scripts/WaterBasic.cs
- Assets/Standard Assets/Environment/Water/Water/Scripts/MeshContainer.cs
- Assets/Standard Assets/ParticleSystems/Scripts/AfterburnerPhysicsForce.cs
- Assets/Standard Assets/ParticleSystems/Scripts/ExplosionFireAndDebris.cs
- Assets/Standard Assets/ParticleSystems/Scripts/ExplosionPhysicsForce.cs
- Assets/Standard Assets/ParticleSystems/Scripts/Explosive.cs
- Assets/Standard Assets/ParticleSystems/Scripts/ExtinguishableParticleSystem.cs
- Assets/Standard Assets/ParticleSystems/Scripts/FireLight.cs
- Assets/Standard Assets/ParticleSystems/Scripts/Hose.cs
- Assets/Standard Assets/ParticleSystems/Scripts/SmokeParticles.cs
- Assets/Standard Assets/ParticleSystems/Scripts/WaterHoseParticles.cs
- Assets/Standard Assets/Utility/ActivateTrigger.cs
- Assets/Standard Assets/Utility/AlphaButtonClickMask.cs
- Assets/Standard Assets/Utility/AutoMobileShaderSwitch.cs
- Assets/Standard Assets/Utility/AutoMoveAndRotate.cs
- Assets/Standard Assets/Utility/DragRigidbody.cs
- Assets/Standard Assets/Utility/DynamicShadowSettings.cs
- Assets/Standard Assets/Utility/EventSystemChecker.cs
- Assets/Standard Assets/Utility/ForcedReset.cs
- Assets/Standard Assets/Utility/FPSCounter.cs
- Assets/Standard Assets/Utility/ParticleSystemDestroyer.cs
- Assets/Standard Assets/Utility/PlatformSpecificContent.cs
- Assets/Standard Assets/Utility/SimpleActivatorMenu.cs
- Assets/Standard Assets/Utility/SimpleMouseRotator.cs
- Assets/Standard Assets/Utility/SmoothFollow.cs
- Assets/Standard Assets/Utility/TimedObjectActivator.cs
- Assets/Standard Assets/Utility/TimedObjectDestructor.cs
- Assets/Standard Assets/Utility/WaypointProgressTracker.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark01.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark01_UGUI.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark03.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark04.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/EnvMapAnimator.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/ShaderPropAnimator.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/SkewTextExample.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/TextMeshSpawner.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_DigitValidator.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_PhoneNumberValidator.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextSelector_A.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/TMPro_InstructionOverlay.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/VertexShakeA.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/VertexShakeB.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/VertexZoom.cs

## Maybe Used Dynamically (Heuristic)
- Assets/Scripts/RealScripts(Used)/BallDeathOnHit.cs
- Assets/Scripts/RealScripts(Used)/DestroyOnLiveAndLetDieDeath.cs
- Assets/Scripts/RealScripts(Used)/EnemyPath.cs
- Assets/Scripts/RealScripts(Used)/PigArchetype.cs
- Assets/Scripts/RealScripts(Used)/PigDeathAudio.cs
- Assets/Scripts/RealScripts(Used)/PigTouchDamage.cs
- Assets/Scripts/RealScripts(Used)/PlayerFootstepAudio.cs
- Assets/Scripts/RealScripts(Used)/PlayerScore.cs
- Assets/Scripts/RealScripts(Used)/ReturnToMainMenuOnDeath.cs
- Assets/Scripts/RealScripts(Used)/UnscaledSelectableTint.cs
- Assets/Standard Assets/Cameras/Scripts/AbstractTargetFollower.cs
- Assets/Standard Assets/Cameras/Scripts/LookatTarget.cs
- Assets/Standard Assets/Cameras/Scripts/PivotBasedCameraRig.cs
- Assets/Standard Assets/Characters/FirstPersonCharacter/Scripts/MouseLook.cs
- Assets/Standard Assets/Characters/FirstPersonCharacter/Scripts/RigidbodyFirstPersonController.cs
- Assets/Standard Assets/Characters/RollerBall/Scripts/Ball.cs
- Assets/Standard Assets/Characters/ThirdPersonCharacter/Scripts/ThirdPersonCharacter.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/CrossPlatformInputManager.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/Joystick.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/PlatformSpecific/MobileInput.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/PlatformSpecific/StandaloneInput.cs
- Assets/Standard Assets/CrossPlatformInput/Scripts/VirtualInput.cs
- Assets/Standard Assets/Environment/Water/Water/Scripts/Displace.cs
- Assets/Standard Assets/Environment/Water/Water/Scripts/GerstnerDisplace.cs
- Assets/Standard Assets/Environment/Water/Water/Scripts/PlanarReflection.cs
- Assets/Standard Assets/Environment/Water/Water/Scripts/SpecularLighting.cs
- Assets/Standard Assets/Environment/Water/Water/Scripts/Water.cs
- Assets/Standard Assets/Environment/Water/Water/Scripts/WaterBase.cs
- Assets/Standard Assets/Environment/Water/Water/Scripts/WaterTile.cs
- Assets/Standard Assets/ParticleSystems/Scripts/ParticleSystemMultiplier.cs
- Assets/Standard Assets/Utility/CameraRefocus.cs
- Assets/Standard Assets/Utility/CurveControlledBob.cs
- Assets/Standard Assets/Utility/FollowTarget.cs
- Assets/Standard Assets/Utility/FOVKick.cs
- Assets/Standard Assets/Utility/LerpControlledBob.cs
- Assets/Standard Assets/Utility/ObjectResetter.cs
- Assets/Standard Assets/Utility/WaypointCircuit.cs
- Assets/TextMesh Pro/Examples & Extras/Scripts/TextMeshProFloatingText.cs

## Editor Scripts (Not Serialized)
- Assets/Standard Assets/Editor/CrossPlatformInput/CrossPlatformInputInitialize.cs
- Assets/Standard Assets/Editor/Water/Water4/GerstnerDisplaceEditor.cs
- Assets/Standard Assets/Editor/Water/Water4/PlanarReflectionEditor.cs
- Assets/Standard Assets/Editor/Water/Water4/SpecularLightingEditor.cs
- Assets/Standard Assets/Editor/Water/Water4/WaterBaseEditor.cs
- Assets/Standard Assets/Editor/Water/Water4/WaterEditorUtility.cs
- Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs

