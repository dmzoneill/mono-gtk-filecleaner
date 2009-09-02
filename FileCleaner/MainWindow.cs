using System;
using Gtk;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

public partial class MainWindow: Gtk.Window
{	
	private TextBuffer errorMsg;
	private TextBuffer outputMsg;
	private TextBuffer regexMsg;
	private System.Threading.Thread runner;
	private ArrayList regexs = new ArrayList();
	private bool test = false;
	private ListStore pStore;
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		
		this.label1.ModifyFont(Pango.FontDescription.FromString("Sans 8"));
		this.label2.ModifyFont(Pango.FontDescription.FromString("Sans 8"));
		this.label3.ModifyFont(Pango.FontDescription.FromString("Sans 8"));
		this.textview1.ModifyFont(Pango.FontDescription.FromString("Sans 7"));
		this.textview2.ModifyFont(Pango.FontDescription.FromString("Sans 7"));
		this.textview1.ModifyFg(StateType.Normal, new Gdk.Color(0x22,0x22,0x22));
		this.textview2.ModifyFg(StateType.Normal, new Gdk.Color(0xcc,0x00,0x00));
		
		this.filechooserbutton1.Title = "Select folder";
		this.filechooserbutton1.Action = FileChooserAction.SelectFolder;	
		
		outputMsg = this.textview1.Buffer;
		errorMsg = this.textview2.Buffer;	
		regexMsg = this.textview3.Buffer;
		this.runner = new System.Threading.Thread (new System.Threading.ThreadStart (this.startCleaner));
		
		string tags = "dvdrip,720p,axxo,xvid,hdtv,x264,DTS, eng ,fxm,DualAudio,DualAudio,zektrom,Donatello,fxg,TELESYNC,stg,DVDSCR,NoGrp,LTT,DivX,NoRARs,HDRip, REPACK ,™,',subs,div3,Andrei Tarkovsky, TBMs,R5,DUQA, www , makingoff , org , iTALiAN , MD , TS , MoON , kamera , ac3 ,bestdivx,!,title0, kg ,engsub1, lol ";
		
		regexMsg.Text = tags;	
		
		
		TreeViewColumn numColumn = new TreeViewColumn ();
		numColumn.Title = "No.";
		CellRendererText numCell = new CellRendererText ();
  		numColumn.PackStart (numCell, true);
		numColumn.AddAttribute (numCell,"text",0);
		
		TreeViewColumn partternColumn = new TreeViewColumn ();
		partternColumn.Title = "Pattern";
		CellRendererText partternCell = new CellRendererText ();
  		partternColumn.PackStart (partternCell, true);
		partternColumn.AddAttribute (partternCell, "text", 1);

		
		TreeViewColumn replacementColumn = new TreeViewColumn ();
		replacementColumn.Title = "Replacement";
		CellRendererText replacementCell = new CellRendererText ();
  		replacementColumn.PackStart (replacementCell, true);
		replacementColumn.AddAttribute (replacementCell, "text", 2);		
		
		this.treeview1.AppendColumn (numColumn);
		this.treeview1.AppendColumn (partternColumn);
		this.treeview1.AppendColumn (replacementColumn);

		this.pStore = new ListStore (typeof(Int16), typeof (string), typeof (string));

		this.treeview1.Model = this.pStore;
		
		foreach(string reg in tags.Split(','))
		{
			this.pStore.AppendValues(0,reg," ");	
		}
		
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	
	
	public ArrayList getSubDirectories(ArrayList directories, string directory)
	{
		DirectoryInfo dir = new DirectoryInfo(directory);	
 		DirectoryInfo[] dirs = dir.GetDirectories();
		directories.Add(dir.FullName);
		
		foreach(DirectoryInfo instance in dirs)
		{
			this.getSubDirectories(directories, instance.FullName);			
		}	
		
		return directories;
	}
	
	
	public void startCleaner()
	{						
		if(this.filechooserbutton1.Uri.Length < 1)
		{
			Application.Invoke( delegate {
			errorMsg.Text += "Select a directory first\n";
			});
			return;
		}
		
		ArrayList alldirs;	
		string[] allDirsArray;
		
		if(this.checkbutton1.Active == true)
		{
			alldirs = this.getSubDirectories(new ArrayList(),this.filechooserbutton1.Filename);
			allDirsArray = (string[])alldirs.ToArray(typeof(String));
		}
		else
		{
			allDirsArray = new string[1];
			allDirsArray[0] = this.filechooserbutton1.Filename;
		}
		
		ArrayList lstFiles = new ArrayList();
		DirectoryInfo dir;
 		FileInfo[] files;
		
		foreach(string dirFile in allDirsArray)
		{
			dir = new DirectoryInfo(dirFile);
			files = dir.GetFiles();	
			
			foreach(FileInfo file in files)
			{		
				if(!file.Name.StartsWith("."))
				{
					lstFiles.Add(file.FullName);
				}			
			}
		} 		
		
		lstFiles.Sort();
		
		string wdir = this.filechooserbutton1.Filename;
		FileInfo finfo;
		string temp;
		string analysis;
		int count = 0;
		
		foreach(string file in lstFiles)
		{		
			finfo = new FileInfo(file);
			wdir = finfo.DirectoryName;
			temp = cleanFilename(finfo.Name);
			
			if(temp.CompareTo(finfo.Name)==0)
				continue;
			
			try
			{			
				if(this.test==false)	
				{
					File.Move(wdir + "/" + finfo.Name, wdir + "/" + temp);
				}
				Application.Invoke( delegate {	
					outputMsg.Text += wdir + "/" + finfo.Name + " - > " + wdir + "/" + temp + "\n";
					this.textview1.ScrollToIter(outputMsg.EndIter,0.0,true,0.0,0.0);
				}
				);
				count++;
				
			}
			catch(Exception e)
			{
				Application.Invoke( delegate {				
					errorMsg.Text += "Unable to rename\n";
					errorMsg.Text += finfo.Name + " - > " + cleanFilename(finfo.Name) + "\n";
					errorMsg.Text += e.Message.ToString() + "\n";
					this.textview2.ScrollToIter(errorMsg.EndIter,0.0,true,0.0,0.0);
				}
				);
				
			}
			System.Threading.Thread.Sleep(40);
		}
		
		string txt = "";
		
		if(this.test==true)
			txt = " ( test run )";
		else
			txt = "";
		
		if(count==0)
			analysis = "0 files renamed " + txt;
		else if(count==1)
			analysis = "1 file renamed " + txt;
		else
			analysis = count + " files renamed " + txt;
		
		
		Application.Invoke( delegate {
			this.filechooserbutton1.Sensitive = true;
			this.button5.Label = "Clean filenames";
			outputMsg.Text += "Job finished ( " + analysis + " ) \n";
			this.textview1.ScrollToIter(outputMsg.EndIter,0.0,true,0.0,0.0);
			this.textview2.ScrollToIter(errorMsg.EndIter,0.0,true,0.0,0.0);
		});		
		
	}
	
	
	public string cleanFilename(string file)
	{				
		int start;
		int end;
		int count;
		
		file = file.Replace(".", " ");
		file = file.Replace("-", " ");
		file = file.Replace(".", " ");
		file = file.Replace("_", " ");
		file = file.Replace(",", " ");
		file = file.Replace("'", "");
		
		if(file.Contains("[") && file.Contains("]"))
		{
			while(file.Contains("["))
			{
				start = file.IndexOf("[");
				end = file.IndexOf("]");
				count = end - start;
				count++;
				file = file.Remove(start, count);	
			}
		}
		
		if(file.Contains("(") && file.Contains(")"))
		{
			while(file.Contains("("))
			{			
				start = file.IndexOf("(");
				end = file.IndexOf(")");
				count = end - start;
				count++;
				file = file.Remove(start, count);			
			}
		}
		
		foreach(string item in this.regexs)
		{		
			file = Regex.Replace(file, item, " ", RegexOptions.IgnoreCase);
		}
				
		file = Regex.Replace(file, " +", " ", RegexOptions.IgnoreCase);	
		
		if(file.StartsWith(" "))
		{
			file = file.Substring(1,file.Length -1);	
		}

		
		int lastspace = file.LastIndexOf(" ");
		char[] chars = file.ToCharArray();
		if(lastspace>-1)
			chars[lastspace] = '.';
		
		file = new string(chars);
		file = file.ToLower();
		string first = file.Substring(0,1);
		first = first.ToUpper();
		
		file = file.Substring(1, file.Length -1);
		file = first + file;
		
		return file;	
	}


	protected virtual void OnRunClicked (object sender, System.EventArgs e)
	{
		
		if(sender.Equals(this.button2))
			this.test = true;
		else
			this.test = false;
		
		
		
			
		
		if(this.runner.IsAlive)
		{
			try
			{
				this.runner.Abort();
				this.button5.Label = "Clean filenames";
				this.filechooserbutton1.Sensitive = true;
			}
			catch(Exception exc)
			{
				this.errorMsg.Text += exc.ToString() + "\n";	
				this.button5.Label = "Clean filenames";
				this.filechooserbutton1.Sensitive = true;
			}
		}
		else
		{
			
			this.regexs.Clear();
			
			string[] items = this.regexMsg.Text.Split(',');
		
			foreach(string item in items)
			{
				this.regexs.Add(item);	
			}	
			
			this.outputMsg.Text += "Starting job\n";
			this.runner = new System.Threading.Thread (new System.Threading.ThreadStart (this.startCleaner));
			runner.Start();	
			this.button5.Label = "Stop";
			this.filechooserbutton1.Sensitive = false;
		}

	}

	protected virtual void OnButton3Clicked (object sender, System.EventArgs e)
	{
		outputMsg.Text = "";
	}

	protected virtual void OnButton4Clicked (object sender, System.EventArgs e)
	{
		errorMsg.Text = "";
	}

	protected virtual void OnDestroyEvent (object o, Gtk.DestroyEventArgs args)
	{
		args.RetVal = true;
		Application.Quit();
	}
}