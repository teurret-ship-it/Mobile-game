# Building the Android APK without installing Unity

You can produce an installable test APK entirely in the cloud using **GitHub Actions +
GameCI**. Unity runs inside a container on GitHub's servers — you never install Unity
locally. The only one-time setup is a free Unity license, because every Unity build
(local or cloud) must be licensed.

The pipeline:

1. `0. Get Unity Activation File` workflow → gives you a `.alf` file.
2. Convert `.alf` → `.ulf` on Unity's website (free).
3. Save the `.ulf` contents as the `UNITY_LICENSE` repository secret.
4. `Android Build (APK)` workflow runs → download the `ArcaneSurvivors-APK` artifact → install on your phone.

---

## Step 1 — Get the activation file (.alf)

1. Make sure you have a free Unity account: <https://id.unity.com> (Personal license is fine).
2. In GitHub: **Actions** tab → **0. Get Unity Activation File** → **Run workflow**.
3. When it finishes, open the run → **Artifacts** → download **Unity_Activation_File**.
   Unzip it; inside is a file like `Unity_v2022.x.alf`.

## Step 2 — Convert .alf → .ulf

1. Go to <https://license.unity3d.com/manual>.
2. Upload the `.alf` file.
3. Choose **Unity Personal** (free) → for personal/hobby use.
4. Download the resulting `Unity_v2022.x.ulf` file.

## Step 3 — Add the license secret

1. In GitHub: **Settings → Secrets and variables → Actions → New repository secret**.
2. Name: `UNITY_LICENSE`
3. Value: paste the **entire contents** of the `.ulf` file (open it in a text editor; it's XML).
4. Save.

## Step 4 — Build the APK

The build runs automatically on every push to `claude/wykonaj-4vkjg4`, or run it manually:

1. **Actions** tab → **Android Build (APK)** → **Run workflow**.
2. Wait for it to finish (first run ~15–25 min; later runs are cached and faster).
3. Open the run → **Artifacts** → download **ArcaneSurvivors-APK** → unzip → `ArcaneSurvivors.apk`.

## Step 5 — Install on your phone

1. Copy the `.apk` to your Android device (USB, Drive, etc.).
2. Allow "install from unknown sources" for your file manager/browser if prompted.
3. Tap the APK to install, then launch **Arcane Survivors**.

> The APK is a **development build** signed with Android's debug keystore — perfect for
> testing/sideloading, not for the Play Store. For a store release you'd switch to IL2CPP +
> ARM64 and a real keystore (see `Assets/Editor/AndroidBuilder.cs`).

---

## Build configuration (what the CI produces)

Set in `Assets/Editor/AndroidBuilder.cs`:

| Setting | Value | Why |
|---------|-------|-----|
| Scripting backend | Mono | Fast CI builds, no NDK step |
| Architecture | ARMv7 | Broad device compatibility for a test APK |
| Min SDK | Android 7.0 (API 24) | Reasonable floor |
| Orientation | Landscape | Matches the HUD layout |
| Signing | Debug keystore (development build) | Installable without secrets |
| Render pipeline | Built-in (Standard shader) | Avoids URP setup; placeholder art renders correctly |

To change the Unity version, edit `unityVersion` in both workflow files **and**
`ProjectSettings/ProjectVersion.txt` so they match a published GameCI editor image.

## Troubleshooting

- **"No valid Unity license" / activation error** → the `UNITY_LICENSE` secret is missing or
  malformed. Re-do Steps 1–3; make sure you pasted the whole `.ulf` XML.
- **Editor image not found for version** → the `unityVersion` has no GameCI image; pick a
  nearby `2022.3.x` patch and update both workflows + `ProjectVersion.txt`.
- **Compile errors in CI logs** → share the log; these are fixable in the scripts.
- **APK won't install** → uninstall any previous build first (signature mismatch), and
  confirm your device allows sideloading.
