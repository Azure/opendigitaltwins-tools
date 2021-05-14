using CommandLine;

namespace DeleteModels
{
    internal class Options
    {
        [Option('t', "tenantId", Required = true, HelpText = "The application's tenant id for connecting to Azure Digital Twins.")]
        public string TenantId { get; set; }

        [Option('c', "clientId", Required = true, HelpText = "The application's client id for connecting to Azure Digital Twins.")]
        public string ClientId { get; set; }

        [Option('h', "hostName", Required = true, HelpText = "The host name of your Azure Digital Twins instance.")]
        public string HostName { get; set; }
    }
}
