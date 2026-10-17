#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/CalculateTotalVolumes.cs
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

using Utah.Udot.Atspm.Common;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    public class CalculateTotalVolumes : TransformProcessStepBase<Tuple<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>>, Tuple<Approach, TotalVolumes>>
    {
        protected override Task<Tuple<Approach, TotalVolumes>> Process(Tuple<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>> input, CancellationToken cancelToken = default)
        {
            var p = input?.Item1?.Item1;
            var o = input?.Item2?.Item1;

            if (p?.Location == null || o?.Location == null)
                return Task.FromResult(Tuple.Create<Approach, TotalVolumes>(null, null));

            if (p.DirectionTypeId == new OpposingDirection(o.DirectionTypeId) && p.Location.LocationIdentifier == o.Location.LocationIdentifier)
            {
                if (input.Item1.Item2 == null || input.Item2.Item2 == null)
                    return Task.FromResult(Tuple.Create<Approach, TotalVolumes>(p, null));
                var volumes = input.Item1.Item2.Segments.Union(input.Item2.Item2.Segments);

                var result = new TotalVolumes(volumes, TimeSpan.FromMinutes(15))
                {
                    LocationIdentifier = p.Location.LocationIdentifier,
                };

                result.Segments.ToList().ForEach(f =>
                {
                    f.LocationIdentifier = p.Location.LocationIdentifier;
                    // These are whole bins, not timestamps in an end-exclusive range.
                    f.Primary = input.Item1.Item2.Segments.FirstOrDefault(d => d.Start == f.Start && d.End == f.End);
                    f.Opposing = input.Item2.Item2.Segments.FirstOrDefault(d => d.Start == f.Start && d.End == f.End);
                });

                return Task.FromResult(Tuple.Create(p, result));
            }

            return Task.FromResult(Tuple.Create<Approach, TotalVolumes>(null, null));
        }
    }
}
