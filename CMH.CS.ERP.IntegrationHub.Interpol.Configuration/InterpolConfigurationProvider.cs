using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Configuration
{
    public class InterpolConfigurationProvider : IInterpolConfigurationProvider
    {
        private readonly ISqlProvider _sqlProvider;
        private const string USP_GETINTERPOLCONFIGURATION = "usp_GetInterpolConfiguration";
        private const string USP_GETINTERPOLSCHEDULES = "usp_GetInterpolSchedules";
        private const string USP_GETINTERPOLEXCLUSIONS = "usp_GetInterpolExclusions";
        private readonly string _instanceId;

        public InterpolConfigurationProvider(ISqlProvider sqlProvider, string instanceId)
        {
            _sqlProvider = sqlProvider;
            _instanceId = instanceId;
        }

        public IInterpolConfigurationDbModel GetConfiguration()
        {
            var parameters = new
            {
                InstanceID = new Guid(_instanceId)
            };

            var config = _sqlProvider.QueryStoredProcedure<InterpolConfigurationDbModel>(USP_GETINTERPOLCONFIGURATION, parameters);
            return config.FirstOrDefault();
        }

        public Dictionary<string, IScheduleConfiguration[]> GetSchedules()
        {
            var parameters = new
            {
                InstanceID = new Guid(_instanceId)
            };
            var schedules = _sqlProvider.QueryStoredProcedure<InterpolScheduleDbModel>(USP_GETINTERPOLSCHEDULES, parameters);
            Dictionary<string, IScheduleConfiguration[]> scheduleConfigurations = ProcessScheduleResults(schedules.ToList());

            return scheduleConfigurations;
        }

        public IExclusion[] GetExclusions()
        {
            var parameters = new
            {
                InstanceID = new Guid(_instanceId)
            };
            var exclusions = _sqlProvider.QueryStoredProcedure<InterpolExclusionsDbModel>(USP_GETINTERPOLEXCLUSIONS, parameters);
            List<IExclusion> scheduleExclusions = ProcessExclusions(exclusions.ToList());

            return scheduleExclusions.ToArray();
        }

        private List<IExclusion> ProcessExclusions(List<InterpolExclusionsDbModel> dbExclusions)
        {
            List<IExclusion> exclusions = new List<IExclusion>();
            foreach (var exclusion in dbExclusions)
            {
                var exc = new Exclusion
                {
                    DataType = exclusion.DataType,
                    ExcludedBUs = MapBusinessUnits(exclusion)
                };
                exclusions.Add(exc);
            }
            return exclusions;
        }

        private Dictionary<string, IScheduleConfiguration[]> ProcessScheduleResults(List<InterpolScheduleDbModel> dbSchedules)
        {
            List<IScheduleConfiguration> schedules = new List<IScheduleConfiguration>();
            foreach (var dbSchedule in dbSchedules)
            {
                var schedule = new ScheduleConfiguration
                {
                    BusinessUnits = MapBusinessUnits(dbSchedule),
                    DataTypes = MapDataTypes(dbSchedule),
                    DaysOfWeek = MapDaysOfWeek(dbSchedule),
                    Name = dbSchedule.InstanceKey.ToString(),
                    PollingIntervalMilliseconds = dbSchedule.PollingIntervalMilliseconds
                };
                schedules.Add(schedule);
            }
            Dictionary<string, IScheduleConfiguration[]> scheduleDictionary = new Dictionary<string, IScheduleConfiguration[]>
            {
                [_instanceId] = schedules.ToArray()
            };
            return scheduleDictionary;
        }

        private static DayOfWeek[] MapDaysOfWeek(InterpolScheduleDbModel result)
        {
            List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();
            result?.DaysOfWeek
                ?.Split(',')
                .ToList()
                .ForEach(x =>
                    daysOfWeek.Add(
                        (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x, true)
                    )
                );
            return daysOfWeek.ToArray();
        }

        private static DataTypes[] MapDataTypes(InterpolScheduleDbModel result)
        {
            List<DataTypes> dataTypes = new List<DataTypes>();
            result?.DataTypes
                ?.Split(',')
                .ToList()
                .ForEach(x =>
                    dataTypes.Add(
                        (DataTypes)Enum.Parse(typeof(DataTypes), x, true)
                    )
                );
            return dataTypes.ToArray();
        }

        private static string[] MapBusinessUnits(InterpolDbModelWithBusinessUnits result)
        {
            List<string> businessUnits = new List<string>();
            result?.BusinessUnits
                ?.Split(',')
                .ToList()
                .ForEach(x =>
                    businessUnits.Add(x)
                );
            return businessUnits.ToArray();
        }
    }
}