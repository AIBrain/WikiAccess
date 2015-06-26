namespace WikiAccess {

    public class ErrorMessage {

        public int Code {
            get;
        }

        public string Message {
            get;
        }

        public string Module {
            get;
        }

        public string SystemMessage {
            get;
        }

        public ErrorMessage( string module, int code, string message, string systemMessage = null ) {
            Module = module;
            Code = code;
            Message = message;
            SystemMessage = systemMessage;
        }

        public override string ToString() {
            var returnMessage = $"{this.Module}{this.Code.ToString( "000" )}: {this.Message}";

            if ( !string.IsNullOrWhiteSpace( SystemMessage ) ) {
                returnMessage += $" ({this.SystemMessage})";
            }

            return returnMessage;
        }
    }
}