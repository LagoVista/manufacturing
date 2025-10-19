// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fa7c58d4d79ab755ddeecc7ba017e25b8490fd59d43917f13276ee392bea3085
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Parser
{
	class ParseException : Exception
	{
		public int Line;
		public string Error;

		public ParseException(string error, int line)
		{
			Line = line;
			Error = error;
		}

		public override string Message
		{
			get
			{
				return $"Error while reading GCode File in Line {Line}:\n{Error}";
			}
		}
	}
}
