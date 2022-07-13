using System;
namespace Telstra.Consumer.Api.Extensions
{
    public static class EnvironmentExtensions
    {
        public static string fakeADUserEmail()
        {
            // Implement the ENV var 
            return "some_email@some_domain.com";
        }

        public static string fakeADUserName()
        {
            // Implement the ENV var 
            return "some_user_name";
        }

        public static string fakeADUserOid()
        {
            // Implement the ENV var 
            return "some_user_id";
        }
    }
}
