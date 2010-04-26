using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Net;
using System.IO.Compression;
using Ionic.Zip;
using System.Diagnostics;

namespace Updater
{
    /// <summary>
    /// Oberon Update Manager form.
    /// </summary>
    public partial class UpdateForm : Form
    {
        #region Private Variables
        /// <summary>
        /// Url to download updates from.
        /// </summary>
        private string UpdateUrl;

        /// <summary>
        /// Extract downloaded files to this location.
        /// </summary>
        private string ExtractLocation;

        /// <summary>
        /// Application to launch after download.
        /// </summary>
        private string ApplicationToLaunch;

        /// <summary>
        /// Thread inside which the download happens.
        /// </summary>
        private Thread DownloadThread;

        /// <summary>
        ///  The stream of data retrieved from the web server.
        /// </summary>
        private Stream ResponseStream;

        /// <summary>
        /// The stream of data that we write to the harddrive.
        /// </summary>
        private Stream LocalStream;

        /// <summary>
        /// The request to the web server for file information.
        /// </summary>
        private HttpWebRequest WebRequest;

        /// <summary>
        /// The response from the web server containing information about the file
        /// </summary>
        private HttpWebResponse WebResponse;

        /// <summary>
        /// Full path to where the downloaded file is located.
        /// </summary>
        private string LocalFileLocation;

        /// <summary>
        /// The progress of the download in percentage
        /// </summary>
        private static int PercentProgress;

        /// <summary>
        /// The delegate which we will call from the thread to update the form
        /// </summary>
        /// <param name="BytesRead"></param>
        /// <param name="TotalBytes"></param>
        private delegate void UpdateProgessCallback(Int64 BytesRead, Int64 TotalBytes);
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public UpdateForm(string updateUrl, string extractLocation, string applicationToLaunch)
        {
            // init components
            InitializeComponent();

            // set internal variables
            UpdateUrl = updateUrl;
            ExtractLocation = extractLocation;
            ApplicationToLaunch = applicationToLaunch;

            // configure progress bar
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;

            // start download
            DownloadThread = new Thread(DownloadAndInstallUpdate);
            DownloadThread.Start();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Downloads the latest update and installs it.
        /// </summary>
        private void DownloadAndInstallUpdate()
        {
            // download file
            using (WebClient client = new WebClient())
            {
                try
                {
                    // set up request
                    WebRequest = (HttpWebRequest)HttpWebRequest.Create(UpdateUrl);
                    WebRequest.Credentials = CredentialCache.DefaultCredentials;

                    // retrieve response
                    WebResponse = (HttpWebResponse)WebRequest.GetResponse();

                    // Ask the server for the file size and store it
                    Int64 fileSize = WebResponse.ContentLength;

                    // open the URL for download
                    ResponseStream = client.OpenRead(UpdateUrl);
                    LocalFileLocation = Path.Combine(Path.GetTempPath(), Path.GetFileName(UpdateUrl));
                    LocalStream = new FileStream(LocalFileLocation, FileMode.Create, FileAccess.Write, FileShare.None);

                    int bytesSize = 0; // it will store the curret number of bytes we retrieve from the server
                    byte[] downBuffer = new byte[2048]; // A buffer for storing and writing the data retrieved from the server

                    // Loop through the buffer until the buffer is empty
                    while ((bytesSize = ResponseStream.Read(downBuffer, 0, downBuffer.Length)) > 0)
                    {
                        // Write the data from the buffer to the local hard drive
                        LocalStream.Write(downBuffer, 0, bytesSize);
                        // Invoke the method that updates the form's label and progress bar
                        this.Invoke(new UpdateProgessCallback(this.UpdateProgress), new object[] { LocalStream.Length, fileSize });
                    }
                }
                catch
                {
                    // bail if we can't download the file
                    return;
                }
                finally
                {
                    // When the above code has ended, close the streams
                    if (ResponseStream != null)
                        ResponseStream.Close();
                    if (LocalStream != null)
                        LocalStream.Close();                    
                }
            }

            // extract files
            this.Invoke(delegate() { ExtractDownload(); });

            // Notify user
            this.Invoke(delegate() { NotifyUserUpdateIsComplete(); });

            // close the app
            Application.Exit();
        }

        /// <summary>
        /// Updates the progress bar with how much we've downloaded.
        /// </summary>
        /// <param name="BytesRead">Number of bytes read.</param>
        /// <param name="TotalBytes">Total number of bytes to read.</param>
        private void UpdateProgress(Int64 BytesRead, Int64 TotalBytes)
        {
            // Calculate the download progress in percentages
            PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);
            // Make progress on the progress bar
            progressBar.Value = PercentProgress;
        }

        /// <summary>
        /// Updates the progress bar with extraction progress.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args</param>
        private void UpdateProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_EntryBytesWritten)
            {
                // Calculate the download progress in percentage
                PercentProgress = Convert.ToInt32((e.BytesTransferred * 100) / e.TotalBytesToTransfer);
                // Make progress on the progress bar
                progressBar.Value = PercentProgress;
            }
        }
        
        /// <summary>
        /// Extract the downloaded file.
        /// </summary>
        private void ExtractDownload()
        {
            // if it's not a zip file, we can't extract it
            if (Path.GetExtension(LocalFileLocation) != ".zip")
                return;

            // reset progress bar
            lblStatus.Text = "Extracting update...";
            progressBar.Value = 0;

            // unzip package
            using (ZipFile package = ZipFile.Read(LocalFileLocation))
            {
                // tie in event handler to show progress
                package.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(UpdateProgress);
                // extract all files and overwrite what is there
                package.ExtractAll(ExtractLocation, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        /// <summary>
        /// Notify's the user that an update is complete and shuts 
        /// </summary>
        private void NotifyUserUpdateIsComplete()
        {
            // set text 
            lblStatus.Text = "Update complete.";

            // notify the user
            if (string.IsNullOrEmpty(ApplicationToLaunch))
            {
                MessageBox.Show("Update is complete!", "Oberon Update Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                DialogResult result = MessageBox.Show("Update is complete! Select Yes to start the program or No to close.", "Launch Program", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // launch program if requested
                    try
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = Path.Combine(ExtractLocation, ApplicationToLaunch);
                        p.Start();
                    }
                    catch { }
                }                
            }
        }
        #endregion
    }
}
