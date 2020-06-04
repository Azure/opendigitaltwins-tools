using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DTDLValidator.Interactive
{
    internal class DTDLParser
    {
        public DTDLParser(IDictionary<Dtmi, DTInterfaceInfo> modelStore)
        {
            this.modelStore = modelStore;
        }

        public async Task<(IReadOnlyDictionary<Dtmi, DTEntityInfo>, IEnumerable<DTInterfaceInfo>)> ParseAsync(IEnumerable<string> jsonTexts)
        {
            // Create resolver state per call to ParseAsync so that multiple calls to ParseAsync can run concurrently.
            DTDLResolver dtdlResolver = new DTDLResolver(modelStore);
            parser.DtmiResolver = dtdlResolver.Resolver;
            IReadOnlyDictionary<Dtmi, DTEntityInfo> entities = await parser.ParseAsync(jsonTexts);
            return (entities, dtdlResolver.ResolvedInterfaces);
        }


        private class DTDLResolver
        {
            public DTDLResolver(IDictionary<Dtmi, DTInterfaceInfo> modelStore)
            {
                this.modelStore = modelStore;
            }

            public async Task<IEnumerable<string>> Resolver(IReadOnlyCollection<Dtmi> dtmis)
            {
                List<string> texts = new List<string>();
                foreach (Dtmi dtmi in dtmis)
                {
                    if (modelStore.TryGetValue(dtmi, out DTInterfaceInfo @interface))
                    {
                        ResolvedInterfaces.Add(@interface);
                        texts.Add(@interface.GetJsonLdText());
                    }
                }

                return texts;
            }

            public IList<DTInterfaceInfo> ResolvedInterfaces { get; private set; } = new List<DTInterfaceInfo>();

            private readonly IDictionary<Dtmi, DTInterfaceInfo> modelStore;
        }

        private readonly ModelParser parser = new ModelParser();
        private readonly IDictionary<Dtmi, DTInterfaceInfo> modelStore;
    }
}
