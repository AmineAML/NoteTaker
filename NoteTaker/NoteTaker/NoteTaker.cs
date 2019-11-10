using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NoteTakerProject
{
    public partial class NoteTaker : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
           int nLeftRect,     // x-coordinate of upper-left corner
           int nTopRect,      // y-coordinate of upper-left corner
           int nRightRect,    // x-coordinate of lower-right corner
           int nBottomRect,   // y-coordinate of lower-right corner
           int nWidthEllipse, // height of ellipse
           int nHeightEllipse // width of ellipse
        );

        public NoteTaker()
        { 
            InitializeComponent();
            //Uses the private static extern and the Dll import above to change the form's border to rounded corners
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            //Notification
            if (File.Exists(FileManagement.logPath))
            {
                var lineCount = File.ReadLines(FileManagement.logPath).Count();
                //Only shows a notification if the user has one note or more
                if(lineCount != 0)
                {
                    notifyIcon1.ShowBalloonTip(1000, "View notes", "You have " + lineCount + " notes.", ToolTipIcon.Info);
                }
            }
            //Hide app's icon from the taskbar
            this.ShowInTaskbar = false;

            //Start position in Top-Right screen
            this.StartPosition = FormStartPosition.Manual;
            foreach (var scrn in Screen.AllScreens)
            {
                if (scrn.Bounds.Contains(this.Location))
                {
                    this.Location = new Point(scrn.Bounds.Right - this.Width, scrn.Bounds.Top);
                    return;
                }
            }
        }

        public int tootalsecs = 2;
        public void NoteTakenNotification()
        {
            var timer = new System.Timers.Timer() { Interval = 1000 };

            timer.Elapsed += (obj, args) =>
            {
                if (tootalsecs == 0)
                {
                    txtNote.Invoke((Action)delegate
                    {
                        label2.Text = "";
                    });
                }
                else
                {
                    //This is away to avoid the Cross-thread error, for exemple if a "txtNote" was used here it would show that error because it's used/invoked elsewhere in this form that's why it's invoked using this way
                    txtNote.Invoke((Action)delegate
                    {
                        label2.Text="Note taken!";
                    });
                    tootalsecs--;
                }
            };
            timer.Start();
            tootalsecs = 2;
            //Making sure that the "Note taken!" message don't stay in display, because sometimes it does "lag" and stays in diplay
            label2.Text = "";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (File.Exists(FileManagement.logPath))
            {
                try
                {
                    FileManagement.Numéro();
                    FileManagement.InsertValue(txtNote.Text);
                    ClearTextbox();
                    //Shows a "message" that the note was taken
                    NoteTakenNotification();
                }
                catch (Exception Xerror)
                {
                    throw Xerror;
                }
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("File does not exist!\r\nDo you want to create it?", "Error", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        FileManagement.CreateFile();
                        //Re-saves the note after creating the text file
                        FileManagement.Numéro();
                        FileManagement.InsertValue(txtNote.Text);
                        ClearTextbox();
                        //Shows a "message" that the note was taken
                        NoteTakenNotification();
                    }
                    catch(Exception Xerror2)
                    {
                        throw Xerror2;
                    }
                }
                else if (dialogResult == DialogResult.No)
                {
                    DialogResult dialogueResult2 = MessageBox.Show("You need a file to store your notes!\r\nDo you want to create it?", "Warning", MessageBoxButtons.YesNo);
                    if(dialogueResult2 == DialogResult.Yes)
                    {
                        try
                        {
                            FileManagement.CreateFile();
                            //Write in the next line after a new line(write at a new line after another line)
                            FileManagement.Numéro();
                            FileManagement.InsertValue(txtNote.Text);
                            ClearTextbox();
                            //Shows a "message" that the note was taken
                            NoteTakenNotification();
                        }
                        catch(Exception Xerror3)
                        {
                            throw Xerror3;
                        }
                    }
                    else if(dialogResult == DialogResult.No)
                    {
                        MessageBox.Show("File was not created!\r\nNote was note saved!");
                    }
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            //txtNote.Text = @"New note..";
            if (txtNote.Text.Length > 0)
            {
                ClearTextbox();
            }
        }

        private void openNotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Open text file
            if (File.Exists(FileManagement.logPath))
            {
                Process.Start(FileManagement.logPath);

            }
            else
            {
                MessageBox.Show("File doesn't exist!");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnFileTextOpen_Click(object sender, EventArgs e)
        {
            if (File.Exists(FileManagement.logPath))
            {
                Process.Start(FileManagement.logPath);
            }
            else
            {
                MessageBox.Show("File doesn't exist!");
            }
        }

        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private void addNoteTakerToStartUpMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Add the value in the registry so that the application runs at startup
            rkApp.SetValue("NoteTaker", Application.ExecutablePath.ToString());
            
            ChangeContextMenuStripItem2();
        }

        //click,movement and color changes by default/input text events
        private void txtNote_Enter(object sender, EventArgs e)
        {
            TextboxDefaultColor();
            if (txtNote.Text.Equals(@"New note..."))
            {
                txtNote.Text = "";
            }
        }
        private void txtNote_Leave(object sender, EventArgs e)
        {
            TextboxChangeColor();
            if (txtNote.Text.Equals(""))
            {
                txtNote.Text = @"New note...";
                btnFileTextOpen.Focus();
            }
        }
        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            TextboxChangeColor();
            if (txtNote.Text.Equals(""))
            {
                txtNote.Text = @"New note...";
                btnFileTextOpen.Focus();
            }
        }
        private void txtNote_MouseClick(object sender, MouseEventArgs e)
        {
            TextboxDefaultColor();
            if (txtNote.Text.Equals(@"New note..."))
            {
                txtNote.Text = "";
            }
        }
        private void txtNote_MouseLeave(object sender, EventArgs e)
        {
            TextboxChangeColor();
            if (txtNote.Text.Equals(""))
            {
                txtNote.Text = @"New note...";
                btnFileTextOpen.Focus();
            }
        }
        public void TextboxChangeColor()
        {
            if (txtNote.Text.Equals(@"New note..."))
            {
                txtNote.ForeColor = Color.DarkSlateGray;
            }
        }

        public void TextboxDefaultColor()
        {
            if (txtNote.Text.Equals(@"New note..."))
            {
                txtNote.ForeColor = Color.Black;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Still somehow doesn't work until the user move the mouse around it
            TextboxDefaultColor();

            if (FileManagement.IsStartupItem("NoteTaker"))
            {
                ChangeContextMenuStripItem2();
            }

            //Organizing the numeritation in case the user removed a note, for exemple:[ 1) First note, 3) Third note] should be corrected to [ 1) First note, 2) Third note]
            FileManagement.OrganizeNumérotation();
        }

        //Change the 3rd item in the contextMenuStrip from "Add NoteTaker to Start-Up menu" to "NoteTaker is now on AutoRun!" and make it Inclickable
        public void ChangeContextMenuStripItem2()
        {
            contextMenuStrip1.Items[2].Enabled = false;
            contextMenuStrip1.Items[2].Text = "NoteTaker is now on AutoRun!";
        }

        public void ClearTextbox()
        {
            txtNote.Text = @"New note...";
            TextboxChangeColor();
            btnFileTextOpen.Focus();
        }
    }
}
