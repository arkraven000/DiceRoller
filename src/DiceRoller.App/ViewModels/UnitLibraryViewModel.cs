using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiceRoller.Core.Models;
using DiceRoller.Core.Services.Database;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DiceRoller.App.ViewModels;

/// <summary>
/// ViewModel for the Unit Library page.
/// Manages CRUD operations for unit profiles.
/// REQ-DATA-001: All unit data stored in encrypted database.
/// </summary>
public partial class UnitLibraryViewModel : ViewModelBase
{
    private readonly IUnitRepository _unitRepository;

    [ObservableProperty]
    private ObservableCollection<UnitProfile> _units = new();

    [ObservableProperty]
    private UnitProfile? _selectedUnit;

    [ObservableProperty]
    private UnitProfile _editingUnit = new();

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public UnitLibraryViewModel(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository ?? throw new ArgumentNullException(nameof(unitRepository));
    }

    /// <summary>
    /// Loads all units from the database.
    /// REQ-DATA-001: Retrieves from encrypted SQLCipher database.
    /// </summary>
    [RelayCommand]
    private async Task LoadUnitsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading unit codex from the Imperial archives...";

            var units = await _unitRepository.GetAllAsync();

            Units.Clear();
            foreach (var unit in units)
            {
                Units.Add(unit);
            }

            StatusMessage = $"Loaded {Units.Count} units. The Emperor's legions stand ready.";
        }
        catch (Exception ex)
        {
            // REQ-ERROR-002: Don't expose internal details
            StatusMessage = $"Failed to load units: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Searches units by name.
    /// REQ-INPUT-003: Search uses parameterized queries via repository.
    /// </summary>
    [RelayCommand]
    private async Task SearchUnitsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Searching unit records...";

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadUnitsAsync();
                return;
            }

            var units = await _unitRepository.SearchByNameAsync(SearchText);

            Units.Clear();
            foreach (var unit in units)
            {
                Units.Add(unit);
            }

            StatusMessage = $"Found {Units.Count} units matching '{SearchText}'.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Creates a new unit profile.
    /// </summary>
    [RelayCommand]
    private void CreateNew()
    {
        EditingUnit = new UnitProfile();
        IsEditing = true;
        StatusMessage = "Recruit a new unit for the Emperor's armies.";
    }

    /// <summary>
    /// Edits the selected unit.
    /// </summary>
    [RelayCommand]
    private void EditSelected()
    {
        if (SelectedUnit == null)
        {
            StatusMessage = "No unit selected for modification.";
            return;
        }

        // Create a copy for editing
        EditingUnit = new UnitProfile
        {
            Id = SelectedUnit.Id,
            Name = SelectedUnit.Name,
            Toughness = SelectedUnit.Toughness,
            Save = SelectedUnit.Save,
            InvulnerableSave = SelectedUnit.InvulnerableSave,
            FeelNoPain = SelectedUnit.FeelNoPain,
            WoundsPerModel = SelectedUnit.WoundsPerModel,
            ModelCount = SelectedUnit.ModelCount,
            Keywords = SelectedUnit.Keywords,
            InCover = SelectedUnit.InCover,
            Description = SelectedUnit.Description,
            CreatedAt = SelectedUnit.CreatedAt,
            ModifiedAt = SelectedUnit.ModifiedAt
        };

        IsEditing = true;
        StatusMessage = $"Modifying unit: {SelectedUnit.Name}";
    }

    /// <summary>
    /// Saves the unit being edited.
    /// REQ-INPUT-001: Validates unit before saving.
    /// REQ-DATA-001: Stores in encrypted database.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Saving unit profile...";

            // REQ-INPUT-001: Validate unit profile
            if (!EditingUnit.IsValid(out string? errorMessage))
            {
                StatusMessage = $"Invalid unit profile: {errorMessage}";
                return;
            }

            if (EditingUnit.Id == 0)
            {
                // Insert new unit
                int newId = await _unitRepository.InsertAsync(EditingUnit);
                EditingUnit.Id = newId;
                Units.Add(EditingUnit);
                StatusMessage = $"Unit '{EditingUnit.Name}' recruited successfully.";
            }
            else
            {
                // Update existing unit
                bool success = await _unitRepository.UpdateAsync(EditingUnit);

                if (success)
                {
                    // Update the observable collection
                    var existing = Units.FirstOrDefault(u => u.Id == EditingUnit.Id);
                    if (existing != null)
                    {
                        int index = Units.IndexOf(existing);
                        Units[index] = EditingUnit;
                    }

                    StatusMessage = $"Unit '{EditingUnit.Name}' updated by the Administratum.";
                }
                else
                {
                    StatusMessage = "Failed to update unit. The dataslate remains unchanged.";
                }
            }

            IsEditing = false;
            EditingUnit = new UnitProfile();
        }
        catch (Exception ex)
        {
            // REQ-ERROR-002: Don't expose internal details
            StatusMessage = $"Failed to save unit: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cancels editing.
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingUnit = new UnitProfile();
        StatusMessage = "Edit cancelled. The records remain sealed.";
    }

    /// <summary>
    /// Deletes the selected unit.
    /// REQ-DATA-001: Removes from encrypted database.
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedUnit == null)
        {
            StatusMessage = "No unit selected for removal.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Removing unit: {SelectedUnit.Name}...";

            bool success = await _unitRepository.DeleteAsync(SelectedUnit.Id);

            if (success)
            {
                Units.Remove(SelectedUnit);
                StatusMessage = $"Unit '{SelectedUnit.Name}' removed from the codex.";
                SelectedUnit = null;
            }
            else
            {
                StatusMessage = "Failed to remove unit. The record persists.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to delete unit: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Called when navigating to this page.
    /// </summary>
    public override async Task OnNavigatedToAsync()
    {
        await LoadUnitsAsync();
    }
}
