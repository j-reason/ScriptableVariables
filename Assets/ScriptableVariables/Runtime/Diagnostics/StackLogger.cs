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


		private static Dictionary<Variable,List<StackTrace>> s_AllLogs = new Dictionary<Variable,List<StackTrace>>();

		static readonly Type[] TYPES_TO_IGNORE = { typeof(StackLogger), typeof(Variable) };


		public static void LogUsage(Variable variable, FunctionType mode)
		{
		
			if (!s_AllLogs.TryGetValue(variable, out List<StackTrace> logs))
			{
				logs= new List<StackTrace>();
                s_AllLogs.Add(variable, logs);
			}

			var st = new StackTrace(3,true);

			if (mode == FunctionType.Get)
			{
				Debug.Log($"GET: {variable}");
			}
			else
			{
                Debug.Log($"SET: {variable}");
            }
			//Debug.Log(PrintStackTrace(st));
			logs.Add(st);
		}

		public static List<StackTrace> GetUsage(Variable variable)
		{
            if(s_AllLogs.TryGetValue(variable, out List<StackTrace> logs))
				return logs;

			return new List<StackTrace>();
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




	} 
}

