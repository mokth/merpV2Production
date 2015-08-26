
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
using Android.Content.PM;
using Android.Bluetooth;

namespace wincom.mobile.erp
{
	[Activity (Label = "SETTINGS")]			
	//[Activity (Label = Resources.GetString(Resource.String.mainmenu_downupload))]			
	public class UtilityActivity : Activity
	{
		
		string pathToDatabase;
		BluetoothAdapter mBluetoothAdapter;
		BluetoothSocket mmSocket;
		BluetoothDevice mmDevice;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Settings);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
		

			Button butSett = FindViewById<Button> (Resource.Id.butsetting);
			butSett.Click += butSetting;
			Button butAbt = FindViewById<Button> (Resource.Id.butAbout);
			butAbt.Click+= ButAbt_Click;
			Button buttestprint =FindViewById<Button> (Resource.Id.buttestprint);
			buttestprint.Click+= Buttestprint_Click;

			Button butback = FindViewById<Button> (Resource.Id.butMain);
			butback.Click+= (object sender, EventArgs e) => {
				base.OnBackPressed();
			};

		
		}

		void Buttestprint_Click (object sender, EventArgs e)
		{
			mmDevice = null;
			findBTPrinter ();
			if (mmDevice != null) {
				string userid = ((GlobalvarsApp)this.Application).USERID_CODE;
				PrintInvHelper prnHelp = new PrintInvHelper (pathToDatabase, userid);
				string msg =prnHelp.OpenBTAndPrintTest(mmSocket, mmDevice);
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}

		void butSetting(object sender,EventArgs e)
		{
			StartActivity (typeof(SettingActivity));
		}

		void ButAbt_Click (object sender, EventArgs e)
		{
			CompanyInfo comp= DataHelper.GetCompany (pathToDatabase);
			View messageView = LayoutInflater.Inflate(Resource.Layout.About, null, false);
			PackageInfo pInfo = PackageManager.GetPackageInfo (PackageName, 0);
			// When linking text, force to always use default color. This works
			// around a pressed color state bug.
			TextView textView = (TextView) messageView.FindViewById(Resource.Id.about_credits);
			TextView textDesc = (TextView) messageView.FindViewById(Resource.Id.about_descrip);
			TextView textVer = (TextView) messageView.FindViewById(Resource.Id.about_ver);
			//textDesc.Text = Html.FromHtml (Resources.GetString(Resource.String.app_descrip))..ToString();
			textView.Text = "For inquiry, please contact " + comp.SupportContat;
			textVer .Text = "Build Version : "+pInfo.VersionName;
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetIcon(Resource.Drawable.Icon);
			builder.SetTitle(Resource.String.app_name);
			builder.SetView(messageView);
			builder.Create();
			builder.Show();
		}
	
		void findBTPrinter(){
			var apara =  DataHelper.GetAdPara (pathToDatabase);
			string printername = apara.PrinterName.Trim ().ToUpper ();
			//			string addrfile = getBTAddrFile(printername);
			//			if (tryConnectBtAddr(addrfile))
			//				return;

			try{
				mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

				if (mBluetoothAdapter ==null)
				{
					Toast.MakeText (this, "Can not Find Bluetooth device,try again.", ToastLength.Long).Show ();	
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
				Toast.MakeText(this, "found device " +mmDevice.Name, ToastLength.Long).Show ();	
				//AlertShow( "found device " +mmDevice.Name);
				//txtv.Text ="found device " +mmDevice.Name;
			}catch(Exception ex) {
				//txtv.Text = ex.Message;
				mmDevice = null;
				Toast.MakeText (this, "Error in Bluetooth device. Try again.", ToastLength.Long).Show ();	
				//AlertShow(ex.Message);
			}
		}
	}
}

