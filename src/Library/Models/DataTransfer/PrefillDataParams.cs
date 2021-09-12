namespace Library.Models.DataTransferObjects
{
    /// <summary>
    /// TODO: Refactor to eliminate hard-coded data.
    /// INTENT: Ideally, we want the client to supply these props, but some clients(IMO)
    ///         will not (and probably should not) have knowledge of these params.
    /// </summary>
    public class PrefillDataParams
    {
        public Rp524PrefillData PrefillData { get; set; }

        private string _cacheKey = "_cacheKey";
        public string CacheKey { 
            get
            {
                return _cacheKey;
            }
            set
            {
                if (!string.IsNullOrEmpty(value)) { _cacheKey = value; }
            } 
        }

        private string _clientCallbackRoute = "rp524";
        public string ClientCallbackRoute
        {
            get
            {
                return _clientCallbackRoute;
            }
            set
            {
                if (!string.IsNullOrEmpty(value)) { _clientCallbackRoute = value; }
            }
        }
    }
}
