namespace WikiAccess {

    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    /// Abstract class that does the actual call to the Wiki websites. Must set Target Framework to
    /// .NET framework 4 (not client profile) if you are using this in your own project, please
    /// change BOTNAME and CONTACT below
    /// </summary>
    public abstract class WikimediaApi {
        private const string Botname = "Perigliobot";
        private const string Contact = "Wikidata@lynxmail.co.uk";

        //private int _second;

        private readonly Stopwatch _timeSinceLastAccess = Stopwatch.StartNew();

        protected WikiMediaApiErrorLog APIErrors {
            get;
        }

        protected abstract string ApIurl {
            get;
        }

        protected string Content {
            get; private set;
        }

        protected abstract string Parameters {
            get;
        }

        public WikimediaApi() {
            APIErrors = new WikiMediaApiErrorLog();
        }

        /// <summary>Method used to grab page from Wiki website, and store into Content property.</summary>
        /// <returns></returns>
        protected bool GrabPage() {
            ThrottleWikiAccess();
            return DownloadPage();
        }

        /// <summary>Download page from Wiki web site into temp file</summary>
        /// <returns>Temp file name</returns>
        private Boolean DownloadPage() {

            //var tempfile = Path.GetTempFileName();

            var wikiClient = new WebClient();
            wikiClient.Headers.Add( "user-agent", $"{Botname} Contact: {Contact})" );
            var fullUrl = $"{this.ApIurl}{this.Parameters}";

            try {

                //wikiClient.DownloadFile( fullUrl, tempfile );
                this.Content = wikiClient.DownloadString( fullUrl );
                return true;
            }
            catch ( WebException e ) {

                //tempfile = null;
                APIErrors.CannotAccessWiki( fullUrl, e.Message );
            }
            return false;

            //return tempfile;
        }

        ///// <summary>Read page from download, store in Content property</summary>
        ///// <param name="tempfile"></param>
        //private bool LoadPage( string tempfile ) {
        //    if ( tempfile == null ) {
        //        return false;
        //    }
        //    try {
        //        this.Content = File.ReadAllText( tempfile );
        //        File.Delete( tempfile );
        //    }
        //    catch ( Exception e ) {
        //        this.APIErrors.UnableToRetrieveDownload( e.Message );
        //        return false;
        //    }
        //    return true;
        //}
        /// <summary>
        /// Make sure we wait a second between calls.
        /// This method only throttles fast running scripts allowing slower ones to run at full speed.
        /// </summary>
        private void ThrottleWikiAccess() {
            try {
                if ( _timeSinceLastAccess.Elapsed >= TimeSpan.FromSeconds( 1 ) ) {
                    return;
                }
                Thread.Sleep( 1000 );
                Application.DoEvents();
            }
            finally {
                _timeSinceLastAccess.Restart();
            }
        }
    }
}