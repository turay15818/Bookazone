namespace Bookazone.Application.Common.Shared.Utils;

public static class Routes
{
    public static class ApiRoute
    {
        private const string PrefixApi = "api";

  
        public static class Security
        {
            private const string PrefixSecurity = PrefixApi + "";

            public static class Auth
            {
                public const string Base = PrefixSecurity + "/auth";
                public const string Login = "login";
                public const string Register = "register";
            }
        }

        public static class Secure
        {
            private const string PrefixSecure = PrefixApi + "/secure";

            public static class App
            {
                private const string PrefixSecureApp = PrefixSecure + "/app";
                public static class PhoneNumber
                {
                    public const string Base = PrefixSecureApp + "/phoneNumber";
                    public const string Create = "create";
                }

                public static class Crypto
                {
                    public const string Base = PrefixSecureApp + "/crypto";
                    public const string Encrypt = "encrypt";
                    public const string Decrypt = "decrypt";
                }
                
                public static class Token
                {
                    public const string Base = PrefixSecureApp + "/token";
                    public const string Generate = "generate";
                    public const string Validate = "validate";
                }

            }
        }
    }
}