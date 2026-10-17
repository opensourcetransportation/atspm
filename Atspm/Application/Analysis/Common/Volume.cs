#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Common/Volume.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Newtonsoft.Json;
using System.Text.Json;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Interfaces;

namespace Utah.Udot.Atspm.Analysis.Common
{
    public interface IDetectorCount
    {
        int DetectorCount { get; }
    }

    public interface IPhaseVolume : IStartEndRange, ILocationPhaseLayer, IDetectorCount
    {
        DirectionTypes Direction { get; set; }
    }

    public interface IToltalVolume : IStartEndRange, ILocationLayer, IDetectorCount
    {

    }

    public class TotalVolume : StartEndRange, IToltalVolume
    {
        public Volume Primary { get; set; }
        public Volume Opposing { get; set; }

        #region IToltalVolume

        #region ILocationLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        #endregion

        #region IDetectorCount

        public int DetectorCount => (Primary?.DetectorCount ?? 0) + (Opposing?.DetectorCount ?? 0);

        #endregion

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier} --- {Primary} --- {Opposing} --- {Start} - {End} - {DetectorCount}";
        }
    }

    public class TotalVolumes : Timeline<TotalVolume>, IToltalVolume
    {
        public TotalVolumes() { }

        public TotalVolumes(DateTime start, DateTime end, TimeSpan segmentSpan) : base(start, end, segmentSpan) { }

        public TotalVolumes(IEnumerable<ITimestamp> ranges, TimeSpan segmentSpan) : base(ranges, segmentSpan) { }

        public TotalVolumes(IEnumerable<IStartEndRange> ranges, TimeSpan segmentSpan) : base(ranges, segmentSpan) { }

        #region IToltalVolume

        #region ILocationLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        #endregion

        #region IDetectorCount

        public int DetectorCount => Segments.Sum(s => s.DetectorCount);

        #endregion

        #endregion
    }

    //public class Volume : List<IDetectorEvent>, ILocationPhaseLayer, IPhaseVolume
    //{
    //    #region IPhaseVolume

    //    #region ILocationPhaseLayer

    //    /// <inheritdoc/>
    //    public string LocationIdentifier { get; set; }

    //    /// <inheritdoc/>
    //    public int PhaseNumber { get; set; }

    //    #endregion

    //    #region IStartEndRange

    //    public DateTime End { get; set; }

    //    public DateTime Start { get; set; }

    //    public virtual bool InRange(DateTime time)
    //    {
    //        return time >= Start && time < End;
    //    }



    //    #endregion

    //    #region IDetectorCount

    //    public int DetectorCount => this.Count();

    //    #endregion

    //    public DirectionTypes Direction { get; set; }

    //    #endregion

    //    public override string ToString()
    //    {
    //        return $"{LocationIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
    //    }
    //}

    public class Volume : StartEndRange, IPhaseVolume
    {
        #region IPhaseVolume

        #region ILocationPhaseLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        #region IDetectorCount

        /// <inheritdoc/>
        public int DetectorCount => DetectorEvents.Count();

        #endregion

        public DirectionTypes Direction { get; set; }

        #endregion

        #region IStartEndRange

        /// <inheritdoc/>
        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        public virtual bool InRange(ITimestamp time)
        {
            return time.Timestamp >= Start && time.Timestamp < End;
        }

        #endregion

        public List<CorrectedDetectorEvent> DetectorEvents { get; set; } = new();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
        }
    }

    //[JsonObject(memberSerialization: MemberSerialization.OptIn)]
    //public class Volume : StartEndList<CorrectedDetectorEvent>, ILocationPhaseLayer, IPhaseVolume
    //{
    //    #region IPhaseVolume

    //    #region ILocationPhaseLayer

    //    /// <inheritdoc/>
    //    [JsonProperty]
    //    public string LocationIdentifier { get; set; }

    //    /// <inheritdoc/>
    //    [JsonProperty]
    //    public int PhaseNumber { get; set; }

    //    #endregion

    //    #region IDetectorCount

    //    [JsonProperty]
    //    public int DetectorCount => this.Count();

    //    #endregion

    //    [JsonProperty]
    //    public DirectionTypes Direction { get; set; }

    //    #endregion

    //    #region IStartEndRange

    //    public virtual bool InRange(DateTime time)
    //    {
    //        return time >= Start && time < End;
    //    }

    //    //public new void AddRange(IEnumerable<CorrectedDetectorEvent> collection)
    //    //{
    //    //    base.AddRange()
    //    //}

    //    #endregion

    //    [JsonProperty]
    //    CorrectedDetectorEvent[] Items
    //    {
    //        get
    //        {
    //            return this.ToArray();
    //        }
    //        set
    //        {
    //            if (value != null)
    //                this.AddRange(value);
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        return $"{LocationIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
    //    }
    //}

    //[JsonObject(memberSerialization: MemberSerialization.OptIn)]
    public class Volumes : Timeline<Volume>, IPhaseVolume
    {
        public Volumes() { }

        public Volumes(DateTime start, DateTime end, TimeSpan segmentSpan) : base(start, end, segmentSpan) { }

        public Volumes(IEnumerable<ITimestamp> ranges, TimeSpan segmentSpan) : base(ranges, segmentSpan) { }

        public Volumes(IEnumerable<IStartEndRange> ranges, TimeSpan segmentSpan) : base(ranges, segmentSpan) { }

        #region IPhaseVolume

        #region ILocationPhaseLayer

        /// <inheritdoc/>
        [JsonProperty]
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        public int PhaseNumber { get; set; }

        #endregion

        #region IStartEndRange

        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        #endregion

        #region IDetectorCount

        [JsonProperty]
        public int DetectorCount => Segments.Sum(s => s.DetectorCount);

        #endregion

        [JsonProperty]
        public DirectionTypes Direction { get; set; }

        #endregion

        public override string ToString()
        {
            return $"{LocationIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
        }
    }
}
