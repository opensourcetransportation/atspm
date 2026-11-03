// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpacePhaseLayout.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
import {
  getDistanceAtDisplayCoordinate,
  getDisplayDistanceScale,
  getHybridDistanceData,
  getSequenceDistanceData,
  getTimeSpaceChartHeight,
  getTimeSpacePhaseRowDistances,
} from '../math/timeSpaceLayout'
import type {
  TimeSpaceDistanceSpacingMode,
  TimeSpaceCoreRow,
  TimeSpacePhaseLayout,
} from '../types/timeSpaceCore.types'

export function buildTimeSpacePhaseLayout<T extends TimeSpaceCoreRow>(
  data: T[],
  options?: {
    distanceSpacingMode?: TimeSpaceDistanceSpacingMode
    sortByOrder?: boolean
  }
): TimeSpacePhaseLayout<T> {
  const byOrder = (a: T, b: T) =>
    (a.order ?? Number.MAX_SAFE_INTEGER) - (b.order ?? Number.MAX_SAFE_INTEGER)

  const primaryPhaseData = data
    .filter((location) => location.phaseType === 'Primary')
    .sort(options?.sortByOrder ? byOrder : undefined)
  const opposingPhaseData = data
    .filter((location) => location.phaseType === 'Opposing')
    .sort(options?.sortByOrder ? byOrder : undefined)

  let initialDistance = 0
  const rawDistanceData: number[] = []
  primaryPhaseData.forEach((location) => {
    rawDistanceData.push(initialDistance)
    initialDistance += location.distanceToNextLocation
  })

  const distanceScale = getDisplayDistanceScale(rawDistanceData)
  const distanceSpacingMode = options?.distanceSpacingMode ?? 'distance'
  const locationCenterDistanceData = getLocationCenterDistanceData(
    rawDistanceData,
    distanceScale,
    distanceSpacingMode
  )
  const getDisplayDistanceOffset = (
    index: number,
    rawDistanceOffset: number
  ) => {
    const rawDistance = rawDistanceData[index]
    const displayDistance = locationCenterDistanceData[index]

    if (
      // A single location has no segment from which to interpolate offsets.
      rawDistanceData.length < 2 ||
      rawDistance == null ||
      displayDistance == null ||
      !Number.isFinite(rawDistanceOffset)
    ) {
      return rawDistanceOffset * distanceScale
    }

    return (
      getDistanceAtDisplayCoordinate(
        rawDistanceData,
        locationCenterDistanceData,
        rawDistance + rawDistanceOffset
      ) - displayDistance
    )
  }
  const { primaryDistanceData, opposingDistanceData } =
    getTimeSpacePhaseRowDistances(locationCenterDistanceData)
  const minDisplayDistance = Math.min(
    ...primaryDistanceData,
    ...opposingDistanceData
  )
  const maxDisplayDistance = Math.max(
    ...primaryDistanceData,
    ...opposingDistanceData
  )

  return {
    primaryDirection: primaryPhaseData[0].approachDescription,
    opposingDirection: opposingPhaseData[0].approachDescription,
    primaryPhaseData,
    opposingPhaseData,
    rawDistanceData,
    distanceScale,
    locationCenterDistanceData,
    primaryDistanceData,
    opposingDistanceData,
    getDisplayDistanceOffset,
    minDisplayDistance,
    maxDisplayDistance,
    chartHeight: getTimeSpaceChartHeight(
      minDisplayDistance,
      maxDisplayDistance,
      primaryPhaseData.length
    ),
  }
}

function getLocationCenterDistanceData(
  rawDistanceData: number[],
  distanceScale: number,
  distanceSpacingMode: TimeSpaceDistanceSpacingMode
) {
  switch (distanceSpacingMode) {
    case 'sequence':
      return getSequenceDistanceData(rawDistanceData.length)
    case 'hybrid':
      return getHybridDistanceData(rawDistanceData)
    case 'distance':
    default:
      return rawDistanceData.map((distance) => distance * distanceScale)
  }
}
