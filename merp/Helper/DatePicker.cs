using System;
using Android.App;
using Android.OS;
using Android.Views;

namespace wincom.mobile.erp
{
	public delegate void SetDateDlg(string datestring);

	public class DatePicker:DialogFragment
	{
		private SetDateDlg setDateAction;


		public static DatePicker NewInstance(SetDateDlg action)
		{
			var dialogFragment = new DatePicker(action);
			return dialogFragment;
		}

		public DatePicker(SetDateDlg action)
		{
			setDateAction = action;
		}
		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public Dialog CreateDialog(Bundle savedInstanceState) {
			// Use the current date as the default date in the picker
			base.OnCreate(savedInstanceState);
			DateTime date = DateTime.Today;
			int year = date.Year;
			int month = date.Month;
			int day = date.Day;

			// Create a new instance of DatePickerDialog and return it
			return new DatePickerDialog(Activity, OnDateSet, year, month, day);
		}

		void OnDateSet (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			setDateAction.Invoke (e.Date.ToString ("dd-MM-yyyy"));
		}
	}
}

