// SPDX-License-Identifier: Apache-2.0

using System;
using Utah.Udot.Atspm.Analysis.Common;
using Utah.Udot.NetStandardToolkit.Common;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows;

// A complete cycle contract for testing the supported metric components independently
// of the legacy workflow orchestrators that are excluded from the application build.
internal sealed class SignalMetricTestCycle : StartEndRange, IRedToRedCycle
{
    public SignalMetricTestCycle(string location = "7115", int phase = 2)
    {
        LocationIdentifier = location;
        PhaseNumber = phase;
        Start = new DateTime(2025, 12, 25, 8, 0, 0);
        GreenEvent = Start.AddSeconds(30);
        YellowEvent = Start.AddSeconds(50);
        End = Start.AddSeconds(60);
    }

    public string LocationIdentifier { get; set; }
    public int PhaseNumber { get; set; }
    public DateTime GreenEvent { get; set; }
    public DateTime YellowEvent { get; set; }
    public double TotalGreenTime => (YellowEvent - GreenEvent).TotalSeconds;
    public double TotalYellowTime => (End - YellowEvent).TotalSeconds;
    public double TotalRedTime => (GreenEvent - Start).TotalSeconds;
    public double TotalTime => (End - Start).TotalSeconds;

    public Vehicle VehicleAt(double seconds) => new(new CorrectedDetectorEvent
    {
        LocationIdentifier = LocationIdentifier,
        PhaseNumber = PhaseNumber,
        DetectorChannel = 2,
        Timestamp = Start.AddSeconds(seconds)
    }, this);
}
