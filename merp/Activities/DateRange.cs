
using System;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using System.Collections;
using System.Globalization;


namespace wincom.mobile.erp
{
	[Activity (Label = "SELECT DATE RANGE")]			
	public class DateRange : Activity
	{
		
		const int DATE_DIALOG_ID1 = 0;
		const int DATE_DIALOG_ID2 = 1;
		DateTime date;
		int EVENTID;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.DateRange);
			EVENTID = Convert.ToInt32(Intent.GetStringExtra ("eventid"));
			Button butOk= FindViewById<Button> (Resource.Id.butok); 
			EditText frd = FindViewById<EditText> (Resource.Id.trxdatefr);
			EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			frd.Text = DateTime.Today.ToString ("dd-MM-yyyy");
			frd.Click += delegate(object sender, EventArgs e) {
				ShowDialog (DATE_DIALOG_ID1);
			};
			tod.Text = DateTime.Today.ToString ("dd-MM-yyyy");
			date =  DateTime.Today;
			tod.Click += delegate(object sender, EventArgs e) {
				
				ShowDialog (DATE_DIALOG_ID2);
			};

			butOk.Click+= ButPrint_Click;
		}
		protected override Dialog OnCreateDialog (int id)
		{
			switch (id) {
			case DATE_DIALOG_ID1:
				return new DatePickerDialog (this, OnDateSet1, date.Year, date.Month - 1, date.Day); 
			case DATE_DIALOG_ID2:
				return new DatePickerDialog (this, OnDateSet2, date.Year, date.Month - 1, date.Day); 
			}
			return null;
		}
		void OnDateSet1 (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			EditText frd = FindViewById<EditText> (Resource.Id.trxdatefr);
			//EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			frd.Text = e.Date.ToString("dd-MM-yyyy");
		}
		void OnDateSet2 (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			tod.Text = e.Date.ToString("dd-MM-yyyy");
		}

		void ButPrint_Click (object sender, EventArgs e)
		{
			bool valid = true;
			EditText frd = FindViewById<EditText> (Resource.Id.trxdatefr);
			EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			DateTime sdate;
			DateTime edate;
			CultureInfo culture;
			DateTimeStyles styles;
			culture = CultureInfo.CreateSpecificCulture("en-GB"); 

			styles = DateTimeStyles.None;

			if (!DateTime.TryParse (frd.Text, culture, styles, out sdate)) {
				valid = false;
			}

			if (!DateTime.TryParse (tod.Text, culture, styles, out edate)) {
				valid = false;
			}

			if (valid)
			{
				FireEvent (EVENTID, frd.Text, tod.Text);
				base.OnBackPressed();
			}
		}


		void FireEvent (int eventID,string startD,string endD)
		{
			Hashtable param = new Hashtable ();
			param.Add ("DATE1", startD);
			param.Add ("DATE2", endD);
			EventParam p = new EventParam (eventID, param);
			EventManagerFacade.Instance.GetEventManager ().PerformEvent (CallingActivity, p);
		}
	}
}

