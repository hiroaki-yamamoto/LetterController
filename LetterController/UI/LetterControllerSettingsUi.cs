using System;
using System.Collections.Generic;
using LetterController.Core;
using LetterController.Services;
using UnityEngine;
using Verse;

namespace LetterController.UI
{
    /// <summary>
    /// Handles the rendering and user interaction for the Letter Controller mod settings screen.
    /// Follows RimWorld's native UI conventions, including FloatMenus, ScrollViews, and visual styling.
    /// Displays letter-sending incidents with per-incident behavior controls.
    /// </summary>
    public class LetterControllerSettingsUi
    {
        private readonly IncidentFilterService filterService = new();
        private Vector2 scrollPosition = Vector2.zero;
        private bool isInitialized;

        private const float RowHeight = 34f;
        private const float ColorBoxWidth = 14f;

        /// <summary>
        /// Renders the settings screen contents within the specified bounding rectangle.
        /// </summary>
        /// <param name="inRect">The bounding rectangle for the mod options window.</param>
        /// <param name="settings">The active mod settings instance.</param>
        public void Draw(Rect inRect, LetterControllerSettings settings)
        {
            if (!isInitialized)
            {
                filterService.InitializeFromDefs();
                isInitialized = true;
            }

            // Ensure search and sort filters are up-to-date
            filterService.Update(settings);

            float currentY = inRect.y;

            // 1. Draw Top Toolbar (Search, Mod Filter, Reset All)
            DrawToolbar(new Rect(inRect.x, currentY, inRect.width, 36f), settings);
            currentY += 44f;

            // 2. Draw Table Headers (Sortable columns)
            DrawHeaders(new Rect(inRect.x, currentY, inRect.width - 16f, 28f));
            currentY += 32f;

            // 3. Draw Incident List (Scroll View with viewport culling for O(1) rendering)
            Rect outRect = new Rect(inRect.x, currentY, inRect.width, inRect.yMax - currentY - 24f);
            DrawIncidentTable(outRect, settings);

            // 4. Footer (Status / Count info)
            Rect footerRect = new Rect(inRect.x, inRect.yMax - 22f, inRect.width, 22f);
            DrawFooter(footerRect);
        }

        private void DrawToolbar(Rect rect, LetterControllerSettings settings)
        {
            float x = rect.x;

            // Search Label & Box
            float labelWidth = 60f;
            float searchBoxWidth = 220f;
            Widgets.Label(new Rect(x, rect.y + 4f, labelWidth, 24f), "LetterController_Search".Translate());
            x += labelWidth;

            string query = Widgets.TextField(new Rect(x, rect.y + 2f, searchBoxWidth, 28f), filterService.SearchQuery);
            if (!string.Equals(query, filterService.SearchQuery, StringComparison.Ordinal))
            {
                filterService.SearchQuery = query;
            }
            x += searchBoxWidth + 6f;

            // Clear Search Button
            if (!string.IsNullOrEmpty(filterService.SearchQuery))
            {
                if (Widgets.ButtonText(new Rect(x, rect.y + 2f, 50f, 28f), "LetterController_Clear".Translate()))
                {
                    filterService.SearchQuery = string.Empty;
                }
            }
            x += 60f;

            // Mod Filter Dropdown
            float modFilterWidth = 200f;
            string currentModLabel = string.IsNullOrEmpty(filterService.SelectedModFilter)
                ? "LetterController_AllMods".Translate().ToString()
                : filterService.SelectedModFilter;

            if (Widgets.ButtonText(new Rect(x, rect.y + 2f, modFilterWidth, 28f), currentModLabel.Truncate(modFilterWidth - 10f)))
            {
                var options = new List<FloatMenuOption>();
                foreach (var mod in filterService.AvailableMods)
                {
                    string targetMod = mod;
                    options.Add(new FloatMenuOption(targetMod, () =>
                    {
                        filterService.SelectedModFilter = targetMod;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            x += modFilterWidth + 12f;

            // Reset All Button
            float resetBtnWidth = 150f;
            Rect resetRect = new Rect(rect.xMax - resetBtnWidth, rect.y + 2f, resetBtnWidth, 28f);
            if (Widgets.ButtonText(resetRect, "LetterController_ResetAll".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "LetterController_ResetConfirm".Translate(),
                    () =>
                    {
                        settings.ResetAll();
                        filterService.SortField = IncidentSortField.Title;
                        filterService.SortDirection = SortDirection.Ascending;
                    },
                    destructive: true));
            }
        }

        private void DrawHeaders(Rect rect)
        {
            GUI.color = new Color(0.85f, 0.85f, 0.85f);
            Widgets.DrawHighlight(rect);

            float titleColWidth = rect.width * 0.45f;
            float modColWidth = rect.width * 0.30f;
            float actionColWidth = rect.width - titleColWidth - modColWidth;

            // Title Header
            string titleSortArrow = GetSortArrow(IncidentSortField.Title);
            Rect titleRect = new Rect(rect.x, rect.y, titleColWidth, rect.height);
            if (Widgets.ButtonInvisible(titleRect))
            {
                filterService.ToggleSort(IncidentSortField.Title);
            }
            Widgets.Label(new Rect(titleRect.x + 8f, titleRect.y + 4f, titleRect.width - 16f, 20f),
                "LetterController_SortTitle".Translate() + titleSortArrow);

            // Mod Header
            string modSortArrow = GetSortArrow(IncidentSortField.Mod);
            Rect modRect = new Rect(rect.x + titleColWidth, rect.y, modColWidth, rect.height);
            if (Widgets.ButtonInvisible(modRect))
            {
                filterService.ToggleSort(IncidentSortField.Mod);
            }
            Widgets.Label(new Rect(modRect.x + 8f, modRect.y + 4f, modRect.width - 16f, 20f),
                "LetterController_SortMod".Translate() + modSortArrow);

            // Behavior Header
            string behSortArrow = GetSortArrow(IncidentSortField.Behavior);
            Rect actionRect = new Rect(rect.x + titleColWidth + modColWidth, rect.y, actionColWidth, rect.height);
            if (Widgets.ButtonInvisible(actionRect))
            {
                filterService.ToggleSort(IncidentSortField.Behavior);
            }
            Widgets.Label(new Rect(actionRect.x + 8f, actionRect.y + 4f, actionRect.width - 16f, 20f),
                "LetterController_SortBehavior".Translate() + behSortArrow);

            GUI.color = Color.white;
        }

        private string GetSortArrow(IncidentSortField field)
        {
            if (filterService.SortField != field)
            {
                return string.Empty;
            }

            return filterService.SortDirection == SortDirection.Ascending ? " ▲" : " ▼";
        }

        private void DrawIncidentTable(Rect outRect, LetterControllerSettings settings)
        {
            var items = filterService.FilteredItems;
            float totalHeight = items.Count * RowHeight;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, totalHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            // Viewport frustum culling: only render visible rows
            int firstVisibleIndex = Mathf.Max(0, (int)(scrollPosition.y / RowHeight));
            int lastVisibleIndex = Mathf.Min(items.Count - 1, (int)((scrollPosition.y + outRect.height) / RowHeight) + 1);

            float titleColWidth = viewRect.width * 0.45f;
            float modColWidth = viewRect.width * 0.30f;
            float actionColWidth = viewRect.width - titleColWidth - modColWidth;

            for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
            {
                var item = items[i];
                float rowY = i * RowHeight;
                Rect rowRect = new Rect(0f, rowY, viewRect.width, RowHeight);

                // Alternating row highlights
                if (i % 2 == 1)
                {
                    Widgets.DrawLightHighlight(rowRect);
                }

                // Color accent bar on the left indicating associated letter color
                Color letterColor = item.LetterDef?.color ?? Color.gray;
                Rect colorRect = new Rect(rowRect.x + 4f, rowRect.y + 8f, ColorBoxWidth, RowHeight - 16f);
                Widgets.DrawBoxSolid(colorRect, letterColor);

                // Title and defName
                Rect titleRect = new Rect(rowRect.x + ColorBoxWidth + 12f, rowRect.y + 6f, titleColWidth - ColorBoxWidth - 16f, RowHeight - 12f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(titleRect, item.Label.Truncate(titleRect.width));
                Text.Anchor = TextAnchor.UpperLeft;

                // Tooltip on title
                if (Mouse.IsOver(titleRect))
                {
                    string tooltip = $"{item.Label}\n(defName: {item.DefName})";
                    if (!string.IsNullOrEmpty(item.Def?.description))
                    {
                        tooltip += $"\n\n{item.Def.description}";
                    }
                    TooltipHandler.TipRegion(titleRect, tooltip);
                }

                // Mod Source
                Rect modRect = new Rect(rowRect.x + titleColWidth + 8f, rowRect.y + 6f, modColWidth - 16f, RowHeight - 12f);
                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                Widgets.Label(modRect, item.ModName.Truncate(modRect.width));
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;

                // Behavior Dropdown Button
                Rect actionRect = new Rect(rowRect.x + titleColWidth + modColWidth + 8f, rowRect.y + 4f, actionColWidth - 16f, RowHeight - 8f);
                var currentBehavior = settings.GetBehavior(item.DefName);

                DrawBehaviorButton(actionRect, item.DefName, currentBehavior, settings);
            }

            Widgets.EndScrollView();
        }

        private void DrawBehaviorButton(Rect rect, string defName, LetterBehavior currentBehavior, LetterControllerSettings settings)
        {
            string labelKey = currentBehavior switch
            {
                LetterBehavior.Ignore => "LetterController_Behavior_Ignore",
                LetterBehavior.Pause => "LetterController_Behavior_Pause",
                _ => "LetterController_Behavior_Default"
            };

            // Set button color based on configured behavior
            Color origColor = GUI.color;
            if (currentBehavior == LetterBehavior.Ignore)
            {
                GUI.color = new Color(1f, 0.6f, 0.6f);
            }
            else if (currentBehavior == LetterBehavior.Pause)
            {
                GUI.color = new Color(1f, 0.9f, 0.5f);
            }

            if (Widgets.ButtonText(rect, labelKey.Translate()))
            {
                var options = new List<FloatMenuOption>
                {
                    new FloatMenuOption(
                        "LetterController_Behavior_Default".Translate() + " - " + "LetterController_BehaviorDesc_Default".Translate(),
                        () => settings.SetBehavior(defName, LetterBehavior.Default)),
                    new FloatMenuOption(
                        "LetterController_Behavior_Ignore".Translate() + " - " + "LetterController_BehaviorDesc_Ignore".Translate(),
                        () => settings.SetBehavior(defName, LetterBehavior.Ignore)),
                    new FloatMenuOption(
                        "LetterController_Behavior_Pause".Translate() + " - " + "LetterController_BehaviorDesc_Pause".Translate(),
                        () => settings.SetBehavior(defName, LetterBehavior.Pause))
                };
                Find.WindowStack.Add(new FloatMenu(options));
            }

            GUI.color = origColor;
        }

        private void DrawFooter(Rect rect)
        {
            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.65f, 0.65f, 0.65f);
            string footerText = "LetterController_IncidentCount".Translate(filterService.FilteredItems.Count, filterService.TotalCount);
            Widgets.Label(rect, footerText);
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }
    }
}
