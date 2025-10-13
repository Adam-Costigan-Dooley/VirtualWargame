# Mobile Game Development - CA1 (Platform & Publication Readiness)

## Build Information
- **Unity Version:** 6.2
- **Template:** Universal 2D
- **Platform:** Android
- **Build Type:** Release (signed)
- **Backend:** IL2CPP
- **Architectures:** ARM64
- **Output:** `\MobileCA\releases\androidbuild1.apk`

## Versioning
- **Package Name:** `ie.setu.mgd.MobileCA`
- **Version Name:** `0.1.0`
- **Version Code:** `1`

## Keystore 
- **Keystore path:** `keys/CA.keystore`
- **Key alias:** `cakey`
- **Validity:** 50 years


## Verification Method
- The APK was verified using Unity’s Device Simulator on a Samsung Galaxy S10 5G and profiler.
- `CAinformation` contains images and documents showcasing the splash and main menu within the simulated phone frame +  build profile data.

## Build Steps
1. Open project in Unity 6.2.
2. Go to **File → Build Profile**
3. Platform: **Android**
6. Build to `releases\Finalkeep-0.1.3-release-arm64.apk`
7. To verify build, open Unity, set to device simulator, any android device, play

## Caveat, emulation build
## Build Information
- **Unity Version:** 6.2
- **Template:** Universal 2D
- **Platform:** Android
- **Build Type:** Release (signed)
- **Backend:** IL2CPP
- **Architectures:** 86-64
- **Output:** `releases\Finalkeep-0.1.4-release-86_64.apk`


- I do not own an Arm64 capable machine, as all emulators were unable to run arm64 emulations on my machine. Neither do i own an Android device. While the simulation ran on Unity, running it on any Emulators quickly led the emulators to crash.
- Logcat and ABD testing was done by building the application to use 86_64 on a 86_64 emulated android device.