using System;
using System.Collections.Generic;
using System.Text;

namespace WebX.Security.Shared.Constants
{
    public class MFAFlag
    {
        public const int MOBILE = 1;
        public const int ALTERNATEEMAIL = 2;
        public const int TOTP = 4;
    }
}
