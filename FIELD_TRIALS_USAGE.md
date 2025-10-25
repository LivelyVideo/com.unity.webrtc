# WebRTC Field Trials Usage Guide

## Overview

Field trials are a WebRTC feature that allows you to enable or configure experimental features and behaviors. This implementation adds field trials support to Unity WebRTC.

## Important Notes

⚠️ **Field trials are process-wide and must be set BEFORE WebRTC initialization**
- Once WebRTC is initialized, field trials cannot be changed
- Field trials affect the entire WebRTC library instance
- They persist for the lifetime of the application process

## Basic Usage

### Option 1: Using the FieldTrials Helper Class

```csharp
using Unity.WebRTC;
using UnityEngine;

// This script must run BEFORE WebRTC auto-initialization
// Use [DefaultExecutionOrder(-100)] to ensure early execution
[DefaultExecutionOrder(-100)]
public class WebRTCFieldTrialsConfig : MonoBehaviour
{
    private void Awake()
    {
        // Build field trials string using the helper
        var trials = FieldTrials.Build(
            (FieldTrials.SimulcastScreenshare, "Enabled"),
            (FieldTrials.BandwidthEstimation, "Enabled")
        );

        // Initialize WebRTC with field trials
        WebRTC.InitializeInternal(
            fieldTrials: trials,
            limitTextureSize: true,
            enableNativeLog: false
        );
    }
}
```

### Option 2: Manual String Format

```csharp
// Field trials string format: key1/value1/key2/value2/
var trials = "WebRTC-SimulcastScreenshare/Enabled/WebRTC-Bwe-NetworkEstimation/Enabled/";

WebRTC.InitializeInternal(fieldTrials: trials);
```

## Available Field Trials Constants

The `FieldTrials` class provides constants for common field trials:

| Constant | WebRTC Trial Name | Description |
|----------|-------------------|-------------|
| `SimulcastScreenshare` | WebRTC-SimulcastScreenshare | Enable simulcast for screen sharing |
| `BandwidthEstimation` | WebRTC-Bwe-NetworkEstimation | Bandwidth estimation configuration |
| `FlexFec` | WebRTC-FlexFEC-03 | Flexible Forward Error Correction |
| `H264HighProfile` | WebRTC-H264HighProfile | Enable H.264 High Profile encoding |
| `VP9FlexibleMode` | WebRTC-VP9-FlexibleMode | VP9 flexible mode configuration |
| `GenericDescriptor` | WebRTC-GenericDescriptor | Generic descriptor for codec compatibility |
| `AudioNetEq` | WebRTC-Audio-NetEq | Audio NetEq for improved audio quality |
| `MinVideoBitrate` | WebRTC-Video-MinVideoBitrate | Minimum video bitrate configuration |

## String Format Validation

Field trials strings must follow this format:
- Format: `key1/value1/key2/value2/`
- Must end with a trailing slash `/`
- Must have an even number of segments (key-value pairs)
- Common values: `"Enabled"`, `"Disabled"`, or custom values

### Valid Examples
```csharp
"WebRTC-Test/Enabled/"                                    // ✓ Single entry
"WebRTC-Test1/Enabled/WebRTC-Test2/Disabled/"            // ✓ Multiple entries
""                                                        // ✓ Empty string
null                                                      // ✓ Null is valid
```

### Invalid Examples
```csharp
"WebRTC-Test/Enabled"                                     // ✗ Missing trailing slash
"WebRTC-Test/"                                            // ✗ Missing value
"WebRTC-Test1/Enabled/WebRTC-Test2/"                     // ✗ Odd number of segments
```

## Example: Complete Configuration Script

```csharp
using Unity.WebRTC;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class WebRTCConfiguration : MonoBehaviour
{
    [SerializeField]
    private bool enableSimulcast = true;

    [SerializeField]
    private bool enableFlexFec = false;

    [SerializeField]
    private bool enableNativeLogging = false;

    private void Awake()
    {
        ConfigureWebRTC();
    }

    private void ConfigureWebRTC()
    {
        // Build field trials based on configuration
        var trialsBuilder = new System.Collections.Generic.List<(string, string)>();

        if (enableSimulcast)
        {
            trialsBuilder.Add((FieldTrials.SimulcastScreenshare, "Enabled"));
        }

        if (enableFlexFec)
        {
            trialsBuilder.Add((FieldTrials.FlexFec, "Enabled"));
        }

        var trials = trialsBuilder.Count > 0
            ? FieldTrials.Build(trialsBuilder.ToArray())
            : null;

        // Initialize WebRTC with configuration
        try
        {
            WebRTC.InitializeInternal(
                fieldTrials: trials,
                limitTextureSize: true,
                enableNativeLog: enableNativeLogging,
                nativeLoggingSeverity: NativeLoggingSeverity.Info
            );

            Debug.Log($"WebRTC initialized with field trials: {trials ?? "none"}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize WebRTC: {e.Message}");
        }
    }
}
```

## Troubleshooting

### "Field trials already initialized" Warning
This occurs if you try to set field trials after WebRTC has already been initialized. Solutions:
1. Use `[DefaultExecutionOrder(-100)]` to ensure your script runs before auto-initialization
2. Only call `WebRTC.InitializeInternal()` once in your application

### "Invalid field trials format" Exception
Ensure your field trials string:
- Ends with a trailing slash `/`
- Has an even number of segments (complete key-value pairs)
- Uses the correct format: `key/value/key/value/`

### Field Trials Not Taking Effect
Remember:
- Field trials must be set BEFORE creating any WebRTC objects
- They are process-wide and cannot be changed after initialization
- Some field trials may require specific WebRTC builds or platforms

## Additional Resources

For more information about WebRTC field trials:
- [Chromium Field Trials](https://source.chromium.org/chromium/chromium/src/+/main:third_party/webrtc/g3doc/field-trials.md)
- [WebRTC Native Code Documentation](https://webrtc.googlesource.com/src/+/refs/heads/main/g3doc/)

## Implementation Details

### Native Layer (C++)
- File: `Plugin~/WebRTCPlugin/WebRTCPlugin.cpp`
- Function: `InitializeFieldTrials(const char* fieldTrials)`
- Uses: `webrtc::field_trial::InitFieldTrialsFromString()`

### Managed Layer (C#)
- File: `Runtime/Scripts/WebRTC.cs`
- Method: `WebRTC.InitializeInternal(string fieldTrials = null, ...)`
- Validation: `WebRTC.ValidateFieldTrialsFormat(string)`
- Helper: `Runtime/Scripts/FieldTrials.cs`

### Tests
- File: `Tests/Runtime/FieldTrialsTest.cs`
- Covers: Format validation, builder functionality, and edge cases
