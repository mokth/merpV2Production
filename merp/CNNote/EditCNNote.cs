using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;

namespace wincom.mobile.erp
{
	[Activity (Label = "EDIT CREDIT NOTE")]			
	public class EditCNNote : Activity,IEventListener
	{
		string pathToDatabase;
		List<Trader> items = null;
		ArrayAdapter<String> dataAdapter;
		DateTime _date ;
		AdPara apara = null;
		Spinner spinner;
		string CNNO;
		CNNote cnInfo;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.CreateCNote);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;

			CNNO = Intent.GetStringExtra ("cnno") ?? "AUTO";
			cnInfo =DataHelper.GetCNNote (pathToDatabase, CNNO);

			// Create your application here
			_date = DateTime.Today;
			spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);

			Button butSave = FindViewById<Button> (Resource.Id.newinv_bsave);
			butSave.Text = "SAVE";
			Button butCancel = FindViewById<Button> (Resource.Id.newinv_cancel);
			Button butFind = FindViewById<Button> (Resource.Id.newinv_bfind);
			Button butFindInv = FindViewById<Button> (Resource.Id.newinv_bfindinv);
			spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
			butSave.Click += butSaveClick;
			butCancel.Click += butCancelClick;
			TextView cnno =  FindViewById<TextView> (Resource.Id.newinv_no);
			cnno.Enabled = false;
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			trxdate.Click += delegate(object sender, EventArgs e) {
				ShowDialog (0);
			};
			butFind.Click+= (object sender, EventArgs e) => {
				ShowCustLookUp();
			};

			butFindInv.Click+= (object sender, EventArgs e) => {
				ShowInvLookUp();
			};


			apara =  DataHelper.GetAdPara (pathToDatabase);
			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				items = db.Table<Trader> ().ToList<Trader> ();
			}

			List<string> icodes = new List<string> ();
			foreach (Trader item in items) {
				icodes.Add (item.CustCode+" | "+item.CustName);
			}
			dataAdapter = new ArrayAdapter<String>(this,Resource.Layout.spinner_item, icodes);
			// Drop down layout style - list view with radio button
			dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			// attaching data adapter to spinner
			spinner.Adapter =dataAdapter;
			LoadData ();
		}

		private void LoadData()
		{
			
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			DateTime invdate = Utility.ConvertToDate (trxdate.Text);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			TextView custname = FindViewById<TextView> (Resource.Id.newinv_custname);
			TextView cninvno =  FindViewById<TextView> (Resource.Id.newcninv_no);
			TextView cnno =  FindViewById<TextView> (Resource.Id.newinv_no);

			trxdate.Text = cnInfo.invdate.ToString ("dd-MM-yyyy");
			int pos1= dataAdapter.GetPosition (cnInfo.custcode+" | "+cnInfo.description);
			spinner.SetSelection (pos1);
			custname.Text = cnInfo.description;
			cninvno.Text = cnInfo.invno;
			cnno.Text = cnInfo.cnno;
		}


		public override void OnBackPressed() {
			// do nothing.
		}

		private void butSaveClick(object sender,EventArgs e)
		{
			
			if (SaveCN ()) {
				base.OnBackPressed();
			}

		}

		private void butCancelClick(object sender,EventArgs e)
		{
			base.OnBackPressed();
		}

		private void spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;

			string txt = spinner.GetItemAtPosition (e.Position).ToString();
			string[] codes = txt.Split (new char[]{ '|' });
			if (codes.Length == 0)
				return;

			Trader item =items.Where (x => x.CustCode ==codes[0].Trim()).FirstOrDefault ();
			if (item != null) {
				TextView name = FindViewById<TextView> (Resource.Id.newinv_custname);
				name.Text = item.CustName;
			}

		}
		[Obsolete]
		protected override Dialog OnCreateDialog (int id)
		{
			return new DatePickerDialog (this, HandleDateSet, _date.Year,
				_date.Month - 1, _date.Day);
		}

		void HandleDateSet (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			_date = e.Date;
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			trxdate.Text = _date.ToString ("dd-MM-yyyy");
		}


		private bool SaveCN()
		{
			bool IsSave = false;
			CNNote inv = new CNNote ();
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			DateTime invdate = Utility.ConvertToDate (trxdate.Text);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			TextView custname = FindViewById<TextView> (Resource.Id.newinv_custname);
			TextView cninvno =  FindViewById<TextView> (Resource.Id.newcninv_no);
			if (spinner.SelectedItem == null) {
				Toast.MakeText (this, "No Customer code selected...", ToastLength.Long).Show ();			
				spinner.RequestFocus ();
				return IsSave;			
			}
			string[] codes = spinner.SelectedItem.ToString().Split (new char[]{ '|' });
			if (codes.Length == 0)
				return IsSave;

			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {

				cnInfo.invdate = invdate;
				cnInfo.created = DateTime.Now;
				cnInfo.description = custname.Text;
				cnInfo.custcode = codes [0].Trim ();
				cnInfo.invno = cninvno.Text;
				cnInfo.trxtype = "";
				if (!string.IsNullOrEmpty (cnInfo.invno)) {
					Invoice invInfo = DataHelper.GetInvoice (pathToDatabase,cnInfo.invno);
					if (invInfo != null) {
						cnInfo.trxtype = invInfo.trxtype;	   		
					}
				}
				db.Update (cnInfo);
				IsSave = true;
			}

			return IsSave;
		}

		void ShowCustLookUp()
		{
			var dialog = TraderDialog.NewInstance();
			dialog.Show(FragmentManager, "dialog");
		}

		void ShowInvLookUp()
		{
			var dialog = InvoiceDialog.NewInstance();
			dialog.Show(FragmentManager, "dialog");
		}

		void SetSelectedItem(string text)
		{
			int position=dataAdapter.GetPosition (text);
			spinner.SetSelection (position);
		}

		void SetSelectedInvoice(string text)
		{
			TextView invno =  FindViewById<TextView> (Resource.Id.newcninv_no);
			invno.Text = text;
		}

		public event nsEventHandler eventHandler;

		public void FireEvent(object sender,EventParam eventArgs)
		{
			if (eventHandler != null)
				eventHandler (sender, eventArgs);
		}

		public void PerformEvent(object sender, EventParam e)
		{
			switch (e.EventID) {
			case EventID.CUSTCODE_SELECTED :
				RunOnUiThread (() => SetSelectedItem(e.Param["SELECTED"].ToString()));
				break;
			case EventID.INVNO_SELECTED:
				RunOnUiThread (() => SetSelectedInvoice(e.Param["SELECTED"].ToString()));
				break;
			}
		}
	}
}

