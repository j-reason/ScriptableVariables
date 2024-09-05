using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Variables.Diagnostics
{
	public static class StackLogger
	{

		public enum FunctionType {Get, Set}

		public class LogData
		{
			public DateTime DateTime;
			public StackTrace StackTrace;
			public int CollapsedUsage = 1;
			public FunctionType FunctionType;

			public LogData(FunctionType FunctionType, StackTrace StrackTrace)
			{
				this.FunctionType = FunctionType;
				this.StackTrace = StrackTrace;
				this.DateTime = DateTime.Now;
			}

			public void AddUsage()
			{
				this.DateTime = DateTime.Now;
				CollapsedUsage++;
			}

		}

		private static Dictionary<Variable,List<LogData>> s_AllLogs = new Dictionary<Variable,List<LogData>>();
		private static Dictionary<Variable, Dictionary<string, LogData>> s_collapsedLogs = new Dictionary<Variable, Dictionary<string, LogData>>();

		static readonly Type[] TYPES_TO_IGNORE = { typeof(StackLogger), typeof(Variable), typeof(Reference)};


		public static void LogUsage(Variable variable, FunctionType mode)
		{
		
			if (!s_AllLogs.TryGetValue(variable, out List<LogData> logs))
			{
				logs= new List<LogData>();
                s_AllLogs.Add(variable, logs);
			}

			if (!s_collapsedLogs.TryGetValue(variable, out Dictionary<string, LogData> collapsedLogs))
			{
				collapsedLogs = new Dictionary<string, LogData>();
				s_collapsedLogs.Add(variable, collapsedLogs);
			}


			var st = new StackTrace(3,true);
			var firstFrame = st.GetFrame(0);


			if (!collapsedLogs.TryGetValue(st.ToString(), out var collapsedData))
			{
				collapsedData = new LogData(mode,st);
				collapsedLogs.Add(st.ToString(), collapsedData);
			}
			else
			{
				collapsedData.AddUsage();
			}
			//Debug.Log(PrintStackTrace(st));
			logs.Add(new LogData(mode,st));
		}

		public static List<LogData> GetUsage(Variable variable)
		{
            if(s_AllLogs.TryGetValue(variable, out List<LogData> logs))
				return logs;

			return new List<LogData>();
        }

		public static List<LogData> GetCollapsedUsage(Variable variable)
		{
            if (s_collapsedLogs.TryGetValue(variable, out Dictionary<string, LogData> logs))
                return logs.Values.ToList();

            return new List<LogData>();
        }

		public static void ClearUsage(Variable variable)
		{
			s_AllLogs.Remove(variable);
			s_collapsedLogs.Remove(variable);
		}


		public static string PrintStackTrace(StackTrace ST)
		{
			string retVal = "";

			foreach(StackFrame frame in ST.GetFrames())
			{
				Debug.Log($"{frame.GetMethod().DeclaringType} == {typeof(StackLogger)} = {frame.GetMethod().DeclaringType == typeof(StackLogger)}");
				if (TYPES_TO_IGNORE.Contains(frame.GetMethod().DeclaringType))
					continue;

				
				retVal += StackFrameToString(frame);

				retVal += "\r\n";
			}
			return retVal;
		}

		public static string StackFrameToString(StackFrame frame)
		{
            var methodData = frame.GetMethod();
            return $"{frame.GetMethod().DeclaringType.FullName}:{methodData.Name} (at {frame.GetFileName()}:{frame.GetFileLineNumber()})";
        }

		public static StackFrame[] GetFilteredFrames(this StackTrace stackTrace)
		{
			return stackTrace.GetFrames().Where(p => !TYPES_TO_IGNORE.Any(q => q.IsAssignableFrom(p.GetMethod().DeclaringType))).ToArray();
		}


	} 
}

