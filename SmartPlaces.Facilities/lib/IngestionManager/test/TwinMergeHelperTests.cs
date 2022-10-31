// -----------------------------------------------------------------------
// <copyright file="TwinMergeHelperTests.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Test
{
    using System.Text.Json;
    using global::Azure.DigitalTwins.Core;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.AzureDigitalTwins;
    using Xunit;
    using Xunit.Abstractions;

    public class TwinMergeHelperTests
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ITestOutputHelper output;
#pragma warning restore IDE0052 // Remove unread private members

        public TwinMergeHelperTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TryCreatePatchDocument_EmptyTwins_ReturnFalse()
        {
            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.False(result);
            Assert.Equal("[]", jsonPatchDocument.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_DifferentModels_ReturnTrue()
        {
            var existingTwin = new BasicDigitalTwin();
            existingTwin.Metadata.ModelId = "dtmi:example:foo;1";
            existingTwin.Contents.Add("oldKey", "OldValue");
            var newTwin = new BasicDigitalTwin();
            newTwin.Metadata.ModelId = "dtmi:example:bar;1";

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Equal("[{\"op\":\"replace\",\"path\":\"/$metadata/$model\",\"value\":\"dtmi:example:bar;1\"}]", jsonPatchDocument.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingStringTwinFieldMoreDataThanNew_ReturnFalse()
        {
            var existingTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("oldKey", "OldValue");
            var newTwin = new BasicDigitalTwin();

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.False(result);
            Assert.Equal("[]", jsonPatchDocument.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_NewStringPropertyonNewTwin_ReturnTrue()
        {
            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            newTwin.Contents.Add("newKey", "NewValue");

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Equal("[{\"op\":\"add\",\"path\":\"/newKey\",\"value\":\"NewValue\"}]", jsonPatchDocument.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingStringPropertyonTwinTheSame_ReturnFalse()
        {
            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("newKey", "NewValue");
            newTwin.Contents.Add("newKey", "NewValue");

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.False(result);
            Assert.Equal("[]", jsonPatchDocument.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingStringPropertyonTwinDifferent_ReturnTrue()
        {
            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("newKey", "oldValue");
            newTwin.Contents.Add("newKey", "NewValue");

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Equal("[{\"op\":\"replace\",\"path\":\"/newKey\",\"value\":\"NewValue\"}]", jsonPatchDocument.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingObjectSingleStringPropertyonTwinDifferent_ReturnTrue()
        {
            var newObj = new
            {
                Vehicle = "Car",
            };

            var existingObj = new
            {
                Vehicle = "Truck",
            };

            var newJson = JsonSerializer.SerializeToElement(newObj);
            var existingJson = JsonSerializer.SerializeToElement(existingObj);

            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("newKey", existingJson);
            newTwin.Contents.Add("newKey", newJson);

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            var expected = string.Format("[{{\"op\":\"replace\",\"path\":\"/newKey/Vehicle\",\"value\":\"Car\"}}]");
            Assert.Equal(expected, jsonPatchDocument.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingObjectMultipleStringPropertyonTwinDifferent_ReturnTrue()
        {
            var newObj = new
            {
                Vehicle = "Car",
                Wheels = "4",
            };

            var existingObj = new
            {
                Vehicle = "Truck",
                Wheels = "6",
            };

            var newJson = JsonSerializer.SerializeToElement(newObj);
            var existingJson = JsonSerializer.SerializeToElement(existingObj);

            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("newKey", existingJson);
            newTwin.Contents.Add("newKey", newJson);

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            var expected = string.Format("[{{\"op\":\"replace\",\"path\":\"/newKey/Vehicle\",\"value\":\"Car\"}},{{\"op\":\"replace\",\"path\":\"/newKey/Wheels\",\"value\":\"4\"}}]");
            Assert.Equal(expected, jsonPatchDocument.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingObjectIntPropertyonTwinDifferent_ReturnTrue()
        {
            var newObj = new
            {
                Vehicle = "Car",
                Wheels = 4,
            };

            var existingObj = new
            {
                Vehicle = "Truck",
                Wheels = 6,
            };

            var newJson = JsonSerializer.SerializeToElement(newObj);
            var existingJson = JsonSerializer.SerializeToElement(existingObj);

            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("newKey", existingJson);
            newTwin.Contents.Add("newKey", newJson);

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            var expected = string.Format("[{{\"op\":\"replace\",\"path\":\"/newKey/Vehicle\",\"value\":\"Car\"}},{{\"op\":\"replace\",\"path\":\"/newKey/Wheels\",\"value\":4}}]");
            Assert.Equal(expected, jsonPatchDocument.ToString());
        }
    }
}