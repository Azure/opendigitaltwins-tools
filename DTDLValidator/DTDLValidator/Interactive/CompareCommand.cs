namespace DTDLValidator.Interactive
{
    using CommandLine;
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Verb("compare", HelpText = "Compare two models.")]
    internal class CompareCommand
    {

        [Value(0, HelpText = "First model id to compare.")]
        public string FirstModelId { get; set; }

        [Value(1, HelpText = "Second model id to compare.")]
        public string SecondModelId { get; set; }

        public Task Run(Interactive p)
        {
            if (FirstModelId == null || SecondModelId == null)
            {
                Log.Error("Please specify two valid model ids as parameters");
                return Task.FromResult<object>(null);
            }

            bool firstValid = false;
            bool secondValid = false;

            Dtmi first = ValidateAndCreateDtmi(FirstModelId);
            Dtmi second = ValidateAndCreateDtmi(SecondModelId);
            DTInterfaceInfo dt1 = null;
            DTInterfaceInfo dt2 = null;

            if (first!=null && p.Models.TryGetValue(first, out dt1))
                firstValid = true;
            if (second != null && p.Models.TryGetValue(second, out dt2))
                secondValid = true;

            if (firstValid == false || secondValid == false)
            {
                if (first == null)
                    Log.Error($"First model not a valid dtmi");
                if (first!=null && firstValid == false)
                    Log.Error($"First model not found in loaded models");
                if (second == null)
                    Log.Error($"Second model not a valid dtmi");
                if (second != null && secondValid == false)
                    Log.Error($"Second model not found in loaded models");
                return Task.FromResult<object>(null);
            }

            IReadOnlyDictionary<string, DTContentInfo> con1 = dt1.Contents;
            IReadOnlyDictionary<string, DTContentInfo> con2 = dt2.Contents;
            
            var props1 = con1
                            .Where(p => p.Value.EntityKind == DTEntityKind.Property)
                            .Select(p => p.Value as DTPropertyInfo);

            var props2 = con2
                            .Where(p => p.Value.EntityKind == DTEntityKind.Property)
                            .Select(p => p.Value as DTPropertyInfo);

            IEnumerable<DTPropertyInfo> duplicates = props1.Intersect(props2, new DTPropertyInfoComparer());
            IEnumerable<DTPropertyInfo> diff1 = props1.Except(props2, new DTPropertyInfoComparer());
            IEnumerable<DTPropertyInfo> diff2 = props2.Except(props1, new DTPropertyInfoComparer());

            Log.Alert("Common Properties (comparing name and schema, ignoring explicit ids)");
            Console.WriteLine(listFormatBoth, "Property Name", "Schema");
            Console.WriteLine(listFormatBoth, "-------------", "------");
            foreach (var pi in duplicates)
                Console.WriteLine(listFormatBoth, pi.Name, pi.Schema);

            Console.WriteLine();
            PrintDifference(dt1, diff1);
            Console.WriteLine();
            PrintDifference(dt2, diff2);

            var rels1 = con1
                            .Where(p => p.Value.EntityKind == DTEntityKind.Relationship)
                            .Select(p => p.Value as DTRelationshipInfo);

            var rels2 = con2
                            .Where(p => p.Value.EntityKind == DTEntityKind.Relationship)
                            .Select(p => p.Value as DTRelationshipInfo);

            IEnumerable<DTRelationshipInfo> dupRels = rels1.Intersect(rels2, new DTRelationshipInfoComparer());
            IEnumerable<DTRelationshipInfo> diffRels1 = rels1.Except(rels2, new DTRelationshipInfoComparer());
            IEnumerable<DTRelationshipInfo> diffRels2 = rels2.Except(rels1, new DTRelationshipInfoComparer());

            Console.WriteLine();
            Log.Alert("Common Relationships (comparing name and target - not checking properties, ignoring explicit ids)");
            Console.WriteLine(listFormatBoth, "Relationship Name", "Target");
            Console.WriteLine(listFormatBoth, "-----------------", "------");
            foreach (var pi in dupRels)
            {
                string target = "<any>";
                if (pi.Target != null)
                    target = pi.Target.ToString();
                Console.WriteLine(listFormatBoth, pi.Name, target);
            }

            Console.WriteLine();
            PrintDifference(dt1, diffRels1);
            Console.WriteLine();
            PrintDifference(dt2, diffRels2);

            return Task.FromResult<object>(null);
        }

        private const string listFormatBoth = "{0,-30}{1}";

        private void PrintDifference(DTInterfaceInfo dti, IEnumerable<DTPropertyInfo> diffs)
        {
            Log.Alert($"Only in {dti.DisplayName.FirstOrDefault().Value ?? dti.Id.ToString()}");
            Console.WriteLine(listFormatBoth, "Property Name", "Schema");
            Console.WriteLine(listFormatBoth, "-------------", "------");
            foreach (var pi in diffs)
            {
                Console.WriteLine(listFormatBoth, pi.Name, pi.Schema);
            }
        }

        private void PrintDifference(DTInterfaceInfo dti, IEnumerable<DTRelationshipInfo> diffs)
        {
            Log.Alert($"Only in {dti.DisplayName.FirstOrDefault().Value ?? dti.Id.ToString()}");
            Console.WriteLine(listFormatBoth, "Relationship Name", "Schema");
            Console.WriteLine(listFormatBoth, "-----------------", "------");
            foreach (var pi in diffs)
            {
                Console.WriteLine(listFormatBoth, pi.Name, pi.Target);
            }
        }

        private Dtmi ValidateAndCreateDtmi(string dtmi)
        {
            try
            {
                Dtmi dt = new Dtmi(dtmi);
                return dt;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    // This assumes that the DTProperties are *not* defined with explicit ids
    class DTPropertyInfoComparer : IEqualityComparer<DTPropertyInfo>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(DTPropertyInfo x, DTPropertyInfo y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Name == y.Name && x.Schema == y.Schema;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(DTPropertyInfo pi)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(pi, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashPIName = pi.Name == null ? 0 : pi.Name.GetHashCode();

            //Get hash code for the Code field.
            int hashPISchema = pi.Schema.GetHashCode();

            //Calculate the hash code for the product.
            return hashPIName ^ hashPISchema;
        }
    }

    // This assumes that the DTRelationships are *not* defined with explicit ids
    // Only compares name and target in this sample
    class DTRelationshipInfoComparer : IEqualityComparer<DTRelationshipInfo>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(DTRelationshipInfo x, DTRelationshipInfo y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Name == y.Name && x.Target == y.Target;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(DTRelationshipInfo pi)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(pi, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashPIName = pi.Name == null ? 0 : pi.Name.GetHashCode();

            //Get hash code for the Code field.
            if (pi.Target == null)
                return hashPIName;

            int hashPITarget = pi.Target.GetHashCode();

            //Calculate the hash code for the product.
            return hashPIName ^ hashPITarget;
        }
    }

}
