# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WebRTC for Unity (`com.unity.webrtc`) is a Unity package that provides WebRTC capabilities for real-time communications. It consists of C# bindings (Unity package) and a native C++ plugin that wraps Google's libwebrtc library.

**Supported platforms:** Windows, macOS, Linux, iOS, Android
**Unity version:** 6000.0+
**libwebrtc version:** M116

## Build Commands

### Native Plugin (C++)

Build scripts are in `BuildScripts~/`. Run from repository root:

```bash
# macOS
./BuildScripts~/build_plugin_mac.sh [debug|release]

# Windows (PowerShell)
.\BuildScripts~\build_plugin_win.cmd

# Linux
./BuildScripts~/build_plugin_linux.sh

# iOS
./BuildScripts~/build_plugin_ios.sh

# Android
./BuildScripts~/build_plugin_android.sh
```

The build scripts automatically download libwebrtc from GitHub releases and use CMake presets defined in `Plugin~/CMakePresets.json`.

### Native Plugin Tests

```bash
# macOS - builds and copies test runner
./BuildScripts~/build_testrunner_mac.sh
./WebRTCLibTest  # Run tests

# Windows
.\BuildScripts~\build_testrunner_win.cmd

# Linux
./BuildScripts~/build_testrunner_linux.sh
```

### Unity Tests

Unity tests are run through the Unity Test Framework. Tests are in `Tests/Runtime/` and `Tests/Editor/`. They use NUnit and require the `UNITY_INCLUDE_TESTS` define constraint.

## Architecture

### Directory Structure

- **Runtime/** - C# Unity package code
  - `Scripts/` - Main API classes (RTCPeerConnection, MediaStreamTrack, RTCDataChannel, etc.)
  - `Plugins/` - Native plugin binaries per platform
- **Editor/** - Unity Editor tooling (WebRTC Stats profiler window, build processors)
- **Plugin~/** - Native C++ plugin source
  - `WebRTCPlugin/` - Main plugin implementation
  - `WebRTCPluginTest/` - Google Test-based native tests
- **BuildScripts~/** - Platform-specific build scripts
- **Samples~/** - Example scenes and scripts
- **Tests/** - Unity test assemblies (Runtime and Editor)

### Key C# Classes

- `WebRTC` - Static class for initialization/disposal
- `RTCPeerConnection` - Main WebRTC peer connection API
- `MediaStreamTrack` / `AudioStreamTrack` / `VideoStreamTrack` - Media tracks
- `RTCDataChannel` - Data channel API
- `Context` - Native plugin context management (handles assembly reload in Editor)

### Native Plugin Architecture

The native plugin (`Plugin~/WebRTCPlugin/`) wraps libwebrtc:
- Uses platform-specific graphics APIs (DirectX, OpenGL, Vulkan, Metal)
- Hardware encoder support via NVIDIA NvCodec (Windows/Linux) and platform encoders
- CMake build system with presets for each platform

### Assembly Definitions

- `Unity.WebRTC.Runtime` - Main runtime assembly
- `Unity.WebRTC.Editor` - Editor-only tools
- `Unity.WebRTC.RuntimeTests` - Runtime test assembly
- `Unity.WebRTC.EditorTests` - Editor test assembly

## Development Notes

- The native plugin must be built separately before Unity package changes can be tested
- Editor scripts handle `AssemblyReloadEvents` to properly dispose/reinitialize WebRTC context during script recompilation
- Plugin binaries go in `Runtime/Plugins/{platform}/`
- Ensure "Load on startup" is enabled for the native plugin in Unity Inspector to prevent crashes
