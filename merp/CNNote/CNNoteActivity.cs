﻿
using System;
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
	[Activity (Label = "CREDIT NOTE")]			
	public class CNNoteActivity : Activity
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
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			// Create your application here
			SetContentView (Resource.Layout.ListView);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
			Button butNew= FindViewById<Button> (Resource.Id.butnewInv); 
			butNew.Click += butCreateNewInv;
			Button butInvBack= FindViewById<Button> (Resource.Id.butInvBack); 
			butInvBack.Click += (object sender, EventArgs e) => {
				StartActivity(typeof(MainActivity));
			};

			listView.ItemClick += OnListItemClick;
			listView.ItemLongClick += OnListItemLongClick;
			//listView.Adapter = new CusotmListAdapter(this, listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<CNNote> (this, listData, Resource.Layout.ListItemRow, viewdlg);

		}

		public override void OnBackPressed() {
			// do nothing.
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
			var intent = new Intent(this, typeof(CNItemActivity)); //need to change
			intent.PutExtra ("invoiceno",item.cnno );
			intent.PutExtra ("custcode",item.custcode );
			StartActivity(intent);
		}

		void OnListItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e) {
			CNNote item = listData.ElementAt (e.Position);
			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupInv);

			//if (!compinfo.AllowDelete) {
				menu.Menu.RemoveItem (Resource.Id.popInvdelete);
			//}
			if (!compinfo.AllowEdit) {
				menu.Menu.RemoveItem (Resource.Id.popInvedit);
			}
			if (DataHelper.GetCNNotePrintStatus (pathToDatabase, item.cnno)) {
				menu.Menu.RemoveItem (Resource.Id.popInvdelete);
				menu.Menu.RemoveItem (Resource.Id.popInvedit);
			}
			menu.MenuItemClick += (s1, arg1) => {
				if (arg1.Item.TitleFormatted.ToString().ToLower()=="add")
				{
					CreateNewInvoice();
				}else if (arg1.Item.TitleFormatted.ToString().ToLower()=="print")
				{
					PrintInv(item,1);	
				}else if (arg1.Item.TitleFormatted.ToString().ToLower()=="print 2 copy")
				{
					PrintInv(item,2);	

				} else if (arg1.Item.TitleFormatted.ToString().ToLower()=="delete")
				{
					Delete(item);
				}else if (arg1.Item.TitleFormatted.ToString().ToLower()=="edit")
				{
					Edit(item);
				}

			};
			menu.Show ();
		}

		void Edit(CNNote cn)
		{
			var intent = new Intent (this, typeof(EditCNNote));
			intent.PutExtra ("cnno", cn.cnno);
			StartActivity (intent);
		}

		void Delete(CNNote inv)
		{
			var builder = new AlertDialog.Builder(this);
			builder.SetMessage("Confimr to Delete?");
			builder.SetPositiveButton("YES", (s, e) => { DeleteItem(inv); });
			builder.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
			builder.Create().Show();
		}
		void DeleteItem(CNNote inv)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<CNNote>().Where(x=>x.cnno==inv.cnno).ToList<CNNote>();
				if (list.Count > 0) {
					db.Delete (list [0]);
					var arrlist= listData.Where(x=>x.cnno==inv.cnno).ToList<CNNote>();
					if (arrlist.Count > 0) {
						listData.Remove (arrlist [0]);
						SetViewDlg viewdlg = SetViewDelegate;
						listView.Adapter = new GenericListAdapter<CNNote> (this, listData, Resource.Layout.ListItemRow, viewdlg);
					}
				}
			}
		}

		void populate(List<CNNote> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<CNNote>()
					.Where(x=>x.isUploaded==false)
					.OrderByDescending (x => x.cnno)
					.ToList<CNNote>();
				foreach(var item in list2)
				{
					list.Add(item);
				}

			}
			compinfo = DataHelper.GetCompany (pathToDatabase);
		}


		private void butCreateNewInv(object sender, EventArgs e)
		{
			CreateNewInvoice ();
		}

		private void CreateNewInvoice()
		{
			var intent = new Intent(this, typeof(CreateCNNote )); //need to change
			StartActivity(intent);
		}

//		void PrintInvSumm(DateTime printdate1,DateTime printdate2)
//		{
//			mmDevice = null;
//			findBTPrinter ();
//
//			if (mmDevice == null)
//				return;
//			
//			string userid = ((GlobalvarsApp)this.Application).USERID_CODE;
//			PrintInvHelper prnHelp = new PrintInvHelper (pathToDatabase, userid);
//			string msg = prnHelp.PrintInvSumm(mmSocket, mmDevice,printdate1,printdate2);
//			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
//
//		}
		void PrintInv(CNNote inv,int noofcopy)
		{
			//Toast.MakeText (this, "print....", ToastLength.Long).Show ();	
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
				updatePrintedStatus (inv);
				var found =listData.Where (x => x.cnno == inv.cnno).ToList ();
				if (found.Count > 0) {
					found [0].isPrinted = true;
					SetViewDlg viewdlg = SetViewDelegate;
					listView.Adapter = new GenericListAdapter<CNNote> (this, listData, Resource.Layout.ListItemRow, viewdlg);
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

			if (!string.IsNullOrEmpty (inv.invno)) {
				updateInvPrintedStatus(inv.invno);

			}
		}

		void updateInvPrintedStatus(string invno)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<Invoice> ().Where (x => x.invno == invno).ToList<Invoice> ();
				if (list.Count > 0) {
					//if only contains items then allow to update the printed status.
					//this to allow the invoice;s item can be added. if not can not be posted(upload)
					var list2 = db.Table<InvoiceDtls> ().Where (x => x.invno == invno).ToList<InvoiceDtls> ();
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
			string msg =prnHelp.OpenBTAndPrintCN(mmSocket, mmDevice, inv, list,noofcopy);
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
		}

		string getBTAddrFile(string printername)
		{
			var documents = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			string filename = Path.Combine (documents, printername+".baddr");
			return filename;
		}

		bool tryConnectBtAddr(string btAddrfile)
		{
			bool found = false;
			if (!File.Exists (btAddrfile))
				return false;
			try{
			mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
			mmDevice = mBluetoothAdapter.GetRemoteDevice (File.ReadAllBytes (btAddrfile));
			if (mmDevice != null) {
				found = true;
			}
			
			}catch(Exception ex) {
			
			}
			return found;
	     }

		void findBTPrinter(){
			string printername = apara.PrinterName.Trim ().ToUpper ();
//			string addrfile = getBTAddrFile(printername);
//			if (tryConnectBtAddr(addrfile))
//				return;
			
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
//							File.WriteAllText(addrfile,dev.Address);
							break;
						}
					}
				}
				Toast.MakeText (this, "found device " +mmDevice.Name, ToastLength.Long).Show ();	
				//txtv.Text ="found device " +mmDevice.Name;
			}catch(Exception ex) {
				mmDevice = null;
				Toast.MakeText (this, "Error initialize bluetooth Adapter. Try again", ToastLength.Long).Show ();	
			}
		}


	}
}

