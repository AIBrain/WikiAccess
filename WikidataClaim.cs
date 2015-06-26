namespace WikiAccess {

    public enum ClaimType {
        Null,
        String,
        Int,
        @DateTime
    }

    /// <summary>Container to hold a Wikidata claim.</summary>
    public class WikidataClaim {
        private Wikidate _valueAsDateTime;
        private int _valueAsInt;
        private string _valueAsString;

        public int Pcode {
            get; set;
        }

        public int Qcode {
            get; set;
        }

        public ClaimType Type {
            private set; get;
        }

        public Wikidate ValueAsDateTime {
            get {
                return this._valueAsDateTime;
            }

            set {
                this._valueAsDateTime = value;
                Type = ClaimType.DateTime;
            }
        }

        public int ValueAsInt {
            get {
                return this._valueAsInt;
            }

            set {
                this._valueAsInt = value;
                Type = ClaimType.Int;
            }
        }

        public string ValueAsString {
            get {
                return this._valueAsString;
            }

            set {
                this._valueAsString = value;
                Type = ClaimType.String;
            }
        }

        public WikidataClaim() {
            this._valueAsDateTime = new Wikidate();
            Type = new ClaimType();
            Qcode = 0;
        }

        public override string ToString() {
            switch ( Type ) {
                case ClaimType.DateTime:
                    return ValueAsDateTime.ToString();

                case ClaimType.Int:
                    return ValueAsInt.ToString();

                default:
                    return ValueAsString;
            }
        }
    }
}