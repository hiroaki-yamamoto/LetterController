using System.Collections.Generic;
using LetterController.Core;
using LetterController.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LetterController.Tests
{
    /// <summary>
    /// Unit tests verifying the search, filtering, and sorting behavior of <see cref="IncidentFilterService"/>.
    /// </summary>
    [TestClass]
    public class FilterServiceTests
    {
        private List<IncidentItem> CreateSampleItems()
        {
            return new List<IncidentItem>
            {
                new IncidentItem("RaidEnemy", "Enemy Raid", "Core"),
                new IncidentItem("TraderCaravanArrival", "Trader Caravan", "Core"),
                new IncidentItem("SolarFlare", "Solar Flare", "Core"),
                new IncidentItem("EntityCodexEntryUnlocked", "Entity Discovered", "Anomaly"),
                new IncidentItem("Infestation", "Infestation", "Biotech"),
                new IncidentItem("ModCustomIncident", "Custom Mod Event", "Awesome Mod"),
                new IncidentItem("NPC_DiploChange", "NPC Diplo Change", "Dynamic Diplomacy - Continued")
            };
        }

        [TestMethod]
        public void SendsLetter_Validations()
        {
            // Null def returns false
            Assert.IsFalse(IncidentFilterService.SendsLetter(null!));

            // Def with null or empty defName returns false
            var emptyDef = new RimWorld.IncidentDef { defName = null };
            Assert.IsFalse(IncidentFilterService.SendsLetter(emptyDef));

            var blankDef = new RimWorld.IncidentDef { defName = string.Empty };
            Assert.IsFalse(IncidentFilterService.SendsLetter(blankDef));

            // Incident without static letterDef/letterLabel (e.g. Dynamic Diplomacy, RaidEnemy) returns true
            var dynamicDef = new RimWorld.IncidentDef
            {
                defName = "NPC_DiploChange",
                letterDef = null,
                letterLabel = null,
                letterText = null
            };
            Assert.IsTrue(IncidentFilterService.SendsLetter(dynamicDef));

            // Incident with static letterDef returns true
            var staticDef = new RimWorld.IncidentDef
            {
                defName = "SolarFlare",
                letterLabel = "Solar flare"
            };
            Assert.IsTrue(IncidentFilterService.SendsLetter(staticDef));
        }

        [TestMethod]
        public void SetItems_InitializesAllModsFilter()
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
        public void SortByBehavior_RespectsSettingsValues()
        {
            var service = new IncidentFilterService();
            var settings = new LetterControllerSettings();
            service.SetItems(CreateSampleItems());

            // Set one item to Pause (value 2), others remain Default (value 0)
            settings.SetBehavior("RaidEnemy", LetterBehavior.Pause);

            service.SortField = IncidentSortField.Behavior;
            service.SortDirection = SortDirection.Descending;
            service.Update(settings);

            // In descending order, Pause (2) should be at the top
            Assert.AreEqual("RaidEnemy", service.FilteredItems[0].DefName);
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
    }
}
