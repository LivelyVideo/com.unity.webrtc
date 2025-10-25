using NUnit.Framework;

namespace Unity.WebRTC.RuntimeTest
{
    class FieldTrialsTest
    {
        [Test]
        public void FieldTrialsBuilder_BuildsCorrectFormat()
        {
            var trials = FieldTrials.Build(
                ("WebRTC-Test1", "Enabled"),
                ("WebRTC-Test2", "Disabled")
            );

            Assert.That(trials, Is.EqualTo("WebRTC-Test1/Enabled/WebRTC-Test2/Disabled/"));
        }

        [Test]
        public void FieldTrialsBuilder_EmptyArray()
        {
            var trials = FieldTrials.Build();

            Assert.That(trials, Is.EqualTo(""));
        }

        [Test]
        public void FieldTrialsBuilder_SingleEntry()
        {
            var trials = FieldTrials.Build(
                (FieldTrials.SimulcastScreenshare, "Enabled")
            );

            Assert.That(trials, Is.EqualTo("WebRTC-SimulcastScreenshare/Enabled/"));
        }

        [Test]
        public void FieldTrialsBuilder_MultipleEntries()
        {
            var trials = FieldTrials.Build(
                (FieldTrials.SimulcastScreenshare, "Enabled"),
                (FieldTrials.BandwidthEstimation, "Enabled"),
                (FieldTrials.FlexFec, "Disabled")
            );

            var expected = "WebRTC-SimulcastScreenshare/Enabled/" +
                          "WebRTC-Bwe-NetworkEstimation/Enabled/" +
                          "WebRTC-FlexFEC-03/Disabled/";

            Assert.That(trials, Is.EqualTo(expected));
        }

        [Test]
        public void ValidateFieldTrialsFormat_ValidFormat()
        {
            // Valid format: key/value/key/value/
            var valid1 = "WebRTC-Test/Enabled/";
            var valid2 = "WebRTC-Test1/Enabled/WebRTC-Test2/Disabled/";
            var valid3 = ""; // Empty is valid

            Assert.That(WebRTC.ValidateFieldTrialsFormat(valid1), Is.True);
            Assert.That(WebRTC.ValidateFieldTrialsFormat(valid2), Is.True);
            Assert.That(WebRTC.ValidateFieldTrialsFormat(valid3), Is.True);
            Assert.That(WebRTC.ValidateFieldTrialsFormat(null), Is.True);
        }

        [Test]
        public void ValidateFieldTrialsFormat_InvalidFormat_MissingTrailingSlash()
        {
            var invalid = "WebRTC-Test/Enabled";

            Assert.That(WebRTC.ValidateFieldTrialsFormat(invalid), Is.False);
        }

        [Test]
        public void ValidateFieldTrialsFormat_InvalidFormat_MissingValue()
        {
            var invalid = "WebRTC-Test/Enabled/WebRTC-Test2/";

            Assert.That(WebRTC.ValidateFieldTrialsFormat(invalid), Is.False);
        }

        [Test]
        public void ValidateFieldTrialsFormat_InvalidFormat_OddSegments()
        {
            var invalid1 = "WebRTC-Test/";
            var invalid2 = "WebRTC-Test1/Value1/WebRTC-Test2/";

            Assert.That(WebRTC.ValidateFieldTrialsFormat(invalid1), Is.False);
            Assert.That(WebRTC.ValidateFieldTrialsFormat(invalid2), Is.False);
        }

        [Test]
        public void FieldTrialsConstants_AreNotEmpty()
        {
            Assert.That(FieldTrials.SimulcastScreenshare, Is.Not.Empty);
            Assert.That(FieldTrials.BandwidthEstimation, Is.Not.Empty);
            Assert.That(FieldTrials.FlexFec, Is.Not.Empty);
            Assert.That(FieldTrials.H264HighProfile, Is.Not.Empty);
            Assert.That(FieldTrials.VP9FlexibleMode, Is.Not.Empty);
            Assert.That(FieldTrials.GenericDescriptor, Is.Not.Empty);
            Assert.That(FieldTrials.AudioNetEq, Is.Not.Empty);
            Assert.That(FieldTrials.MinVideoBitrate, Is.Not.Empty);
        }
    }
}
