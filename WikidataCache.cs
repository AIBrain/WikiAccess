namespace WikiAccess {

    using System;
    using System.Collections.Concurrent;
    using System.IO;

    /// <summary>Class to create a cache of property labels, cutting down on Wikidata traffic.</summary>
    internal class WikidataCache {
        private readonly ConcurrentDictionary<int, string> _cache = new ConcurrentDictionary<int, string>();
        private readonly string _labelcache = Path.GetTempPath() + "WikidataLabelCache";

        /// <summary>
        /// Constructor. Reads in existing cache from LABELCACHE TODO Error trap for dodgy cache.
        /// </summary>
        public WikidataCache() {
            if ( !File.Exists( this._labelcache ) ) {
                File.Create( this._labelcache ).Close();
            }

            if ( this._cache.Count == 0 ) {
                using ( var sr = new StreamReader( this._labelcache ) ) {
                    string propertyAsString;
                    int property;
                    while ( ( propertyAsString = sr.ReadLine() ) != null ) {
                        property = Convert.ToInt32( propertyAsString );
                        var description = sr.ReadLine();
                        this._cache[ property ] = description;
                    }
                }
            }
        }

        public string RetrieveLabel( int qcode ) {
            string description;
            if ( this._cache.TryGetValue( qcode, out description ) ) {
                return description;
            }
            return this.LookupLabel( qcode );
        }

        /// <summary>If its a new property, look up label on Wikidata and add to cache.</summary>
        /// <param name="qcode"></param>
        /// <returns></returns>
        private string LookupLabel( int qcode ) {
            var io = new WikidataIO {
                Action = "wbgetentities",
                Format = "json",
                Ids = qcode,
                Props = "labels",
                Languages = "en|en-gb|ro"
            };
            var fields = io.GetData();

            string name;
            if ( !fields.Labels.TryGetValue( "en-gb", out name ) ) {
                if ( !fields.Labels.TryGetValue( "en", out name ) ) {
                    fields.Labels.TryGetValue( "en", out name );
                }
            }

            using ( var sw = File.AppendText( this._labelcache ) ) {
                sw.WriteLine( qcode );
                sw.WriteLine( name );
            }

            this._cache[ qcode ] = name;

            return name;
        }
    }
}