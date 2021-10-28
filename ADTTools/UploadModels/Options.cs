using CommandLine;
using System.Collections.Generic;

namespace UploadModels
{
    internal class Options
    {
        [Option('t', "tenantId", SetName = "upload", Required = true, HelpText = "The application's tenant id for connecting to Azure Digital Twins.")]
        public string TenantId { get; set; }

        [Option('c', "clientId", SetName = "upload", Required = false, HelpText = "The application's client id for connecting to Azure Digital Twins.")]
        public string ClientId { get; set; }

        [Option('h', "hostName", SetName = "upload", Required = true, HelpText = "The host name of your Azure Digital Twins instance.")]
        public string HostName { get; set; }

        [Option('b', "batchSize", SetName = "upload", Default = 100, HelpText = "The maximum number of models uploaded in each batch (default 100).")]
        public int BatchSize { get; set; }

        [Option('w', "whatIf", SetName = "whatif", Required = true, Default = false, HelpText = "Display the order of the models that would be uploaded, but do not upload.")]
        public bool WhatIf { get; set; }

        [Value(0, Required = true, HelpText = "Model file names to upload (supports globs).")]
        public IEnumerable<string> FileSpecs { get; set; }
    }
}
