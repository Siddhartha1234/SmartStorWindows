using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Web;
using System.Security.AccessControl;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Process p;
        List<String> data = new List<string>();
        List<Panel> listpanel = new List<Panel>();
        public String[] dirs = Environment.GetLogicalDrives();       
        private BackgroundWorker bw = new BackgroundWorker();
        private BackgroundWorker mworker = new BackgroundWorker();
        private BackgroundWorker sworker = new BackgroundWorker();
        
       
        public Form1()
        {
            InitializeComponent();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            mworker.WorkerReportsProgress = true;
            mworker.WorkerSupportsCancellation = true;
            mworker.DoWork += new DoWorkEventHandler(mworker_DoWork);
            mworker.ProgressChanged += new ProgressChangedEventHandler(mworker_ProgressChanged);
            mworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(mworker_RunWorkerCompleted);
            sworker.WorkerReportsProgress = true;
            sworker.WorkerSupportsCancellation = true;
            sworker.DoWork += new DoWorkEventHandler(sworker_DoWork);
            sworker.ProgressChanged += new ProgressChangedEventHandler(sworker_ProgressChanged);
            sworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(sworker_RunWorkerCompleted);


        }
        private delegate void myUpdate(int progress);

        //method to update progress bar
        private void updateProgress(int progress)
        {
            progressBar1.Value = progress;
        }
        private void sworker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string path = @"c:\\Program Files\\server-nodejs\\musicJSON.json";
            File.Delete(path);
            File.Create(path).Close();
            Music music = new Music();
            music.music = new List<MusicMetadata>();
            using (FileStream fs1 = new FileStream(@"c:\\Program Files\\server-nodejs\\musiclist.txt", FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs1))
            {
                int count = File.ReadLines(@"c:\\Program Files\\server-nodejs\\musiclist.txt").Count();
                int c1 = 0;
                while (!sr.EndOfStream)
                {
                    if (File.Exists(sr.ReadLine()))
                    {
                        Invoke(new myUpdate(updateProgress), ((int)((++c1) *2* 100) / (count)));
                        string @pth = sr.ReadLine();
                        if (pth == null)
                            continue;
                        //GrantAccess(pth);
                        TagLib.File file;
                        try
                        {
                            file = TagLib.File.Create(pth);
                        }
                        catch(TagLib.CorruptFileException err)
                        {
                            continue;
                        }
                        if (file.Tag.Title != null)
                        {
                            if (file.Tag.Title.Length > 0)
                            {
                                bool flag = false;
                                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                                {
                                    if (file.Tag.Title.Contains(c)) { flag = true; break; }
                                   

                                }
                                if (flag)
                                    continue;
                                

                                if (file.Tag.Pictures.Length > 0)
                                    {
                                        byte[] bitmap = file.Tag.Pictures[0].Data.Data;
                                        if (bitmap.Length > 0)
                                        {
                                            using (Image image = Image.FromStream(new MemoryStream(bitmap)))
                                            {
                                                image.Save(@"c:\\Program Files\\server-nodejs\\pictures\\" + file.Tag.Title + ".jpg", ImageFormat.Jpeg);  // Or Png
                                            }
                                        }
                                    }
                                    MusicMetadata mm = new MusicMetadata(file.Tag.Title, file.Tag.Album, file.Tag.FirstPerformer, file.Tag.FirstGenre,"stream-music?path="+pth, @"get-picture?path=c:\\Program Files\\server-nodejs\\pictures\\" + file.Tag.Title + ".jpg", file.Tag.Track, file.Tag.TrackCount, file.Properties.Duration.TotalSeconds, file.Properties.Description);
                                    if (mm != null)
                                        music.music.Add(mm);
                               

                                }
                            }
                        }
                    }
                }
                string output = JsonConvert.SerializeObject(music, Formatting.Indented);

                using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(output);
                }
            }
        
        private bool GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.FullControl,
                                                             InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                                                             PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
            return true;
        }
        private void sworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                this.progressBar1.Text = "Canceled!";
            }

            else if (!(e.Error == null))
            {
                MessageBox.Show("Error Loading Virtual File ", e.Error.ToString());
            }

            else
            {
                MessageBox.Show("Click on Music in another device to stream the Music files in this device!", "Virtual File generated");
            }
        }

        private void sworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = (e.ProgressPercentage);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listpanel.Add(panel1);
            listpanel.Add(panel2);
            listpanel[0].BringToFront();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
                webBrowser1.Url = new Uri("c:\\SmartStor");
                buttonenable.Enabled = false;
                
            }
         }
            
           
            
        private void mworker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
          
            using (Privilege p = new Privilege(Privilege.Backup))
            {

                for (int j = 0; j < dirs.Length; j++)
                {
                    Process g = new Process();
                    g.StartInfo.UseShellExecute = false;
                    g.StartInfo.RedirectStandardInput = true;
                    g.StartInfo.RedirectStandardOutput = true;

                    g.StartInfo.CreateNoWindow = true;
                    g.StartInfo.WorkingDirectory = @"c:\\Program Files\\server-nodejs";
                    g.StartInfo.FileName = @"c:\\Program Files\\server-nodejs\\music_"+dirs[j].ElementAt(0) +".bat";
                    g.Start();
                    g.EnableRaisingEvents = true;
                    g.WaitForExit();
                    if (j != dirs.Length)
                        mworker.ReportProgress(((j + 1) * 100) / (dirs.Length));
                    else
                        mworker.ReportProgress(100);

                }
             }


        }
        private void mworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                this.progressBar1.Text = "Canceled!";
            }

            else if (!(e.Error == null))
            {
                this.progressBar1.Text = ("Error: " + e.Error.Message);
            }

            else
            {
                this.progressBar1.Text = "Done!";
                dataGridView1.Columns.Add("file list", "Music file list");
                dataGridView1.Columns[0].Width = dataGridView1.Parent.Width;
                DriveInfo[] drives = DriveInfo.GetDrives();
                for (int i = 0; i < drives.Length; i++)
                {
                    if (drives[i].DriveType != DriveType.CDRom)
                    {
                        string path1 = dirs[i].ElementAt(0) + ":\\music_" + dirs[i].ElementAt(0) + ".txt";
                        using (FileStream fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read))
                        using (StreamReader sr = new StreamReader(fs1))
                        {
                            while (!sr.EndOfStream)
                            {
                                 data.Add(sr.ReadLine());
                                 dataGridView1.Rows.Add(data.Last());
                                    
                                
                            }

                        }
                       
                    }

                }
                bw.CancelAsync();
                bw.Dispose();
                string path = @"c:\\Program Files\\server-nodejs\\musiclist.txt";
                using (FileStream fs = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                            if (File.Exists(data.ElementAt(i)))
                                    sw.WriteLine(data.ElementAt(i));
                    }
                    sw.Close();

                }

                if (!bw.IsBusy)
                    bw.RunWorkerAsync();
             }
        }


        private void mworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value=(e.ProgressPercentage);
        }


        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            
            BackgroundWorker worker = sender as BackgroundWorker;
            p = new Process();
            p.StartInfo.UseShellExecute = false;
            
            //p.StartInfo.RedirectStandardError = true;
            //p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.FileName = @"c:\\Program Files\\server-nodejs\\npm.cmd";
            p.StartInfo.WorkingDirectory = @"c:\\Program Files\\server-nodejs";
            p.StartInfo.Arguments = @"start";
            p.StartInfo.RedirectStandardOutput = false;
           // p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            p.WaitForExit();            
            
        }

        

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!buttonenable.Enabled)
            {
                listpanel[1].BringToFront();
                label1.Text = "Image Gallery";
            }
            else
            {
                MessageBox.Show("Enable Cloud service first");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (!sworker.IsBusy)
            {
                
                sworker.RunWorkerAsync();
                MessageBox.Show("Generating virtual file to share!");
            }
        }

        private void buttonmusiclib_Click(object sender, EventArgs e)
        {
            if (!buttonenable.Enabled)
            {

                if (File.Exists(@"c:\\Program Files\\server-nodejs\\musiclist.txt")) //&& (DateTime.Now - File.GetLastWriteTime(@"c:\\Program Files\\server-nodejs\\musicfile.txt")).TotalHours < 24)
                {
                    listpanel[1].BringToFront();
                    label1.Text = "Music Library";
                    using (FileStream fs1 = new FileStream(@"c:\\Program Files\\server-nodejs\\musiclist.txt", FileMode.Open, FileAccess.Read))
                    using (StreamReader sr = new StreamReader(fs1))
                    {
                        dataGridView1.Columns.Add("file list", "Music file list");
                        dataGridView1.Columns[0].Width = dataGridView1.Parent.Width;
                        while (!sr.EndOfStream)
                        {
                            data.Add(sr.ReadLine());
                            dataGridView1.Rows.Add(data.Last());
                          

                        }
                    }
                }
                else
                {
                    listpanel[1].BringToFront();
                    label1.Text = "Music Library";
                    
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        string path = @"c:\\Program Files\\server-nodejs\\music_" + dirs[i].ElementAt(0);
                        File.Create(path + ".bat").Close();
                        using (FileStream fs = new FileStream(path + ".bat", FileMode.Truncate, FileAccess.ReadWrite))
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("@echo off");
                            sw.WriteLine("cd /d " + dirs[i]);
                            sw.WriteLine("dir /s/b *.mp3 >" + "music_" + dirs[i].ElementAt(0) + ".txt");
                            sw.WriteLine("exit");

                        }


                    }
                    

                    if (!mworker.IsBusy)
                        mworker.RunWorkerAsync();


                }
            }
            else
            {
                MessageBox.Show("Enable Cloud service first");
            }

        }

        private void buttonvid_Click(object sender, EventArgs e)
        {
            if (!buttonenable.Enabled)
            {
                listpanel[1].BringToFront();
                label1.Text = "Videos/Movies";
            }
            else
            {
                MessageBox.Show("Enable Cloud service first");
            }
        }

        private void buttonfiles_Click(object sender, EventArgs e)
        {
            listpanel[1].SendToBack();
            
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            
            if (webBrowser1.CanGoBack)
                webBrowser1.GoBack();
        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoForward)
                webBrowser1.GoForward();
        }

        private void webBrowser1_DocumentCompleted_1(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
    public class Music {
        public List<MusicMetadata> music;    
    }
    public class MusicMetadata
    {
        public string title;
        public string album;
        public string artist;
        public string genre;
        public string source;
        public string image;
        public uint trackNumber;
        public uint totalTrackCount;
        public double duration;
        public string site;
        public MusicMetadata(string title, string album,string artist,string genre,string source,string image,uint trackNumber,uint totalTrackCount,double duration,string site)
        {
            if (title != null)
                this.title = title;
            else
                this.title = " ";

            if (album != null)
                this.album =album;
            else
                this.album = " ";
            this.artist = artist;
            if (genre != null)
                this.genre = genre;
            else
                this.genre = " ";
            this.source = source;
            this.image = image;
            this.trackNumber = trackNumber;
            this.totalTrackCount = totalTrackCount;
            this.duration = duration;
            this.site = site;
        }
        

    }
    public sealed class Privilege : IDisposable
    {
        private static Type _privilegeType;
        private object _privilege;

        static Privilege()
        {
            _privilegeType = typeof(string).Assembly.GetType("System.Security.AccessControl.Privilege", false); // mscorlib
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Privilege"/> class and enable the privilege.
        /// </summary>
        /// <param name="name">The privilege name.</param>
        public Privilege(string name)
            : this(name, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Privilege" /> class.
        /// </summary>
        /// <param name="name">The privilege name.</param>
        /// <param name="enable">if set to <c>true</c> the privilege is enabled.</param>
        public Privilege(string name, bool enable)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (_privilegeType == null)
                throw new NotSupportedException();

            _privilege = _privilegeType.GetConstructors()[0].Invoke(new object[] { name });
            if (enable)
            {
                Enable();
            }
        }

        /// <summary>
        /// Disable this privilege from the current thread. 
        /// </summary>
        public void Revert()
        {
            _privilegeType.InvokeMember("Revert", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, _privilege, null);
        }

        /// <summary>
        /// Gets a value indicating whether Revert must be called.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Revert must be called; otherwise, <c>false</c>.
        /// </value>
        public bool NeedToRevert
        {
            get
            {
                return (bool)_privilegeType.InvokeMember("NeedToRevert", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, _privilege, null);
            }
        }

        /// <summary>
        /// Enables this privilege to the current thread. 
        /// </summary>
        public void Enable()
        {
            _privilegeType.InvokeMember("Enable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, _privilege, null);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (NeedToRevert)
            {
                Revert();
            }
        }

        /// <summary>
        /// The SE_ASSIGNPRIMARYTOKEN_NAME privilege.
        /// </summary>
        public const string AssignPrimaryToken = "SeAssignPrimaryTokenPrivilege";

        /// <summary>
        /// The SE_AUDIT_NAME privilege.
        /// </summary>
        public const string Audit = "SeAuditPrivilege";

        /// <summary>
        /// The SE_BACKUP_NAME privilege.
        /// </summary>
        public const string Backup = "SeBackupPrivilege";

        /// <summary>
        /// The SE_CHANGE_NOTIFY_NAME privilege.
        /// </summary>
        public const string ChangeNotify = "SeChangeNotifyPrivilege";

        /// <summary>
        /// The SE_CREATE_GLOBAL_NAME privilege.
        /// </summary>
        public const string CreateGlobal = "SeCreateGlobalPrivilege";

        /// <summary>
        /// The SE_CREATE_PAGEFILE_NAME privilege.
        /// </summary>
        public const string CreatePageFile = "SeCreatePagefilePrivilege";

        /// <summary>
        /// The SE_CREATE_PERMANENT_NAME privilege.
        /// </summary>
        public const string CreatePermanent = "SeCreatePermanentPrivilege";

        /// <summary>
        /// The SE_CREATE_SYMBOLIC_LINK_NAME privilege.
        /// </summary>
        public const string CreateSymbolicLink = "SeCreateSymbolicLinkPrivilege";

        /// <summary>
        /// The SE_CREATE_TOKEN_NAME privilege.
        /// </summary>
        public const string CreateToken = "SeCreateTokenPrivilege";

        /// <summary>
        /// The SE_DEBUG_NAME privilege.
        /// </summary>
        public const string Debug = "SeDebugPrivilege";

        /// <summary>
        /// The SE_ENABLE_DELEGATION_NAME privilege.
        /// </summary>
        public const string EnableDelegation = "SeEnableDelegationPrivilege";

        /// <summary>
        /// The SE_IMPERSONATE_NAME privilege.
        /// </summary>
        public const string Impersonate = "SeImpersonatePrivilege";

        /// <summary>
        /// The SE_INC_BASE_PRIORITY_NAME privilege.
        /// </summary>
        public const string IncreaseBasePriority = "SeIncreaseBasePriorityPrivilege";

        /// <summary>
        /// The SE_INCREASE_QUOTA_NAME privilege.
        /// </summary>
        public const string IncreaseQuota = "SeIncreaseQuotaPrivilege";

        /// <summary>
        /// The SE_INC_WORKING_SET_NAME privilege.
        /// </summary>
        public const string IncreaseWorkingSet = "SeIncreaseWorkingSetPrivilege";

        /// <summary>
        /// The SE_LOAD_DRIVER_NAME privilege.
        /// </summary>
        public const string LoadDriver = "SeLoadDriverPrivilege";

        /// <summary>
        /// The SE_LOCK_MEMORY_NAME privilege.
        /// </summary>
        public const string LockMemory = "SeLockMemoryPrivilege";

        /// <summary>
        /// The SE_MACHINE_ACCOUNT_NAME privilege.
        /// </summary>
        public const string MachineAccount = "SeMachineAccountPrivilege";

        /// <summary>
        /// The SE_MANAGE_VOLUME_NAME privilege.
        /// </summary>
        public const string ManageVolume = "SeManageVolumePrivilege";

        /// <summary>
        /// The SE_PROF_SINGLE_PROCESS_NAME privilege.
        /// </summary>
        public const string ProfileSingleProcess = "SeProfileSingleProcessPrivilege";

        /// <summary>
        /// The SE_RELABEL_NAME privilege.
        /// </summary>
        public const string Relabel = "SeRelabelPrivilege";

        /// <summary>
        /// The SE_REMOTE_SHUTDOWN_NAME privilege.
        /// </summary>
        public const string RemoteShutdown = "SeRemoteShutdownPrivilege";

        ///// <summary>
        ///// The SE_RESERVE_PROCESSOR_NAME privilege.
        ///// </summary>
        //public const string ReserveProcessor = "SeReserveProcessorPrivilege";

        /// <summary>
        /// The SE_RESTORE_NAME privilege.
        /// </summary>
        public const string Restore = "SeRestorePrivilege";

        /// <summary>
        /// The SE_SECURITY_NAME privilege.
        /// </summary>
        public const string Security = "SeSecurityPrivilege";

        /// <summary>
        /// The SE_SHUTDOWN_NAME privilege.
        /// </summary>
        public const string Shutdown = "SeShutdownPrivilege";

        /// <summary>
        /// The SE_SYNC_AGENT_NAME privilege.
        /// </summary>
        public const string SyncAgent = "SeSyncAgentPrivilege";

        /// <summary>
        /// The SE_SYSTEM_ENVIRONMENT_NAME privilege.
        /// </summary>
        public const string SystemEnvironment = "SeSystemEnvironmentPrivilege";

        /// <summary>
        /// The SE_SYSTEM_PROFILE_NAME privilege.
        /// </summary>
        public const string SystemProfile = "SeSystemProfilePrivilege";

        /// <summary>
        /// The SE_SYSTEMTIME_NAME privilege.
        /// </summary>
        public const string SystemTime = "SeSystemtimePrivilege";

        /// <summary>
        /// The SE_TAKE_OWNERSHIP_NAME privilege.
        /// </summary>
        public const string TakeOwnership = "SeTakeOwnershipPrivilege";

        /// <summary>
        /// The SE_TCB_NAME privilege.
        /// </summary>
        public const string TrustedComputingBase = "SeTcbPrivilege";

        /// <summary>
        /// The SE_TIME_ZONE_NAME privilege.
        /// </summary>
        public const string TimeZone = "SeTimeZonePrivilege";

        /// <summary>
        /// The SE_TRUSTED_CREDMAN_ACCESS_NAME privilege.
        /// </summary>
        public const string TrustedCredentialManagerAccess = "SeTrustedCredManAccessPrivilege";

        /// <summary>
        /// The SE_UNDOCK_NAME privilege.
        /// </summary>
        public const string Undock = "SeUndockPrivilege";

        /// <summary>
        /// The SE_UNSOLICITED_INPUT_NAME privilege.
        /// </summary>
        public const string UnsolicitedInput = "SeUnsolicitedInputPrivilege";
    }
}
