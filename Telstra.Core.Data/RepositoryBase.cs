using System;
namespace Telstra.Core.Data
{
    public abstract class RepositoryBase
    {
        private const string DefaultConnectionStringTemplate = "Server=(local);Database={0};Trusted_Connection=True;MultipleActiveResultSets=true";

        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_internalConnectionString))
                {
                    _internalConnectionString = this.GetOrCreateConnectionString();
                }

                return _internalConnectionString;
            }

            set => _internalConnectionString = value;
        }

        private string _internalConnectionString = string.Empty;

        protected virtual string GetOrCreateConnectionString()
        {
            var connectionString = string.Format(DefaultConnectionStringTemplate, this.GetType().FullName?.Replace(".", ""));
            return connectionString;
        }
    }
}
