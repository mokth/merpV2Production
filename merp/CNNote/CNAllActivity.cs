﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Bluetooth;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using SQLite;
using System.Threading;
using Java.Util;

namespace wincom.mobile.erp
{
	[Activity (Label = "CREDIT NOTE LIST")]			
	public class CNAllActivity : Activity,IEventListener
	{
		ListView listView ;
		List<CNNote> listData = new List<CNNote> ();
		string pathToDatabase;
		BluetoothAdapter mBluetoothAdapter;
		BluetoothSocket mmSocket;
		BluetoothDevice mmDevice;
		//Thread workerThread;
		Stream mmOutputStream;
		AdPara apara=null;
		CompanyInfo compinfo;
		DateTime sdate;
		DateTime edate;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			// Create your application here
			SetContentView (Resource.Layout.ListView);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			Utility.GetDateRange (ref sdate,ref edate);
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
//			TableLayout tlay = FindViewById<TableLayout> (Resource.Id.tableLayout1);
//			tlay.Visibility = ViewStates.Invisible;
			Button butNew= FindViewById<Button> (Resource.Id.butnewInv); 
			butNew.Visibility = ViewStates.Invisible;
			Button butInvBack= FindViewById<Button> (Resource.Id.butInvBack); 
			butInvBack.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(TransListActivity));
			};

			listView.ItemClick += OnListItemClick;
			listView.ItemLongClick += OnListItemLongClick;
			//listView.Adapter = new CusotmListAdapter(this, listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<CNNote> (this, listData, Resource.Layout.ListItemRow, viewdlg);

		}

		private void SetViewDelegate(View view,object clsobj)
		{
			CNNote item = (CNNote)clsobj;
			view.FindViewById<TextView> (Resource.Id.invdate).Text = item.invdate.ToString ("dd-MM-yy");
			view.FindViewById<TextView> (Resource.Id.invno).Text = item.cnno;
			view.FindViewById<TextView> (Resource.Id.trxtype).Text = item.trxtype;
			view.FindViewById<TextView>(Resource.Id.invcust).Text = item.description;
			//view.FindViewById<TextView> (Resource.Id.Amount).Text = item.amount.ToString("n2");
			view.FindViewById<TextView> (Resource.Id.TaxAmount).Text = item.taxamt.ToString("n2");
			double ttl = item.amount + item.taxamt;
			view.FindViewById<TextView> (Resource.Id.TtlAmount).Text =ttl.ToString("n2");
			ImageView img = view.FindViewById<ImageView> (Resource.Id.printed);
			if (!item.isPrinted)
				img.Visibility = ViewStates.Invisible;
		}

		protected override void OnResume()
		{
			base.OnResume();
			listData = new List<CNNote> ();
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<CNNote> (this, listData, Resource.Layout.ListItemRow, viewdlg);
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
			CNNote item = listData.ElementAt (e.Position);
			var intent = new Intent(this, typeof(CNItemHisActivity));
			intent.PutExtra ("invoiceno",item.cnno );
			intent.PutExtra ("custcode",item.custcode );
			StartActivity(intent);
		}

		void OnListItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e) {
			CNNote item = listData.ElementAt (e.Position);
			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupHis);
			menu.Menu.RemoveItem (Resource.Id.popInvdelete);
			menu.Menu.RemoveItem (Resource.Id.popInvadd);
			menu.MenuItemClick += (s1, arg1) => {
				
				if (arg1.Item.TitleFormatted.ToString ().ToLower () == "print") {
					PrintInv (item,1);	
				}else if (arg1.Item.TitleFormatted.ToString ().ToLower () == "print 2 copy") {
					PrintInv (item,2);	
				}else if (arg1.Item.TitleFormatted.ToString ().ToLower () == "filter") {
					ShowDateRangeLookUp();
				}  
			};
			menu.Show ();
		}

		void ShowDateRangeLookUp()
		{
			var intent = new Intent (this, typeof(DateRange));
			intent.PutExtra ("eventid", "2021");
			StartActivity (intent);
		}

		void populate(List<CNNote> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<CNNote>()
					.Where(x=>x.isUploaded==true&&x.invdate>=sdate&&x.invdate<=edate)
					.OrderByDescending (x => x.cnno)
					.ToList<CNNote>();
				foreach(var item in list2)
				{
					list.Add(item);
				}

			}
			compinfo = DataHelper.GetCompany (pathToDatabase);
		}


		void PrintInv(CNNote inv,int noofcopy)
		{
			Toast.MakeText (this, "print....", ToastLength.Long).Show ();	
			CNNoteDtls[] list;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var ls= db.Table<CNNoteDtls> ().Where (x => x.cnno==inv.cnno).ToList<CNNoteDtls>();
				list = new CNNoteDtls[ls.Count];
				ls.CopyTo (list);
			}
			mmDevice = null;
			findBTPrinter ();
			if (mmDevice != null) {
				StartPrint (inv, list,noofcopy);
				if (!inv.isPrinted) {
					updatePrintedStatus (inv);
					var found =listData.Where (x => x.cnno == inv.cnno).ToList ();
					if (found.Count > 0) {
						found [0].isPrinted = true;
						SetViewDlg viewdlg = SetViewDelegate;
						listView.Adapter = new GenericListAdapter<CNNote> (this, listData, Resource.Layout.ListItemRow, viewdlg);
					}
				}
			}
		}

		void updatePrintedStatus(CNNote inv)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<CNNote> ().Where (x => x.cnno == inv.cnno).ToList<CNNote> ();
				if (list.Count > 0) {
					var list2 = db.Table<CNNoteDtls> ().Where (x => x.cnno == inv.cnno).ToList<CNNoteDtls> ();
					if (list2.Count > 0) {
						list [0].isPrinted = true;
						db.Update (list [0]);
					}
				}
			}
		}


		void StartPrint(CNNote inv,CNNoteDtls[] list,int noofcopy )
		{
			string userid = ((GlobalvarsApp)this.Application).USERID_CODE;
			PrintInvHelper prnHelp = new PrintInvHelper (pathToDatabase, userid);
			string msg =prnHelp.OpenBTAndPrintCN (mmSocket, mmDevice, inv, list,noofcopy);
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
		}

		void findBTPrinter(){
			string printername = apara.PrinterName.Trim ().ToUpper ();
			try{
				mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
				if (mBluetoothAdapter==null)
				{
					Toast.MakeText (this, "Error initialize bluetooth Adapter. Try again", ToastLength.Long).Show ();
					return;					
				}
				string txt ="";
				if (!mBluetoothAdapter.Enable()) {
					Intent enableBluetooth = new Intent(
						BluetoothAdapter.ActionRequestEnable);
					StartActivityForResult(enableBluetooth, 0);
				}


				var pair= mBluetoothAdapter.BondedDevices;
				if (pair.Count > 0) {
					foreach (BluetoothDevice dev in pair) {
						Console.WriteLine (dev.Name);
						txt = txt+","+dev.Name;
						if (dev.Name.ToUpper()==printername)
						{
							mmDevice = dev;
							break;
						}
					}
				}
				Toast.MakeText (this, "found device " +mmDevice.Name, ToastLength.Long).Show ();	
				//txtv.Text ="found device " +mmDevice.Name;
			}catch(Exception ex) {
				mmDevice = null;
				//txtv.Text = ex.Message;
				//Toast.MakeText (this, ex.Message, ToastLength.Long).Show ();	
				Toast.MakeText (this, "Error initialize bluetooth Adapter. Try again", ToastLength.Long).Show ();	
			}
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
			case 2021:
				sdate =Utility.ConvertToDate(e.Param ["DATE1"].ToString ());
				edate=Utility.ConvertToDate(e.Param ["DATE2"].ToString ());
				break;
			}
		}

	}
}

