using System.Text;

namespace Unity.WebRTC
{
    /// <summary>
    /// Helper for building WebRTC field trials configuration strings.
    /// Field trials enable experimental features in WebRTC.
    /// IMPORTANT: Field trials must be set before WebRTC initialization and cannot be changed afterward.
    /// </summary>
    /// <remarks>
    /// Field trials are process-wide settings that control experimental WebRTC features.
    /// Once WebRTC is initialized, field trials cannot be modified.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Build a field trials string
    /// var trials = FieldTrials.Build(
    ///     (FieldTrials.SimulcastScreenshare, "Enabled"),
    ///     (FieldTrials.BandwidthEstimation, "Enabled")
    /// );
    /// </code>
    /// </example>
    public static class FieldTrials
    {
        /// <summary>
        /// Build a field trials string from key-value pairs.
        /// </summary>
        /// <param name="trials">Array of (key, value) tuples representing field trial configurations.</param>
        /// <returns>A formatted field trials string in the format: key1/value1/key2/value2/</returns>
        /// <example>
        /// <code>
        /// var trials = FieldTrials.Build(
        ///     ("WebRTC-SimulcastScreenshare", "Enabled"),
        ///     ("WebRTC-Bwe-NetworkEstimation", "Disabled")
        /// );
        /// // Returns: "WebRTC-SimulcastScreenshare/Enabled/WebRTC-Bwe-NetworkEstimation/Disabled/"
        /// </code>
        /// </example>
        public static string Build(params (string key, string value)[] trials)
        {
            var sb = new StringBuilder();
            foreach (var (key, value) in trials)
            {
                sb.Append($"{key}/{value}/");
            }
            return sb.ToString();
        }

        // Common field trial names as constants

        /// <summary>
        /// Enable simulcast for screen sharing.
        /// </summary>
        public const string SimulcastScreenshare = "WebRTC-SimulcastScreenshare";

        /// <summary>
        /// Bandwidth estimation configuration.
        /// </summary>
        public const string BandwidthEstimation = "WebRTC-Bwe-NetworkEstimation";

        /// <summary>
        /// FlexFEC (Flexible Forward Error Correction) for improved packet loss recovery.
        /// </summary>
        public const string FlexFec = "WebRTC-FlexFEC-03";

        /// <summary>
        /// Enable H.264 High Profile encoding.
        /// </summary>
        public const string H264HighProfile = "WebRTC-H264HighProfile";

        /// <summary>
        /// VP9 flexible mode configuration.
        /// </summary>
        public const string VP9FlexibleMode = "WebRTC-VP9-FlexibleMode";

        /// <summary>
        /// Generic descriptor configuration for improved codec compatibility.
        /// </summary>
        public const string GenericDescriptor = "WebRTC-GenericDescriptor";

        /// <summary>
        /// Audio NetEq configuration for improved audio quality.
        /// </summary>
        public const string AudioNetEq = "WebRTC-Audio-NetEq";

        /// <summary>
        /// Minimum video bitrate configuration.
        /// </summary>
        public const string MinVideoBitrate = "WebRTC-Video-MinVideoBitrate";
    }
}
