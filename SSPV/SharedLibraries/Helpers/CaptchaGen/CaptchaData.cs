using System.Collections.Generic;

namespace CaptchaGen
{
    public class CaptchaData
    {
        #region Propiedades

        public List<string> Values { get; internal set; }

        public string ImageName { get; internal set; }

        public string ImageFieldName { get; internal set; }

        public string AudioFieldName { get; internal set; }

        #endregion
        
        #region Constructor

        public CaptchaData()
        {

        } 

        #endregion
    }
}
