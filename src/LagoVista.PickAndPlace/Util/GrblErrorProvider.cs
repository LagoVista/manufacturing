﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using LagoVista.Core.PlatformSupport;
using System.Threading.Tasks;
using LagoVista.IoT.Logging;

namespace LagoVista.PickAndPlace.Util
{
    public class GrblErrorProvider
    {
        Dictionary<int, string> _errors;
        Dictionary<int, string> _alarms;

        private GrblErrorProvider()
        {
            _errors = new Dictionary<int, string>();
            _alarms = new Dictionary<int, string>();
        }

        private async Task LoadErrorFileAsync()
        {
            Services.Logger.AddCustomEvent(LogLevel.Message, "GrblErrorProvider_Init", "Loading GRBL Error Database");

            var errors = await Services.Storage.ReadAllTextAsync(Constants.FilePathErrors);
            var alarms = await Services.Storage.ReadAllTextAsync(Constants.FilePathAlarmCodes);

            Regex LineParser = new Regex(@"([0-9]+)\t([^\n^\r]*)");     //test here https://www.regex101.com/r/hO5zI1/2

            MatchCollection mc = LineParser.Matches(errors);

            foreach (Match m in mc)
            {
                int errorNo = int.Parse(m.Groups[1].Value);

                _errors.Add(errorNo, m.Groups[2].Value);
            }

            mc = LineParser.Matches(alarms);

            foreach (Match m in mc)
            {
                int errorNo = int.Parse(m.Groups[1].Value);

                _alarms.Add(errorNo, m.Groups[2].Value);
            }

            Services.Logger.AddCustomEvent(LogLevel.Message, "GrblErrorProvider_Init", "Loaded GRBL Error Database");
        }

        public string GetErrorMessage(int errorCode)
        {
            if (_errors.ContainsKey(errorCode))
                return _errors[errorCode];
            else
                return $"Unknown Error: {errorCode}";
        }

        public string GetAlarmCode(int alarmCode)
        {
            if (_alarms.ContainsKey(alarmCode))
                return _alarms[alarmCode];
            else
                return $"Unknown Alarm Code: {alarmCode}";
        }

        static Regex ErrorExp = new Regex(@"Invalid gcode ID:(\d+)");
        private string ErrorMatchEvaluator(Match m)
        {
            return GetErrorMessage(int.Parse(m.Groups[1].Value));
        }

        public string ExpandError(string error)
        {
            return ErrorExp.Replace(error, ErrorMatchEvaluator);
        }

        static GrblErrorProvider _instance;

        public static GrblErrorProvider Instance
        {
            get
            {
                if(_instance == null)
                {
                    throw new Exception("Please call static init method first.");
                }

                return _instance;
            }
        }

        public async static Task InitAsync()
        {
            _instance = new GrblErrorProvider();
            await _instance.LoadErrorFileAsync();
        }


    }
}
