namespace WikiAccess {

    using System.Collections.Generic;

    public class WikiMediaApiErrorLog : IErrorLog {

        public WikiMediaApiErrorLog() {

            // ReSharper disable once UseObjectOrCollectionInitializer
            Errors = new List<ErrorMessage>();
#if DEBUG
            Errors.Add( new ErrorMessage( Module, 0, "WikimediaAPI module" ) );
#endif
        }

        public string Module => "A";

        public List<ErrorMessage> Errors {
            get; set;
        }

        /// <summary>Web server is not contactable. Either no Internet or an invalid URL</summary>
        public void CannotAccessWiki( string url, string systemMessage ) => this.Errors.Add( new ErrorMessage( this.Module, 1, "Unable to contact Wiki URL " + url, systemMessage ) );

        /// <summary>No file was grabbed from the internet page. Unknown reason.</summary>
        public void NoFileDownloaded() => this.Errors.Add( new ErrorMessage( this.Module, 2, "No file downloaded" ) );

        public void UnableToRetrieveDownload( string systemMessage ) => this.Errors.Add( new ErrorMessage( this.Module, 3, "Unable to retrieve Download", systemMessage ) );
    }
}