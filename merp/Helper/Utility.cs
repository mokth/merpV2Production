using System;

namespace wincom.mobile.erp
{
	public class Utility
	{/// <summary>
	/// Converts to date.
	/// </summary>
	/// <returns>The to date.</returns>
	/// <param name="sdate">Sdate.</param>
		public static DateTime ConvertToDate(string sdate)
		{
			DateTime date = DateTime.Today;  
			string[] para = sdate.Split(new char[]{'-'});
			if (para.Length > 2) {
				int yy = Convert.ToInt32 (para [2]);
				int mm = Convert.ToInt32 (para [1]);
				int dd = Convert.ToInt32 (para [0]);

				date = new DateTime (yy, mm, dd);
			}

			return date;
		}
		/// <summary>
		/// Gets the date range for 3 months (start from last 3 month).
		/// </summary>
		/// <param name="sdate">Sdate.</param>
		/// <param name="edate">Edate.</param>
		public static void GetDateRange (ref DateTime sdate,ref DateTime edate)
		{
			DateTime today = DateTime.Today;
			sdate = new DateTime (today.Year, today.Month , 1);
			sdate = sdate.AddMonths (-3);
			edate = today.AddMonths (1).AddDays (-1);
		}
	}
}

