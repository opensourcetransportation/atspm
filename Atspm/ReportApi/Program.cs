#region license
// Copyright 2026 Utah Departement of Transportation
// for ReportApi - %Namespace%/Program.cs
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using Utah.Udot.Atspm.Business.AppoachDelay;
using Utah.Udot.Atspm.Business.ApproachSpeed;
using Utah.Udot.Atspm.Business.ApproachVolume;
using Utah.Udot.Atspm.Business.ArrivalOnRed;
using Utah.Udot.Atspm.Business.GreenTimeUtilization;
using Utah.Udot.Atspm.Business.LeftTurnGapAnalysis;
using Utah.Udot.Atspm.Business.LeftTurnGapReport;
using Utah.Udot.Atspm.Business.LinkPivot;
using Utah.Udot.Atspm.Business.PedDelay;
using Utah.Udot.Atspm.Business.PhaseTermination;
using Utah.Udot.Atspm.Business.PreempDetail;
using Utah.Udot.Atspm.Business.PreemptService;
using Utah.Udot.Atspm.Business.PreemptServiceRequest;
using Utah.Udot.Atspm.Business.PriorityDetails;
using Utah.Udot.Atspm.Business.PrioritySummary;
using Utah.Udot.Atspm.Business.PurdueCoordinationDiagram;
using Utah.Udot.Atspm.Business.RampMetering;
using Utah.Udot.Atspm.Business.SplitFail;
using Utah.Udot.Atspm.Business.SplitMonitor;
using Utah.Udot.Atspm.Business.TimeOfDay;
using Utah.Udot.Atspm.Business.TimeSpaceDiagram;
using Utah.Udot.Atspm.Business.TimingAndActuation;
using Utah.Udot.Atspm.Business.TransitSignalPriority;
using Utah.Udot.Atspm.Business.TransitSignalPriorityRequest;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Business.WaitTime;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Business.YellowRedActivations;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Infrastructure.Common;
using Utah.Udot.Atspm.ReportApi.DataAggregation;
using Utah.Udot.Atspm.ReportApi.ReportServices;
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;
using Utah.Udot.ATSPM.ReportApi.DataAggregation;
using Utah.Udot.ATSPM.ReportApi.ReportServices;

//git 2

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ApplyVolumeConfiguration()
    .ConfigureLogging((h, l) => l.AddGoogle(h))
    .ConfigureServices((h, s) =>
    {
        s.AddControllers(o =>
        {
            o.ReturnHttpNotAcceptable = true;
            o.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
            o.Filters.Add(new ProducesAttribute("application/json"));
        });
        s.AddProblemDetails();
        s.AddConfiguredCompression(new[] { "application/json", "application/xml", "text/csv", "application/x-ndjson" });
        s.AddConfiguredSwagger(builder.Configuration, o =>
        {
            o.IncludeXmlComments(typeof(Program).Assembly);
            o.CustomOperationIds((controller, verb, action) => $"{verb}{controller}{action}");
            o.CustomSchemaIds(type => type.Name);
            o.EnableAnnotations();
            o.AddAtspmSecurityDefinitions();

        });
        s.AddConfiguredCors(builder.Configuration);
        s.AddAtspmHttpLogging();

        s.AddAtspmDbContext(h);
        s.AddAtspmEFEventLogRepositories();
        s.AddAtspmEFConfigRepositories();
        s.AddAtspmEFAggregationRepositories();

        //report services
        s.AddScoped<IReportService<AggregationOptions, IEnumerable<AggregationResult>>, AggregationReportService>();
        s.AddScoped<IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>, ApproachDelayReportService>();
        s.AddScoped<IReportService<PedatLocationDataQuery, IEnumerable<PedatLocationData>>, PedestrianAggregationService>();
        s.AddScoped<IReportService<ApproachSpeedOptions, IEnumerable<ApproachSpeedResult>>, ApproachSpeedReportService>();
        s.AddScoped<IReportService<ApproachVolumeOptions, IEnumerable<ApproachVolumeResult>>, ApproachVolumeReportService>();
        s.AddScoped<IReportService<ArrivalOnRedOptions, IEnumerable<ArrivalOnRedResult>>, ArrivalOnRedReportService>();
        s.AddScoped<IReportService<GapDurationOptions, GapDurationResult>, LeftTurnGapDurationService>();
        s.AddScoped<IReportService<GreenTimeUtilizationOptions, IEnumerable<GreenTimeUtilizationResult>>, GreenTimeUtilizationReportService>();
        s.AddScoped<IReportService<LeftTurnGapAnalysisOptions, IEnumerable<LeftTurnGapAnalysisResult>>, LeftTurnGapAnalysisReportService>();
        s.AddScoped<IReportService<LeftTurnGapDataCheckOptions, LeftTurnGapDataCheckResult>, LeftTurnGapReportDataCheckService>();
        s.AddScoped<IReportService<LeftTurnSplitFailOptions, LeftTurnSplitFailResult>, LeftTurnSplitFailService>();
        s.AddScoped<IReportService<LeftTurnGapReportOptions, IEnumerable<LeftTurnGapReportResult>>, LeftTurnGapReportService>();
        s.AddScoped<IReportService<LinkPivotOptions, LinkPivotResult>, LinkPivotReportService>();
        s.AddScoped<LinkPivotReportService>();
        s.AddScoped<IReportService<VolumeOptions, VolumeResult>, LeftTurnVolumeService>();
        s.AddScoped<IReportService<PedActuationOptions, PedActuationResult>, LeftTurnPedActuationService>();
        s.AddScoped<IReportService<PedDelayOptions, IEnumerable<PedDelayResult>>, PedDelayReportService>();
        s.AddScoped<IReportService<PeakHourOptions, PeakHourResult>, LeftTurnPeakHourService>();
        s.AddScoped<IReportService<PreemptDetailOptions, PreemptDetailResult>, PreemptDetailReportService>();
        s.AddScoped<IReportService<PreemptServiceOptions, PreemptServiceResult>, PreemptServiceReportService>();
        s.AddScoped<IReportService<PreemptServiceRequestOptions, PreemptServiceRequestResult>, PreemptRequestReportService>();
        s.AddScoped<IReportService<PrioritySummaryOptions, PrioritySummaryResult>, PrioritySummaryReportService>();
        s.AddScoped<IReportService<PriorityDetailsOptions, IEnumerable<PriorityDetailsResult>>, PriorityDetailsReportService>();
        s.AddScoped<IReportService<PurdueCoordinationDiagramOptions, IEnumerable<PurdueCoordinationDiagramResult>>, PurdueCoordinationDiagramReportService>();
        s.AddScoped<IReportService<PurduePhaseTerminationOptions, PhaseTerminationResult>, PurduePhaseTerminationReportService>();
        s.AddScoped<IReportService<RampMeteringOptions, RampMeteringResult>, RampMeteringReportService>();
        s.AddScoped<IReportService<SplitFailOptions, IEnumerable<SplitFailsResult>>, SplitFailReportService>();
        s.AddScoped<IReportService<SplitMonitorOptions, IEnumerable<SplitMonitorResult>>, SplitMonitorReportService>();
        s.AddScoped<IReportService<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramPhaseResult>>, TimeSpaceDiagramReportService>();
        s.AddScoped<IReportService<TimeSpaceDiagramAverageOptions, IEnumerable<TimeSpaceDiagramAveragePhaseResult>>, TimeSpaceDiagramAverageReportService>();
        s.AddScoped<IReportService<TimeOfDayOptions, TimeOfDayResult>, TimeOfDayReportService>();
        s.AddScoped<IReportService<TransitSignalPriorityOptions, List<TransitSignalPriorityResult>>, TransitSignalPriorityReportService>();
        s.AddScoped<IReportService<TimingAndActuationsOptions, IEnumerable<TimingAndActuationsForPhaseResult>>, TimingAndActuactionReportService>();
        s.AddScoped<IReportService<TurningMovementCountsOptions, TurningMovementCountsResult>, TurningMovementCountReportService>();
        s.AddScoped<IReportService<YellowRedActivationsOptions, IEnumerable<YellowRedActivationsResult>>, YellowRedActivationsReportService>();
        s.AddScoped<IReportService<WaitTimeOptions, IEnumerable<WaitTimeResult>>, WaitTimeReportService>();
        s.AddScoped<IReportService<WatchDogOptions, WatchDogResult>, WatchDogReportService>();
        s.AddScoped<WatchDogDashboardReportService>();

        //AggregationResult Services
        s.AddScoped<AggregationReportService>();
        s.AddScoped<PedestrianAggregationService>();
        s.AddScoped<ApproachDelayService>();
        s.AddScoped<ApproachSpeedService>();
        s.AddScoped<ApproachVolumeService>();
        s.AddScoped<ArrivalOnRedService>();
        s.AddScoped<LeftTurnGapAnalysisService>();
        s.AddScoped<LeftTurnGapReportDataCheckService>();
        s.AddScoped<LeftTurnSplitFailService>();
        s.AddScoped<LeftTurnPedActuationService>();
        s.AddScoped<LeftTurnGapDurationService>();
        s.AddScoped<LeftTurnVolumeService>();
        s.AddScoped<LeftTurnGapReportService>();

        //s.AddScoped<LeftTurnVolumeAnalysisService>();
        s.AddScoped<PedDelayService>();
        s.AddScoped<GreenTimeUtilizationService>();
        s.AddScoped<LeftTurnPeakHourService>();
        s.AddScoped<PreemptServiceService>();
        s.AddScoped<PreemptServiceRequestService>();
        s.AddScoped<PrioritySummaryService>();
        s.AddScoped<PriorityDetailsService>();
        s.AddScoped<PurdueCoordinationDiagramService>();
        s.AddScoped<SplitFailPhaseService>();
        s.AddScoped<SplitMonitorService>();
        s.AddScoped<TimeSpaceDiagramForPhaseService>();
        s.AddScoped<ITimeSpaceDiagramSrmSource, TimeSpaceDiagramSrmCsvSource>();
        s.AddScoped<TimeSpaceDiagramSrmService>();
        s.AddScoped<TimeSpaceAverageService>();
        s.AddScoped<TimeOfDayService>();
        s.AddScoped<TimeOfDayLocationService>();
        s.AddScoped<ITimeOfDayObservationService, TimeOfDayObservationService>();
        s.AddScoped<ITimeOfDayProfileService, TimeOfDayProfileService>();
        s.AddScoped<ITimeOfDayRecommendationService, TimeOfDayRecommendationService>();
        s.AddScoped<ITimeOfDayPlanScheduleService, TimeOfDayPlanScheduleService>();
        s.AddScoped<ITimeOfDayPlanProfileService, TimeOfDayPlanProfileService>();
        s.AddScoped<ITimeOfDaySplitPressureService, TimeOfDaySplitPressureService>();
        s.AddScoped<TimingAndActuationsForPhaseService>();
        s.AddScoped<PriorityDetailsReportService>();
        s.AddScoped<TimeSpaceDiagramReportService>();
        s.AddScoped<TransitSignalPriorityService>();
        s.AddScoped<TurningMovementCountsService>();
        s.AddScoped<WaitTimeService>();
        s.AddScoped<YellowRedActivationsService>();
        s.AddScoped<WatchDogReportService>();

        //Aggregation Services
        s.AddScoped<DetectorVolumeAggregationOptions>();
        s.AddScoped<ApproachSpeedAggregationOptions>();
        s.AddScoped<ApproachPcdAggregationOptions>();
        s.AddScoped<PhaseCycleAggregationOptions>();
        s.AddScoped<ApproachSplitFailAggregationOptions>();
        s.AddScoped<ApproachYellowRedActivationsAggregationOptions>();
        s.AddScoped<PreemptionAggregationOptions>();
        s.AddScoped<PriorityAggregationOptions>();
        s.AddScoped<SignalEventCountAggregationOptions>();
        s.AddScoped<PhaseTerminationAggregationOptions>();
        s.AddScoped<PhasePedAggregationOptions>();
        s.AddScoped<PhaseLeftTurnGapAggregationOptions>();
        s.AddScoped<PhaseSplitMonitorAggregationOptions>();

        //Common Services
        s.AddScoped<PlanService>();
        s.AddScoped<PedActuationService>();
        s.AddScoped<LocationPhaseService>();
        s.AddScoped<CycleService>();
        s.AddScoped<PedPhaseService>();
        s.AddScoped<GapDurationService>();
        s.AddScoped<AnalysisPhaseCollectionService>();
        s.AddScoped<AnalysisPhaseService>();
        s.AddScoped<PreemptDetailService>();
        s.AddScoped<PhaseService>();
        s.AddScoped<SplitFailService>();
        s.AddScoped<LeftTurnReportService>();
        s.AddScoped<VolumeService>();
        s.AddScoped<LinkPivotService>();
        s.AddScoped<LinkPivotPairService>();
        s.AddScoped<LinkPivotPcdService>();
        s.AddScoped<WatchDogIgnoreEventService>();
        s.AddScoped<RampMeteringService>();
        s.AddScoped<DetectionService>();

        s.AddPathBaseFilter(h);

        s.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );
        });

        s.AddAtspmIdentity(h);
        s.AddHealthChecks();
    });

var app = builder.Build();

await app.ApplyMigrations<ConfigContext>();
await app.ApplyMigrations<EventLogContext>();
await app.ApplyMigrations<AggregationContext>();

#region Middleware Pipeline

//Error handling
if (!app.Environment.IsProduction())
{
    app.Services.PrintHostInformation();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

//Security
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

//Cross-cutting
app.UseResponseCompression();
app.UseHttpLogging();
app.UseMiddleware<UsageLoggingMiddleware>(
    (HttpContext ctx) => Assembly.GetEntryAssembly()?.GetName().Name,
    (HttpContext ctx, ControllerActionDescriptor? ad) => false,
    (HttpContext ctx, ControllerActionDescriptor? ad) => ad?.ActionName == "GetReportData",
    (HttpContext ctx) => ctx.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User?.Identity?.Name);

//Swagger
app.UseConfiguredSwaggerUI();

//Endpoints
app.MapControllers();
app.MapJsonHealthChecks();

#endregion

app.Run();
