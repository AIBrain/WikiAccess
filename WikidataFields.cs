﻿namespace WikiAccess {

    using System.Collections.Generic;

    /// <summary>Container to hold a single item grabbed from Wikidata</summary>
    public class WikidataFields {

        // Cannot use Dictionary as can have multiple claims per item

        public List<KeyValuePair<int, WikidataClaim>> Claims {
            get; set;
        }

        public Dictionary<string, string> Description {
            get; set;
        }

        public int ID {
            get; set;
        }

        public Dictionary<string, string> Labels {
            get; set;
        }

        public Dictionary<string, string> WikipediaLinks {
            get; set;
        }

        public WikidataFields() {
            WikipediaLinks = new Dictionary<string, string>();
            Labels = new Dictionary<string, string>();
            Description = new Dictionary<string, string>();
            Claims = new List<KeyValuePair<int, WikidataClaim>>();
        }
    }
}