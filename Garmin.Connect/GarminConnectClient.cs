using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Garmin.Connect.Models;

namespace Garmin.Connect;

public class GarminConnectClient : IGarminConnectClient
{
    private readonly GarminConnectContext _context;

    private const string UserSettingsUrl = "/proxy/userprofile-service/userprofile/user-settings";
    private const string UserSummaryUrl = "/proxy/usersummary-service/usersummary/daily/";
    private const string UserSummaryChartUrl = "/proxy/wellness-service/wellness/dailySummaryChart/";
    private const string HeartRatesUrl = "/proxy/wellness-service/wellness/dailyHeartRate/";
    private const string SleepDataUrl = "/proxy/wellness-service/wellness/dailySleepData/";
    private const string BodyCompositionUrl = "/proxy/weight-service/weight/daterangesnapshot";
    private const string ActivitiesUrl = "/proxy/activitylist-service/activities/search/activities";
    private const string HydrationDataUrl = "/proxy/usersummary-service/usersummary/hydration/daily/";
    private const string ActivityUrl = "/proxy/activity-service/activity/";
    private const string PersonalRecordUrl = "/proxy/personalrecord-service/personalrecord/";
    private const string TcxDownloadUrl = "/proxy/download-service/export/tcx/activity/";
    private const string GpxDownloadUrl = "/proxy/download-service/export/gpx/activity/";
    private const string KmlDownloadUrl = "/proxy/download-service/export/kml/activity/";
    private const string FitDownloadUrl = "/proxy/download-service/files/activity/";
    private const string CsvDownloadUrl = "/proxy/download-service/export/csv/activity/";
    private const string DeviceListUrl = "/proxy/device-service/deviceregistration/devices";
    private const string DeviceServiceUrl = "/proxy/device-service/deviceservice/";

    public GarminConnectClient(GarminConnectContext context)
    {
        _context = context;
    }

    public Task<GarminActivity[]> GetActivities(int start, int limit)
    {
        var activitiesUrl = $"{ActivitiesUrl}?start={start}&limit={limit}";

        return _context.GetAndDeserialize<GarminActivity[]>(activitiesUrl);
    }

    public Task<GarminDevice[]> GetDevices()
    {
        return _context.GetAndDeserialize<GarminDevice[]>(DeviceListUrl);
    }

    public async Task<GarminStepsData[]> GetWellnessStepsData(DateTime date)
    {
        var profile = await GetSocialProfile();

        return await _context.GetAndDeserialize<GarminStepsData[]>(
            $"{UserSummaryChartUrl}{profile.DisplayName}?date={date:yyyy-MM-dd}");
    }

    public async Task<GarminActivity[]> GetActivitiesByDate(DateTime startDate, DateTime endDate,
        string activityType)
    {
        string activitySlug;

        var start = 0;
        var limit = 20;

        // mimicking the behavior of the web interface that fetches 20 activities at a time
        // and automatically loads more on scroll
        if (!string.IsNullOrEmpty(activityType))
        {
            activitySlug = "&activityType=" + activityType;
        }
        else
        {
            activitySlug = "";
        }

        var result = new List<GarminActivity>();

        var returnData = true;
        while (returnData)
        {
            var activitiesUrl =
                $"{ActivitiesUrl}?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&start={start}&limit={limit}{activitySlug}";

            var activities = await _context.GetAndDeserialize<GarminActivity[]>(activitiesUrl);

            if (activities.Any())
            {
                result.AddRange(activities);
                start += limit;
            }
            else
            {
                returnData = false;
            }
        }

        return result.ToArray();
    }

    public async Task<GarminUserPreferences> GetPreferences()
    {
        if (_context.Preferences is null)
        {
            await _context.ReLoginIfExpired();
        }

        return _context.Preferences;
    }

    public async Task<GarminSocialProfile> GetSocialProfile()
    {
        if (_context.Profile is null)
        {
            await _context.ReLoginIfExpired();
        }

        return _context.Profile;
    }

    public Task<GarminUserSettings> GetUserSettings()
    {
        return _context.GetAndDeserialize<GarminUserSettings>(UserSettingsUrl);
    }

    public async Task<GarminStats> GetUserSummary(DateTime date)
    {
        var profile = await GetSocialProfile();

        return await _context.GetAndDeserialize<GarminStats>(
            $"{UserSummaryUrl}{profile.DisplayName}?calendarDate={date:yyy-MM-dd}");
    }

    public async Task<GarminHr> GetWellnessHeartRates(DateTime date)
    {
        var profile = await GetSocialProfile();

        return await _context.GetAndDeserialize<GarminHr>(
            $"{HeartRatesUrl}{profile.DisplayName}?date={date:yyyy-MM-dd}");
    }

    public async Task<GarminSleepData> GetWellnessSleepData(DateTime date)
    {
        var profile = await GetSocialProfile();

        return await _context.GetAndDeserialize<GarminSleepData>(
            $"{SleepDataUrl}{profile.DisplayName}?date={date:yyyy-MM-dd}");
    }

    public Task<GarminBodyComposition> GetBodyComposition(DateTime startDate, DateTime endDate)
    {
        var bodyCompositionUrl =
            $"{BodyCompositionUrl}?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";

        return _context.GetAndDeserialize<GarminBodyComposition>(bodyCompositionUrl);
    }

    public Task<GarminDeviceSettings> GetDeviceSettings(long deviceId)
    {
        var devicesUrl = $"{DeviceServiceUrl}device-info/settings/{deviceId}";

        return _context.GetAndDeserialize<GarminDeviceSettings>(devicesUrl);
    }

    public Task<GarminHydrationData> GetHydrationData(DateTime date)
    {
        var hydrationUrl = $"{HydrationDataUrl}{date:yyyy-MM-dd}";

        return _context.GetAndDeserialize<GarminHydrationData>(hydrationUrl);
    }

    public Task<GarminExcerciseSets> GetActivityExcerciseSets(long activityId)
    {
        var exerciseSetsUrl = $"{ActivityUrl}{activityId}";

        return _context.GetAndDeserialize<GarminExcerciseSets>(exerciseSetsUrl);
    }

    public Task<GarminActivitySplits> GetActivitySplits(long activityId)
    {
        var splitsUrl = $"{ActivityUrl}{activityId}/splits";

        return _context.GetAndDeserialize<GarminActivitySplits>(splitsUrl);
    }

    public Task<GarminSplitSummary> GetActivitySplitSummaries(long activityId)
    {
        var splitSummariesUrl = $"{ActivityUrl}{activityId}/split_summaries";

        return _context.GetAndDeserialize<GarminSplitSummary>(splitSummariesUrl);
    }

    public Task<GarminActivityWeather> GetActivityWeather(long activityId)
    {
        var activityWeatherUrl = $"{ActivityUrl}{activityId}/weather";

        return _context.GetAndDeserialize<GarminActivityWeather>(activityWeatherUrl);
    }

    public Task<GarminHrTimeInZones[]> GetActivityHrInTimezones(long activityId)
    {
        var activityHrTimezoneUrl = $"{ActivityUrl}{activityId}/hrTimeInZones";

        return _context.GetAndDeserialize<GarminHrTimeInZones[]>(activityHrTimezoneUrl);
    }

    public Task<GarminActivityDetails> GetActivityDetails(long activityId, int maxChartSize,
        int maxPolylineSize = 4000)
    {
        var queryParams = $"maxChartSize={maxChartSize}&maxPolylineSize={maxPolylineSize}";
        var detailsUrl = $"{ActivityUrl}{activityId}/details?{queryParams}";

        return _context.GetAndDeserialize<GarminActivityDetails>(detailsUrl);
    }

    public Task<GarminPersonalRecord[]> GetPersonalRecord(string ownerDisplayName)
    {
        var personalRecordsUrl = $"{PersonalRecordUrl}prs/{ownerDisplayName}";

        return _context.GetAndDeserialize<GarminPersonalRecord[]>(personalRecordsUrl);
    }

    public Task<GarminDeviceLastUsed> GetDeviceLastUsed()
    {
        var deviceLastUsedUrl = $"{DeviceServiceUrl}mylastused";

        return _context.GetAndDeserialize<GarminDeviceLastUsed>(deviceLastUsedUrl);
    }

    public async Task<byte[]> DownloadActivity(long activityId, ActivityDownloadFormat format)
    {
        var urls = new Dictionary<ActivityDownloadFormat, string>
        {
            {
                ActivityDownloadFormat.ORIGINAL,
                $"{FitDownloadUrl}{activityId}"
            },
            {
                ActivityDownloadFormat.TCX,
                $"{TcxDownloadUrl}{activityId}"
            },
            {
                ActivityDownloadFormat.GPX,
                $"{GpxDownloadUrl}{activityId}"
            },
            {
                ActivityDownloadFormat.KML,
                $"{KmlDownloadUrl}{activityId}"
            },
            {
                ActivityDownloadFormat.CSV,
                $"{CsvDownloadUrl}{activityId}"
            }
        };
        if (!urls.ContainsKey(format))
        {
            throw new ArgumentException($"Unexpected value {format} for dl_fmt");
        }

        var url = urls[format];

        var response = await _context.MakeHttpGet(url);

        return await response.Content.ReadAsByteArrayAsync();
    }
}