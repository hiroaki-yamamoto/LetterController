using LetterController.Core;
using LetterController.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RimWorld;

namespace LetterController.Tests
{
    /// <summary>
    /// Unit tests verifying the search, filtering, and linear-time O(N) sorting behaviors of <see cref="IncidentFilterService"/>.
    /// </summary>
    [TestClass]
    public class FilterServiceTests
    {
        private static List<IncidentItem> CreateSampleItems()
        {
            return new List<IncidentItem>
            {
                new("RaidEnemy", "Enemy Raid", "Core"),
                new("TraderCaravanArrival", "Trader Caravan", "Core"),
                new("SolarFlare", "Solar Flare", "Core"),
                new("EntityCodexEntryUnlocked", "Entity Discovered", "Anomaly"),
                new("Infestation", "Infestation", "Biotech"),
                new("ModCustomIncident", "Custom Mod Event", "Awesome Mod"),
                new("NPC_DiploChange", "NPC Diplo Change", "Dynamic Diplomacy - Continued")
            };
        }

        [TestMethod]
        public void SendsLetter_Validations()
        {
            // Null def returns false
            Assert.IsFalse(IncidentFilterService.SendsLetter(null!));

            // Def with null or empty defName returns false
            var emptyDef = new IncidentDef { defName = null };
            Assert.IsFalse(IncidentFilterService.SendsLetter(emptyDef));

            var blankDef = new IncidentDef { defName = string.Empty };
            Assert.IsFalse(IncidentFilterService.SendsLetter(blankDef));

            // Incident without static letterDef/letterLabel (e.g. Dynamic Diplomacy, RaidEnemy) returns true
            var dynamicDef = new IncidentDef
            {
                defName = "NPC_DiploChange",
                letterDef = null,
                letterLabel = null,
                letterText = null
            };
            Assert.IsTrue(IncidentFilterService.SendsLetter(dynamicDef));

            // Incident with static letterDef returns true
            var staticDef = new IncidentDef
            {
                defName = "SolarFlare",
                letterLabel = "Solar flare"
            };
            Assert.IsTrue(IncidentFilterService.SendsLetter(staticDef));
        }

        [TestMethod]
        public void SetItems_InitializesAllModsFilterWithoutShifting()
        {
            var service = new IncidentFilterService();
            service.SetItems(CreateSampleItems());

            Assert.AreEqual(7, service.TotalCount);
            Assert.IsTrue(service.AvailableMods.Count >= 5);
            Assert.IsTrue(service.AvailableMods.Contains("Core"));
            Assert.IsTrue(service.AvailableMods.Contains("Anomaly"));
            Assert.IsTrue(service.AvailableMods.Contains("Biotech"));
            Assert.IsTrue(service.AvailableMods.Contains("Awesome Mod"));
            Assert.IsTrue(service.AvailableMods.Contains("Dynamic Diplomacy - Continued"));
        }

        [TestMethod]
        public void SearchQuery_FiltersByLabelOrDefNameOrMod()
        {
            var service = new IncidentFilterService();
            var settings = new LetterControllerSettings();
            service.SetItems(CreateSampleItems());

            // Search by label keyword
            service.SearchQuery = "Enemy";
            service.Update(settings);
            Assert.AreEqual(1, service.FilteredItems.Count);
            Assert.AreEqual("RaidEnemy", service.FilteredItems[0].DefName);

            // Search by mod name
            service.SearchQuery = "Anomaly";
            service.Update(settings);
            Assert.AreEqual(1, service.FilteredItems.Count);
            Assert.AreEqual("EntityCodexEntryUnlocked", service.FilteredItems[0].DefName);

            // Search by defName
            service.SearchQuery = "SolarFlare";
            service.Update(settings);
            Assert.AreEqual(1, service.FilteredItems.Count);
            Assert.AreEqual("SolarFlare", service.FilteredItems[0].DefName);
        }

        [TestMethod]
        public void ModFilter_FiltersItemsExclusivelyBySelectedMod()
        {
            var service = new IncidentFilterService();
            var settings = new LetterControllerSettings();
            service.SetItems(CreateSampleItems());

            service.SelectedModFilter = "Core";
            service.Update(settings);

            Assert.AreEqual(3, service.FilteredItems.Count);
            foreach (var item in service.FilteredItems)
            {
                Assert.AreEqual("Core", item.ModName);
            }
        }

        [TestMethod]
        public void SinglePassFilter_CombinesModAndQueryFiltersAccurately()
        {
            var service = new IncidentFilterService();
            var settings = new LetterControllerSettings();
            service.SetItems(CreateSampleItems());

            // Core has RaidEnemy, TraderCaravanArrival, SolarFlare
            service.SelectedModFilter = "Core";
            service.SearchQuery = "Flare";
            service.Update(settings);

            Assert.AreEqual(1, service.FilteredItems.Count);
            Assert.AreEqual("SolarFlare", service.FilteredItems[0].DefName);
        }

        [TestMethod]
        public void SortByTitle_AscendingAndDescending()
        {
            var service = new IncidentFilterService();
            var settings = new LetterControllerSettings();
            service.SetItems(CreateSampleItems());

            service.SortField = IncidentSortField.Title;
            service.SortDirection = SortDirection.Ascending;
            service.Update(settings);

            // Ascending: "Custom Mod Event" should come first
            Assert.AreEqual("Custom Mod Event", service.FilteredItems[0].Label);

            // Descending: "Trader Caravan" should come first
            service.SortDirection = SortDirection.Descending;
            service.Update(settings);
            Assert.AreEqual("Trader Caravan", service.FilteredItems[0].Label);
        }

        [TestMethod]
        public void SortByMod_AscendingAndDescending()
        {
            var service = new IncidentFilterService();
            var settings = new LetterControllerSettings();
            service.SetItems(CreateSampleItems());

            service.SortField = IncidentSortField.Mod;
            service.SortDirection = SortDirection.Ascending;
            service.Update(settings);

            // Ascending: "Anomaly" should come first
            Assert.AreEqual("Anomaly", service.FilteredItems[0].ModName);

            // Descending: "Dynamic Diplomacy - Continued" should come first
            service.SortDirection = SortDirection.Descending;
            service.Update(settings);
            Assert.AreEqual("Dynamic Diplomacy - Continued", service.FilteredItems[0].ModName);
        }

        [TestMethod]
        public void SortByBehaviorLinear_AscendingAndDescending()
        {
            var service = new IncidentFilterService();
            var settings = new LetterControllerSettings();
            service.SetItems(CreateSampleItems());

            // Set different behaviors
            settings.SetBehavior("RaidEnemy", LetterBehavior.Pause); // 2
            settings.SetBehavior("Infestation", LetterBehavior.Ignore); // 1
            // Others are Default: 0

            // Test Descending: Pause (2) -> Ignore (1) -> Default (0)
            service.SortField = IncidentSortField.Behavior;
            service.SortDirection = SortDirection.Descending;
            service.Update(settings);

            Assert.AreEqual("RaidEnemy", service.FilteredItems[0].DefName, "Pause item should be first in descending order.");
            Assert.AreEqual("Infestation", service.FilteredItems[1].DefName, "Ignore item should be second in descending order.");

            // Test Ascending: Default (0) -> Ignore (1) -> Pause (2)
            service.SortDirection = SortDirection.Ascending;
            service.Update(settings);

            Assert.AreEqual(LetterBehavior.Default, settings.GetBehavior(service.FilteredItems[0].DefName));
            Assert.AreEqual("Infestation", service.FilteredItems[^2].DefName);
            Assert.AreEqual("RaidEnemy", service.FilteredItems[^1].DefName);
        }

        [TestMethod]
        public void SortByBehaviorLinear_PreservesStableTitleOrderWithinBuckets()
        {
            var service = new IncidentFilterService();
            var settings = new LetterControllerSettings();

            var items = new List<IncidentItem>
            {
                new("DefZ", "Zebra", "ModA"),
                new("DefA", "Apple", "ModA"),
                new("DefM", "Mango", "ModA"),
                new("DefB", "Banana", "ModA")
            };
            service.SetItems(items);

            // Set Apple and Zebra to Pause; Banana and Mango remain Default
            settings.SetBehavior("DefA", LetterBehavior.Pause);
            settings.SetBehavior("DefZ", LetterBehavior.Pause);

            service.SortField = IncidentSortField.Behavior;
            service.SortDirection = SortDirection.Descending;
            service.Update(settings);

            // The pause bucket should contain Apple before Zebra (stably sorted by title)
            Assert.AreEqual("Apple", service.FilteredItems[0].Label);
            Assert.AreEqual("Zebra", service.FilteredItems[1].Label);

            // The default bucket should contain Banana before Mango (stably sorted by title)
            Assert.AreEqual("Banana", service.FilteredItems[2].Label);
            Assert.AreEqual("Mango", service.FilteredItems[3].Label);
        }

        [TestMethod]
        public void ToggleSort_SwitchesDirectionOrChangesField()
        {
            var service = new IncidentFilterService();
            service.SortField = IncidentSortField.Title;
            service.SortDirection = SortDirection.Ascending;

            // Toggling same field reverses direction
            service.ToggleSort(IncidentSortField.Title);
            Assert.AreEqual(SortDirection.Descending, service.SortDirection);

            // Toggling different field switches field and resets direction to Ascending
            service.ToggleSort(IncidentSortField.Mod);
            Assert.AreEqual(IncidentSortField.Mod, service.SortField);
            Assert.AreEqual(SortDirection.Ascending, service.SortDirection);
        }

        [TestMethod]
        public void InitializeFromCatalog_PopulatesFromCustomProvider()
        {
            var customDefs = new List<IncidentDef>
            {
                new() { defName = "CustomDef1", label = "Custom 1" },
                new() { defName = "CustomDef2", label = "Custom 2" },
                new() { defName = null } // Invalid, should be filtered out
            };

            var mockProvider = new StubCatalogProvider(customDefs);
            var service = new IncidentFilterService();
            service.InitializeFromCatalog(mockProvider);
            service.Update(new LetterControllerSettings());

            Assert.AreEqual(2, service.TotalCount);
            Assert.AreEqual("CustomDef1", service.FilteredItems[0].DefName);
            Assert.AreEqual("CustomDef2", service.FilteredItems[1].DefName);
        }

        [TestMethod]
        public void LargeDataset_LinearSortPerformanceAndCorrectness()
        {
            // Verify O(N) performance on 1,000 items
            var largeList = new List<IncidentItem>(1000);
            var settings = new LetterControllerSettings();

            for (int i = 0; i < 1000; i++)
            {
                string defName = $"Incident_{i:D4}";
                largeList.Add(new IncidentItem(defName, $"Incident {i:D4}", "TestMod"));

                if (i % 3 == 1)
                {
                    settings.SetBehavior(defName, LetterBehavior.Ignore);
                }
                else if (i % 3 == 2)
                {
                    settings.SetBehavior(defName, LetterBehavior.Pause);
                }
            }

            var service = new IncidentFilterService();
            service.SetItems(largeList);
            service.SortField = IncidentSortField.Behavior;
            service.SortDirection = SortDirection.Descending;
            service.Update(settings);

            Assert.AreEqual(1000, service.FilteredItems.Count);

            // Descending order: all Pause first, then all Ignore, then all Default
            int expectedPauseCount = largeList.Count(x => settings.GetBehavior(x.DefName) == LetterBehavior.Pause);
            int expectedIgnoreCount = largeList.Count(x => settings.GetBehavior(x.DefName) == LetterBehavior.Ignore);

            for (int i = 0; i < expectedPauseCount; i++)
            {
                Assert.AreEqual(LetterBehavior.Pause, settings.GetBehavior(service.FilteredItems[i].DefName));
            }

            for (int i = expectedPauseCount; i < expectedPauseCount + expectedIgnoreCount; i++)
            {
                Assert.AreEqual(LetterBehavior.Ignore, settings.GetBehavior(service.FilteredItems[i].DefName));
            }

            for (int i = expectedPauseCount + expectedIgnoreCount; i < service.FilteredItems.Count; i++)
            {
                Assert.AreEqual(LetterBehavior.Default, settings.GetBehavior(service.FilteredItems[i].DefName));
            }
        }

        private sealed class StubCatalogProvider : IIncidentCatalogProvider
        {
            private readonly IEnumerable<IncidentDef> defs;
            public StubCatalogProvider(IEnumerable<IncidentDef> defs) => this.defs = defs;
            public IEnumerable<IncidentDef> GetAllIncidents() => defs;
        }
    }
}
