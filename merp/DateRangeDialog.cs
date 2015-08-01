using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.IO;
using Android.Graphics.Drawables;
using System.Collections;
using Android.Graphics;

namespace wincom.mobile.erp
{
	public class DateRangeDialog : DialogFragment
	{
		DatePicker datePicker1;
		DatePicker datePicker2;
		SetDateDlg setdateAction1;
		SetDateDlg setdateAction2;

		View dateView;
		string _selectedItem;
	    
		public static DateRangeDialog NewInstance()
		{
			var dialogFragment = new DateRangeDialog();
			return dialogFragment;
		}


		public string SelectedItem
		{
			get { return _selectedItem;}
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}
		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{ 
			base.OnCreate(savedInstanceState);
			// Begin building a new dialog.
			var builder = new AlertDialog.Builder(Activity);

			Rect displayRectangle = new Rect();
			Window window = Activity.Window;
			window.DecorView.GetWindowVisibleDisplayFrame(displayRectangle);

			var inflater = Activity.LayoutInflater;
			dateView = inflater.Inflate(Resource.Layout.DateRange, null);
			dateView.SetMinimumHeight((int)(displayRectangle.Width() * 0.9f));
			dateView.SetMinimumHeight((int)(displayRectangle.Height() * 0.9f));

			Button butOk = dateView.FindViewById<Button> (Resource.Id.butok);  
			butOk.Click+= ButOk_Click;
			datePicker1 = DatePicker.NewInstance(SetDateDlg1);
			datePicker2 =  DatePicker.NewInstance(SetDateDlg2);
			EditText frd = dateView.FindViewById<EditText> (Resource.Id.trxdatefr);
			EditText tod = dateView.FindViewById<EditText> (Resource.Id.trxdateto);
			frd.Text = DateTime.Today.ToString ("dd-MM-yyyy");
			frd.Click += delegate(object sender, EventArgs e) {
				datePicker1.Show(FragmentManager, "datePicker");
			};
			tod.Text = DateTime.Today.ToString ("dd-MM-yyyy");
			tod.Click += delegate(object sender, EventArgs e) {
				datePicker2.Show(FragmentManager, "datePicker2");
			};

			builder.SetView (dateView);
			builder.SetPositiveButton ("CANCEL", HandlePositiveButtonClick);
			var dialog = builder.Create();
			//Now return the constructed dialog to the calling activity
			return dialog;
		}

		public void SetDateDlg1(string datestring)
		{
			EditText frd = dateView.FindViewById<EditText> (Resource.Id.trxdatefr);
			frd.Text = datestring;
		}

		public void SetDateDlg2(string datestring)
		{
			EditText tod = dateView.FindViewById<EditText> (Resource.Id.trxdateto);
			tod.Text = datestring;
		}
//		protected override Dialog Activity.OnCreateDialog (int id)
//		{
//			switch (id) {
//			case DATE_DIALOG_ID1:
//				return new DatePickerDialog (this, OnDateSet1, date.Year, date.Month - 1, date.Day); 
//			case DATE_DIALOG_ID2:
//				return new DatePickerDialog (this, OnDateSet2, date.Year, date.Month - 1, date.Day); 
//			}
//			return null;
//		}

		void OnDateSet1 (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			EditText frd = dateView.FindViewById<EditText> (Resource.Id.trxdatefr);
			//EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			frd.Text = e.Date.ToString("dd-MM-yyyy");
		}
		void OnDateSet2 (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			EditText tod = dateView.FindViewById<EditText> (Resource.Id.trxdateto);
			tod.Text = e.Date.ToString("dd-MM-yyyy");
		}


		private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
		{
			var dialog = (AlertDialog) sender;
			dialog.Dismiss();
		}

		void ButOk_Click (object sender, EventArgs e)
		{
			EditText frd = dateView.FindViewById<EditText> (Resource.Id.trxdatefr);
			EditText tod = dateView.FindViewById<EditText> (Resource.Id.trxdateto);
			_selectedItem = frd.Text + "|" + tod.Text;

		}
		private void  HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
		{
			var dialog = (AlertDialog) sender;
			dialog.Dismiss();
		}


	}
}

