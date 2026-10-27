using System;
using System.Collections.Generic;
using System.Linq;
using LetterController.Core;
using RimWorld;
using Verse;

namespace LetterController.Services
{
    /// <summary>
    /// Specifies the field by which the incident letter list is sorted.
    /// </summary>
    public enum IncidentSortField
    {
        /// <summary>
        /// Sort by incident display label or defName.
        /// </summary>
        Title,

        /// <summary>
        /// Sort by originating DLC or mod name.
        /// </summary>
        Mod,

        /// <summary>
        /// Sort by configured behavior.
        /// </summary>
        Behavior
    }

    /// <summary>
    /// Specifies the sort direction.
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Ascending order.
        /// </summary>
        Ascending,

        /// <summary>
        /// Descending order.
        /// </summary>
        Descending
    }

    /// <summary>
    /// Service responsible for gathering, filtering, searching, and sorting incident definitions that send letters.
    /// Employs cached state and dirty flags to avoid redundant allocations on every UI render frame.
    /// </summary>
    public class IncidentFilterService
    {
        private List<IncidentItem> allItems = new();
        private List<string> availableMods = new();
        private List<IncidentItem> filteredItems = new();

        private string searchQuery = string.Empty;
        private string selectedModFilter = string.Empty;
        private IncidentSortField currentSortField = IncidentSortField.Title;
        private SortDirection currentSortDirection = SortDirection.Ascending;
        private bool isDirty = true;
        private int lastSettingsCustomCount = -1;

        /// <summary>
        /// Gets the filtered and sorted list of incident items.
        /// </summary>
        public IReadOnlyList<IncidentItem> FilteredItems => filteredItems;

        /// <summary>
        /// Gets the total count of letter-sending incidents loaded in the game.
        /// </summary>
        public int TotalCount => allItems.Count;

        /// <summary>
        /// Gets the list of available mod and DLC names for filtering, with "All Mods" as the first option.
        /// </summary>
        public IReadOnlyList<string> AvailableMods => availableMods;

        /// <summary>
        /// Gets or sets the search query string.
        /// </summary>
        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                var trimmed = value ?? string.Empty;
                if (!string.Equals(searchQuery, trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    searchQuery = trimmed;
                    isDirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently selected mod filter.
        /// Empty or "All" indicates no mod filter.
        /// </summary>
        public string SelectedModFilter
        {
            get => selectedModFilter;
            set
            {
                var val = value ?? string.Empty;
                if (!string.Equals(selectedModFilter, val, StringComparison.OrdinalIgnoreCase))
                {
                    selectedModFilter = val;
                    isDirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the active sort field.
        /// </summary>
        public IncidentSortField SortField
        {
            get => currentSortField;
            set
            {
                if (currentSortField != value)
                {
                    currentSortField = value;
                    isDirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the active sort direction.
        /// </summary>
        public SortDirection SortDirection
        {
            get => currentSortDirection;
            set
            {
                if (currentSortDirection != value)
                {
                    currentSortDirection = value;
                    isDirty = true;
                }
            }
        }

        /// <summary>
        /// Initializes the incident catalog from the RimWorld DefDatabase,
        /// including all incidents that send in-game letters.
        /// </summary>
        public void InitializeFromDefs()
        {
            var defs = DefDatabase<IncidentDef>.AllDefsListForReading;
            var items = new List<IncidentItem>(defs.Count);
            foreach (var def in defs)
            {
                if (def != null && SendsLetter(def))
                {
                    items.Add(new IncidentItem(def));
                }
            }
            SetItems(items);
        }

        /// <summary>
        /// Determines whether the specified <see cref="IncidentDef"/> is valid and can send in-game letters.
        /// In RimWorld, many incidents (such as raids, trader visits, resource pod crashes, and modded events like Dynamic Diplomacy)
        /// dispatch letters dynamically via their worker class rather than declaring static XML letter fields (e.g. letterDef, letterLabel).
        /// Therefore, all valid incident definitions are cataloged so players can configure their letter behaviors.
        /// </summary>
        /// <param name="def">The incident definition to evaluate.</param>
        /// <returns><c>true</c> if the incident is valid and can send letters; otherwise, <c>false</c>.</returns>
        public static bool SendsLetter(IncidentDef def)
        {
            if (def == null || string.IsNullOrEmpty(def.defName))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets items explicitly (primarily used for unit testing).
        /// </summary>
        /// <param name="items">The items list.</param>
        public void SetItems(IEnumerable<IncidentItem> items)
        {
            allItems = items?.ToList() ?? new List<IncidentItem>();

            var modSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in allItems)
            {
                if (!string.IsNullOrEmpty(item.ModName))
                {
                    modSet.Add(item.ModName);
                }
            }

            var sortedMods = modSet.OrderBy(m => m, StringComparer.OrdinalIgnoreCase).ToList();
            sortedMods.Insert(0, "LetterController_AllMods".Translate().ToString());
            availableMods = sortedMods;

            selectedModFilter = availableMods[0];
            isDirty = true;
        }

        /// <summary>
        /// Toggles the sort direction if the same field is selected, or switches to the new field in ascending order.
        /// </summary>
        /// <param name="field">The sort field to activate.</param>
        public void ToggleSort(IncidentSortField field)
        {
            if (currentSortField == field)
            {
                SortDirection = currentSortDirection == SortDirection.Ascending
                    ? SortDirection.Descending
                    : SortDirection.Ascending;
            }
            else
            {
                currentSortField = field;
                SortDirection = SortDirection.Ascending;
                isDirty = true;
            }
        }

        /// <summary>
        /// Updates the filtered and sorted list if any search parameters or settings changed.
        /// Should be called before rendering the UI table.
        /// </summary>
        /// <param name="settings">The active settings instance to resolve behaviors for sorting.</param>
        public void Update(LetterControllerSettings settings)
        {
            // If settings changed externally (e.g. custom count differs), mark dirty for behavior sort
            if (currentSortField == IncidentSortField.Behavior && settings != null && settings.CustomCount != lastSettingsCustomCount)
            {
                isDirty = true;
                lastSettingsCustomCount = settings.CustomCount;
            }

            if (!isDirty)
            {
                return;
            }

            isDirty = false;

            IEnumerable<IncidentItem> result = allItems;

            // 1. Filter by Mod if specified
            string allModsLabel = availableMods.Count > 0 ? availableMods[0] : "All";
            if (!string.IsNullOrEmpty(selectedModFilter) && !string.Equals(selectedModFilter, allModsLabel, StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(i => string.Equals(i.ModName, selectedModFilter, StringComparison.OrdinalIgnoreCase));
            }

            // 2. Filter by search query (O(N) search across label, defName, and modName)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var query = searchQuery.Trim();
                result = result.Where(i =>
                    i.Label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    i.DefName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    i.ModName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var list = result.ToList();

            // 3. Sort items
            IComparer<IncidentItem> comparer = GetComparer(currentSortField, settings);
            if (currentSortDirection == SortDirection.Descending)
            {
                comparer = new ReverseComparer<IncidentItem>(comparer);
            }

            list.Sort(comparer);
            filteredItems = list;
        }

        private IComparer<IncidentItem> GetComparer(IncidentSortField field, LetterControllerSettings? settings)
        {
            switch (field)
            {
                case IncidentSortField.Mod:
                    return Comparer<IncidentItem>.Create((a, b) =>
                    {
                        int cmp = string.Compare(a.ModName, b.ModName, StringComparison.OrdinalIgnoreCase);
                        return cmp != 0 ? cmp : string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase);
                    });

                case IncidentSortField.Behavior:
                    return Comparer<IncidentItem>.Create((a, b) =>
                    {
                        var behA = settings?.GetBehavior(a.DefName) ?? LetterBehavior.Default;
                        var behB = settings?.GetBehavior(b.DefName) ?? LetterBehavior.Default;
                        int cmp = behA.CompareTo(behB);
                        return cmp != 0 ? cmp : string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase);
                    });

                case IncidentSortField.Title:
                default:
                    return Comparer<IncidentItem>.Create((a, b) =>
                    {
                        int cmp = string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase);
                        return cmp != 0 ? cmp : string.Compare(a.DefName, b.DefName, StringComparison.OrdinalIgnoreCase);
                    });
            }
        }

        private sealed class ReverseComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> original;

            public ReverseComparer(IComparer<T> original)
            {
                this.original = original;
            }

            public int Compare(T? x, T? y)
            {
                return original.Compare(y!, x!);
            }
        }
    }
}
