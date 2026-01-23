using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    ///     Specifies the reason why video quality is being limited.
    /// </summary>
    public enum RTCQualityLimitationReason
    {
        /// <summary>
        ///     Quality is not being limited.
        /// </summary>
        None,

        /// <summary>
        ///     Quality is limited due to CPU constraints.
        /// </summary>
        Cpu,

        /// <summary>
        ///     Quality is limited due to bandwidth constraints.
        /// </summary>
        Bandwidth,

        /// <summary>
        ///     Quality is limited for other reasons.
        /// </summary>
        Other
    }

    /// <summary>
    ///     Contains information about the current video adaptation state.
    /// </summary>
    /// <remarks>
    ///     This class provides details about how the video encoder is adapting to
    ///     network conditions, including current restrictions on resolution and framerate,
    ///     and the reason for any quality limitations.
    /// </remarks>
    public class RTCVideoAdaptationState
    {
        /// <summary>
        ///     The current width of the encoded video frames in pixels.
        /// </summary>
        public uint FrameWidth { get; internal set; }

        /// <summary>
        ///     The current height of the encoded video frames in pixels.
        /// </summary>
        public uint FrameHeight { get; internal set; }

        /// <summary>
        ///     The current frames per second being encoded.
        /// </summary>
        public double FramesPerSecond { get; internal set; }

        /// <summary>
        ///     The reason why video quality is currently being limited, if any.
        /// </summary>
        public RTCQualityLimitationReason QualityLimitationReason { get; internal set; }

        /// <summary>
        ///     The number of times the resolution has changed due to quality limitations.
        /// </summary>
        public uint QualityLimitationResolutionChanges { get; internal set; }

        /// <summary>
        ///     Duration in seconds spent in each quality limitation state.
        /// </summary>
        /// <remarks>
        ///     Keys are "none", "cpu", "bandwidth", and "other".
        /// </remarks>
        public Dictionary<string, double> QualityLimitationDurations { get; internal set; }

        /// <summary>
        ///     The name of the encoder implementation being used.
        /// </summary>
        public string EncoderImplementation { get; internal set; }

        /// <summary>
        ///     Whether the encoder is using a power-efficient (hardware) encoder.
        /// </summary>
        public bool PowerEfficientEncoder { get; internal set; }

        /// <summary>
        ///     Creates an RTCVideoAdaptationState by parsing the quality limitation reason string.
        /// </summary>
        internal static RTCQualityLimitationReason ParseQualityLimitationReason(string reason)
        {
            if (string.IsNullOrEmpty(reason))
                return RTCQualityLimitationReason.None;

            switch (reason.ToLowerInvariant())
            {
                case "none":
                    return RTCQualityLimitationReason.None;
                case "cpu":
                    return RTCQualityLimitationReason.Cpu;
                case "bandwidth":
                    return RTCQualityLimitationReason.Bandwidth;
                default:
                    return RTCQualityLimitationReason.Other;
            }
        }

        /// <summary>
        ///     Returns a string representation of the adaptation state.
        /// </summary>
        public override string ToString()
        {
            return $"[AdaptationState: {FrameWidth}x{FrameHeight}@{FramesPerSecond:F1}fps, " +
                   $"Limitation={QualityLimitationReason}, Changes={QualityLimitationResolutionChanges}]";
        }
    }

    /// <summary>
    ///     Event arguments for adaptation change events.
    /// </summary>
    /// <remarks>
    ///     Contains the previous and current adaptation state, allowing the application
    ///     to understand how the video encoder's adaptation has changed.
    /// </remarks>
    public class AdaptationChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     The previous adaptation state before the change.
        /// </summary>
        public RTCVideoAdaptationState PreviousState { get; }

        /// <summary>
        ///     The current adaptation state after the change.
        /// </summary>
        public RTCVideoAdaptationState CurrentState { get; }

        /// <summary>
        ///     Creates a new AdaptationChangedEventArgs instance.
        /// </summary>
        /// <param name="previousState">The previous adaptation state.</param>
        /// <param name="currentState">The current adaptation state.</param>
        public AdaptationChangedEventArgs(RTCVideoAdaptationState previousState, RTCVideoAdaptationState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    /// <summary>
    ///     Provides the ability to control and access to details on encoding and sending a MediaStreamTrack to a remote peer.
    /// </summary>
    /// <remarks>
    ///     `RTCRtpSender` class allows customization of media encoding and transmission to a remote peer.
    ///     It provides access to the device's media capabilities and supports sending DTMF tones for telephony interactions.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         var senders = peerConnection.GetSenders();
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCPeerConnection" />
    public class RTCRtpSender : RefCountedObject
    {
        private RTCPeerConnection peer;
        private RTCRtpTransform transform;
        private RTCVideoAdaptationState lastAdaptationState;

        /// <summary>
        ///     Event raised when the video adaptation state changes.
        /// </summary>
        /// <remarks>
        ///     This event is raised when the video encoder's adaptation state changes,
        ///     such as when resolution or framerate is adjusted due to bandwidth or CPU constraints.
        ///
        ///     Note: This event must be triggered by calling <see cref="CheckAdaptationState"/> with
        ///     the latest stats. The event is not automatically raised by the native WebRTC library.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         sender.OnAdaptationChanged += (sender, args) =>
        ///         {
        ///             Debug.Log($"Adaptation changed: {args.CurrentState}");
        ///         };
        ///     ]]></code>
        /// </example>
        public event EventHandler<AdaptationChangedEventArgs> OnAdaptationChanged;

        internal RTCRtpSender(IntPtr ptr, RTCPeerConnection peer) : base(ptr)
        {
            WebRTC.Table.Add(self, this);
            this.peer = peer;
        }

        /// <summary>
        ///     Finalizer for RTCRtpSender.
        /// </summary>
        /// <remarks>
        ///     Ensures that resources are released by calling the `Dispose` method
        /// </remarks>
        ~RTCRtpSender()
        {
            this.Dispose();
        }

        /// <summary>
        ///     Disposes of RTCRtpSender.
        /// </summary>
        /// <remarks>
        ///     `Dispose` method disposes of the `RTCRtpSender` and releases the associated resources. 
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         sender.Dispose();
        ///     ]]></code>
        /// </example>
        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }

        /// <summary>
        ///     Provides a `RTCRtpCapabilities` object describing the codec and header extension capabilities.
        /// </summary>
        /// <remarks>
        ///     `GetCapabilities` method provides a `RTCRtpCapabilities` object that describes the codec and header extension capabilities supported by `RTCRtpSender`.
        /// </remarks>
        /// <param name="kind">`TrackKind` value indicating the type of media.</param>
        /// <returns>`RTCRtpCapabilities` object contains an array of `RTCRtpCodecCapability` objects.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpCapabilities capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
        ///         RTCRtpTransceiver transceiver = peerConnection.GetTransceivers().First();
        ///         RTCErrorType error = transceiver.SetCodecPreferences(capabilities.codecs);
        ///         if (error.errorType != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to set codec preferences: {error.message}");
        ///         }
        ///     ]]></code>
        /// </example>
        public static RTCRtpCapabilities GetCapabilities(TrackKind kind)
        {
            WebRTC.Context.GetSenderCapabilities(kind, out IntPtr ptr);
            RTCRtpCapabilitiesInternal capabilitiesInternal =
                Marshal.PtrToStructure<RTCRtpCapabilitiesInternal>(ptr);
            RTCRtpCapabilities capabilities = new RTCRtpCapabilities(capabilitiesInternal);
            Marshal.FreeHGlobal(ptr);
            return capabilities;
        }

        /// <summary>
        ///     Asynchronously requests statistics about outgoing traffic on the RTCPeerConnection associated with the RTCRtpSender.
        /// </summary>
        /// <remarks>
        ///     `GetStats` method asynchronously requests an `RTCStatsReport` containing statistics about the outgoing traffic for the `RTCPeerConnection` associated with the `RTCRtpSender`.
        /// </remarks>
        /// <returns>`RTCStatsReportAsyncOperation` object containing `RTCStatsReport` object.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCStatsReportAsyncOperation asyncOperation = sender.GetStats();
        ///         yield return asyncOperation;
        ///         
        ///         if (!asyncOperation.IsError)
        ///         {
        ///             RTCStatsReport statsReport = asyncOperation.Value;
        ///             RTCStats stats = statsReport.Stats.ElementAt(0).Value;
        ///             string statsText = "Id:" + stats.Id + "\n";
        ///             statsText += "Timestamp:" + stats.Timestamp + "\n";
        ///             statsText += stats.Dict.Aggregate(string.Empty, (str, next) =>
        ///                 str + next.Key + ":" + (next.Value == null ? string.Empty : next.Value.ToString()) + "\n");
        ///             Debug.Log(statsText);
        ///             statsReport.Dispose();
        ///         }
        ///     ]]></code>
        /// </example>
        public RTCStatsReportAsyncOperation GetStats()
        {
            return peer.GetStats(this);
        }

        /// <summary>
        ///      <see cref="MediaStreamTrack"/> managed by RTCRtpSender. If it is null, no transmission occurs.
        /// </summary>
        public MediaStreamTrack Track
        {
            get
            {
                IntPtr ptr = NativeMethods.SenderGetTrack(GetSelfOrThrow());
                if (ptr == IntPtr.Zero)
                    return null;
                return WebRTC.FindOrCreate(ptr, MediaStreamTrack.Create);
            }
        }

        /// <summary>
        ///     <see cref="RTCRtpScriptTransform"/> used to insert a transform stream in a worker thread into the sender pipeline,
        ///     enabling transformations on encoded video and audio frames after output by a codec but before transmission.
        /// </summary>
        public RTCRtpTransform Transform
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // cache reference
                transform = value;
                NativeMethods.SenderSetTransform(GetSelfOrThrow(), value.self);
            }
            get
            {
                return transform;
            }
        }

        /// <summary>
        ///     Indicates if the video track's framerate is synchronized with the application's framerate.
        /// </summary>
        public bool SyncApplicationFramerate
        {
            get
            {
                if (Track is VideoStreamTrack videoTrack)
                {
                    if (videoTrack.m_source == null)
                    {
                        throw new InvalidOperationException("This track doesn't have a video source.");
                    }
                    return videoTrack.m_source.SyncApplicationFramerate;
                }
                else
                {
                    throw new InvalidOperationException("This track is not VideoStreamTrack.");
                }
            }
            set
            {
                if (Track is VideoStreamTrack videoTrack)
                {
                    if (videoTrack.m_source == null)
                    {
                        throw new InvalidOperationException("This track doesn't have a video source.");
                    }
                    videoTrack.m_source.SyncApplicationFramerate = value;
                }
                else
                {
                    throw new InvalidOperationException("This track is not VideoStreamTrack.");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the degradation preference for video encoding when bandwidth is constrained.
        /// </summary>
        /// <remarks>
        ///     When bandwidth is constrained and the encoder needs to choose between degrading resolution
        ///     or degrading framerate, this property indicates which is preferred. Only applicable for video tracks.
        ///
        ///     Setting this property directly controls libwebrtc's native adaptation behavior:
        ///     - MaintainFramerateAndResolution: Disable native adaptation
        ///     - MaintainFramerate: Reduce resolution before framerate
        ///     - MaintainResolution: Reduce framerate before resolution
        ///     - Balanced: Let libwebrtc balance both
        ///
        ///     Returns null if no preference has been set.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpSender sender = peerConnection.GetSenders().First();
        ///         // Prioritize smooth motion over resolution
        ///         RTCError error = sender.SetDegradationPreference(RTCDegradationPreference.MaintainFramerate);
        ///         if (error.errorType != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to set degradation preference: {error.message}");
        ///         }
        ///     ]]></code>
        /// </example>
        public RTCDegradationPreference? DegradationPreference
        {
            get
            {
                int value = NativeMethods.SenderGetDegradationPreference(GetSelfOrThrow());
                if (value < 0)
                {
                    return null;
                }
                return (RTCDegradationPreference)value;
            }
        }

        /// <summary>
        ///     Sets the degradation preference for video encoding when bandwidth is constrained.
        /// </summary>
        /// <param name="preference">
        ///     The degradation preference to set, or null to clear the preference.
        /// </param>
        /// <returns>An RTCError indicating success or failure.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpSender sender = peerConnection.GetSenders().First();
        ///         RTCError error = sender.SetDegradationPreference(RTCDegradationPreference.MaintainFramerate);
        ///     ]]></code>
        /// </example>
        public RTCError SetDegradationPreference(RTCDegradationPreference? preference)
        {
            int value = preference.HasValue ? (int)preference.Value : -1;
            RTCErrorType type = NativeMethods.SenderSetDegradationPreference(GetSelfOrThrow(), value);
            return new RTCError { errorType = type };
        }

        /// <summary>
        ///     Extracts the current video adaptation state from outbound RTP stream stats.
        /// </summary>
        /// <param name="stats">The RTCOutboundRTPStreamStats to extract adaptation state from.</param>
        /// <returns>An RTCVideoAdaptationState object with the current adaptation info.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         var statsOp = sender.GetStats();
        ///         yield return statsOp;
        ///         foreach (var stat in statsOp.Value.Stats.Values)
        ///         {
        ///             if (stat is RTCOutboundRTPStreamStats outbound)
        ///             {
        ///                 var state = RTCRtpSender.GetAdaptationStateFromStats(outbound);
        ///                 Debug.Log(state);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public static RTCVideoAdaptationState GetAdaptationStateFromStats(RTCOutboundRTPStreamStats stats)
        {
            if (stats == null)
                throw new ArgumentNullException(nameof(stats));

            return new RTCVideoAdaptationState
            {
                FrameWidth = stats.frameWidth,
                FrameHeight = stats.frameHeight,
                FramesPerSecond = stats.framesPerSecond,
                QualityLimitationReason = RTCVideoAdaptationState.ParseQualityLimitationReason(stats.qualityLimitationReason),
                QualityLimitationResolutionChanges = stats.qualityLimitationResolutionChanges,
                QualityLimitationDurations = stats.qualityLimitationDurations,
                EncoderImplementation = stats.encoderImplementation,
                PowerEfficientEncoder = stats.powerEfficientEncoder
            };
        }

        /// <summary>
        ///     Checks if the adaptation state has changed and raises the OnAdaptationChanged event if so.
        /// </summary>
        /// <param name="stats">The RTCOutboundRTPStreamStats to check for adaptation changes.</param>
        /// <returns>True if an adaptation change was detected and the event was raised.</returns>
        /// <remarks>
        ///     Call this method periodically with fresh stats to detect adaptation changes.
        ///     The method compares the current stats against the last known state and raises
        ///     the OnAdaptationChanged event if there are significant changes to resolution,
        ///     framerate, or quality limitation reason.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         IEnumerator MonitorAdaptation()
        ///         {
        ///             while (true)
        ///             {
        ///                 var statsOp = sender.GetStats();
        ///                 yield return statsOp;
        ///                 foreach (var stat in statsOp.Value.Stats.Values)
        ///                 {
        ///                     if (stat is RTCOutboundRTPStreamStats outbound)
        ///                     {
        ///                         sender.CheckAdaptationState(outbound);
        ///                     }
        ///                 }
        ///                 yield return new WaitForSeconds(1f);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public bool CheckAdaptationState(RTCOutboundRTPStreamStats stats)
        {
            if (stats == null)
                return false;

            var currentState = GetAdaptationStateFromStats(stats);

            // Check if this is the first state or if there's a significant change
            bool hasChanged = lastAdaptationState == null ||
                              lastAdaptationState.FrameWidth != currentState.FrameWidth ||
                              lastAdaptationState.FrameHeight != currentState.FrameHeight ||
                              lastAdaptationState.QualityLimitationReason != currentState.QualityLimitationReason ||
                              Math.Abs(lastAdaptationState.FramesPerSecond - currentState.FramesPerSecond) > 1.0;

            if (hasChanged && lastAdaptationState != null)
            {
                OnAdaptationChanged?.Invoke(this, new AdaptationChangedEventArgs(lastAdaptationState, currentState));
            }

            lastAdaptationState = currentState;
            return hasChanged && lastAdaptationState != null;
        }

        /// <summary>
        ///     Gets the last known adaptation state.
        /// </summary>
        /// <remarks>
        ///     Returns null if CheckAdaptationState has never been called.
        /// </remarks>
        public RTCVideoAdaptationState LastAdaptationState => lastAdaptationState;

        /// <summary>
        ///     Retrieves the current configuration of the RTCRtpSender.
        /// </summary>
        /// <remarks>
        ///     `GetParameters` method retrieves `RTCRtpSendParameters` object describing the current configuration of the `RTCRtpSender`.
        /// </remarks>
        /// <returns>`RTCRtpSendParameters` object containing the current configuration of the `RTCRtpSender`.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpSender sender = peerConnection.GetSenders().First();
        ///         RTCRtpSendParameters parameters = sender.GetParameters();
        ///         parameters.encodings[0].maxBitrate = bandwidth * 1000;
        ///         RTCError error = sender.SetParameters(parameters);
        ///         if (error.errorType != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to set parameters: {error.message}");
        ///         }
        ///     ]]></code>
        /// </example>
        /// <seealso cref="SetParameters" />
        public RTCRtpSendParameters GetParameters()
        {
            NativeMethods.SenderGetParameters(GetSelfOrThrow(), out var ptr);
            RTCRtpSendParametersInternal parametersInternal = Marshal.PtrToStructure<RTCRtpSendParametersInternal>(ptr);
            RTCRtpSendParameters parameters = new RTCRtpSendParameters(ref parametersInternal);
            Marshal.FreeHGlobal(ptr);
            return parameters;
        }

        /// <summary>
        ///     Updates the configuration of the sender's track.
        /// </summary>
        /// <remarks>
        ///     `SetParameters` method updates the configuration of the sender's `MediaStreamTrack`
        ///     by applying changes the RTP transmission and the encoding parameters for a specific outgoing media on the connection.
        /// </remarks>
        /// <param name="parameters">
        ///     A `RTCRtpSendParameters` object previously obtained by calling the sender's `GetParameters`,
        ///     includes desired configuration changes and potential codecs for encoding the sender's track.
        /// </param>
        /// <returns>`RTCError` value.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpSender sender = peerConnection.GetSenders().First();
        ///         RTCRtpSendParameters parameters = sender.GetParameters();
        ///         parameters.encodings[0].maxBitrate = bandwidth * 1000;
        ///         RTCError error = sender.SetParameters(parameters);
        ///         if (error.errorType != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to set parameters: {error.message}");
        ///         }
        ///     ]]></code>
        /// </example>
        /// <seealso cref="GetParameters" />
        public RTCError SetParameters(RTCRtpSendParameters parameters)
        {
            if (Track is VideoStreamTrack videoTrack)
            {
                foreach (var encoding in parameters.encodings)
                {
                    var scale = encoding.scaleResolutionDownBy;
                    if (!scale.HasValue)
                    {
                        continue;
                    }

                    var error = WebRTC.ValidateTextureSize((int)(videoTrack.Texture.width / scale),
                        (int)(videoTrack.Texture.height / scale), Application.platform);
                    if (error.errorType != RTCErrorType.None)
                    {
                        return error;
                    }
                }
            }

            parameters.CreateInstance(out RTCRtpSendParametersInternal instance);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(instance));
            Marshal.StructureToPtr(instance, ptr, false);
            RTCErrorType type = NativeMethods.SenderSetParameters(GetSelfOrThrow(), ptr);
            Marshal.FreeCoTaskMem(ptr);
            return new RTCError { errorType = type };
        }

        /// <summary>
        ///     Replaces the current source track with a new MediaStreamTrack.
        /// </summary>
        /// <remarks>
        ///    `ReplaceTrack` method replaces the track currently being used as the sender's source with a new `MediaStreamTrack`.
        ///    It is often used to switch between two cameras.
        /// </remarks>
        /// <param name="track">
        ///     A `MediaStreamTrack` to replace the current source track of the `RTCRtpSender`.
        ///     The new track must be the same type as the current one.
        /// </param>
        /// <returns>`true` if the track has been successfully replaced.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpTransceiver transceiver = peerConnection.GetTransceivers().First();
        ///         transceiver.Sender.ReplaceTrack(newTrack);
        ///     ]]></code>
        /// </example>
        public bool ReplaceTrack(MediaStreamTrack track)
        {
            IntPtr trackPtr = track?.GetSelfOrThrow() ?? IntPtr.Zero;
            return NativeMethods.SenderReplaceTrack(GetSelfOrThrow(), trackPtr);
        }
    }
}
