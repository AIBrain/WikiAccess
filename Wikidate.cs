namespace WikiAccess {

    using System;

    public enum DatePrecision {
        Null,
        Day,
        Month,
        Year,
        Decade,
        Century,
        Unknown,
        NotEntered,
        NoProperty,
        Bce,
        Invalid,
        Millenium
    };

    /// <summary>Class to hold a date, which includes a precision</summary>
    public class Wikidate {

        public DateTime ThisDate {
            get; set;
        }

        public DatePrecision ThisPrecision {
            get; set;
        }

        public static bool IsCalculable( DatePrecision thisPrecision ) {
            switch ( thisPrecision ) {
                case DatePrecision.Day:
                case DatePrecision.Decade:
                case DatePrecision.Month:
                case DatePrecision.Year:
                    return true;

                default:
                    return false;
            }
        }

        public override string ToString() {
            var formattedDate = "Invalid";

            switch ( this.ThisPrecision ) {
                case DatePrecision.Null:
                case DatePrecision.Day:
                    formattedDate = this.ThisDate.ToString( "d MMMM yyyy" );
                    break;

                case DatePrecision.Month:
                    formattedDate = this.ThisDate.ToString( "MMMM yyyy" );
                    break;

                case DatePrecision.Year:
                    formattedDate = this.ThisDate.ToString( "yyyy" );
                    break;

                case DatePrecision.Decade:
                    formattedDate = this.ThisDate.ToString( "yyyy" ).Substring( 0, 3 ) + "0s";
                    break;

                case DatePrecision.Century:
                    var century = Convert.ToInt32( this.ThisDate.ToString( "yyyy" ).Substring( 0, 2 ) );
                    formattedDate = ( century + 1 ) + "th century";
                    break;

                case DatePrecision.Millenium:
                    var millenium = Convert.ToInt32( this.ThisDate.ToString( "yyyy" ).Substring( 0, 1 ) );
                    formattedDate = ( millenium + 1 ) + " millenium";
                    break;

                case DatePrecision.Unknown:
                    formattedDate = "Unknown";
                    break;

                case DatePrecision.NotEntered:
                case DatePrecision.NoProperty:
                    formattedDate = "No value";
                    break;
            }
            return formattedDate;
        }
    }
}