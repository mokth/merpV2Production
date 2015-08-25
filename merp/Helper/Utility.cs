using System;
using System.Globalization;

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

		public static bool IsValidDateString (string datestr)
		{   
			bool valid = false;
			DateTime sdate;
			CultureInfo culture;
			DateTimeStyles styles;
			culture = CultureInfo.CreateSpecificCulture("en-GB"); 
			styles = DateTimeStyles.None;

			if (DateTime.TryParse (datestr, culture, styles, out sdate)) {
				valid = true;
			}

			return valid;
		}

		public static double AdjusttoNear(double amount,ref double roundVal)
		{
			double amt = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
			double intval = Math.Truncate(amt);
			string decPlaces = amt.ToString("n2");
			string[] digits = decPlaces.Split(new char[] { '.' });
			int num0 = Convert.ToInt32(digits[1][0].ToString());
			int num1 = Convert.ToInt32(digits[1][1].ToString());
			string dec = "0";
			string rdec = "0";
			double diff = 0;
			bool isUp = false;
			if (num1 > 2 && num1 <= 5)
			{
				dec = string.Format("0.{0}{1}", num0, 5);
				rdec = string.Format("0.0{0}", 5-num1);
				isUp = true;
			}
			if (num1 <= 2)
			{
				dec = string.Format("0.{0}{1}", num0, 0);
				rdec = string.Format("0.0{0}",  num1);
				isUp = false;
			}
			if (num1 > 5 && num1 <= 7)
			{
				dec = string.Format("0.{0}{1}", num0, 5);
				rdec = string.Format("0.0{0}", num1-5);
				isUp = false;
			}
			if (num1 > 7)
			{
				dec = string.Format("0.{0}{1}", num0 + 1, 0);
				rdec = string.Format("0.0{0}", 10-num1);
				isUp = true;
			}


			roundVal = Convert.ToDouble(rdec) * ((!isUp)?-1:1);
			double decval = Convert.ToDouble(dec);
			intval = intval + decval;
			return intval;
		}
	}
}

