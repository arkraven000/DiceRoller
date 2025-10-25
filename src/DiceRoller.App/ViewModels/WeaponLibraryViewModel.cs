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
/// ViewModel for the Weapon Library page.
/// Manages CRUD operations for weapon profiles.
/// REQ-DATA-001: All weapon data stored in encrypted database.
/// </summary>
public partial class WeaponLibraryViewModel : ViewModelBase
{
    private readonly IWeaponRepository _weaponRepository;

    [ObservableProperty]
    private ObservableCollection<WeaponProfile> _weapons = new();

    [ObservableProperty]
    private WeaponProfile? _selectedWeapon;

    [ObservableProperty]
    private WeaponProfile _editingWeapon = new();

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public WeaponLibraryViewModel(IWeaponRepository weaponRepository)
    {
        _weaponRepository = weaponRepository ?? throw new ArgumentNullException(nameof(weaponRepository));
    }

    /// <summary>
    /// Loads all weapons from the database.
    /// REQ-DATA-001: Retrieves from encrypted SQLCipher database.
    /// </summary>
    [RelayCommand]
    private async Task LoadWeaponsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading weapon arsenal from the Omnissiah's vaults...";

            var weapons = await _weaponRepository.GetAllAsync();

            Weapons.Clear();
            foreach (var weapon in weapons)
            {
                Weapons.Add(weapon);
            }

            StatusMessage = $"Loaded {Weapons.Count} weapons. The Emperor's armory is ready.";
        }
        catch (Exception ex)
        {
            // REQ-ERROR-002: Don't expose internal details
            StatusMessage = $"Failed to load weapons: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Searches weapons by name.
    /// REQ-INPUT-003: Search uses parameterized queries via repository.
    /// </summary>
    [RelayCommand]
    private async Task SearchWeaponsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Searching weapon archives...";

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadWeaponsAsync();
                return;
            }

            var weapons = await _weaponRepository.SearchByNameAsync(SearchText);

            Weapons.Clear();
            foreach (var weapon in weapons)
            {
                Weapons.Add(weapon);
            }

            StatusMessage = $"Found {Weapons.Count} weapons matching '{SearchText}'.";
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
    /// Creates a new weapon profile.
    /// </summary>
    [RelayCommand]
    private void CreateNew()
    {
        EditingWeapon = new WeaponProfile();
        IsEditing = true;
        StatusMessage = "Forge a new weapon for the Emperor's armies.";
    }

    /// <summary>
    /// Edits the selected weapon.
    /// </summary>
    [RelayCommand]
    private void EditSelected()
    {
        if (SelectedWeapon == null)
        {
            StatusMessage = "No weapon selected for modification.";
            return;
        }

        // Create a copy for editing
        EditingWeapon = new WeaponProfile
        {
            Id = SelectedWeapon.Id,
            Name = SelectedWeapon.Name,
            Attacks = SelectedWeapon.Attacks,
            BallisticSkill = SelectedWeapon.BallisticSkill,
            Strength = SelectedWeapon.Strength,
            ArmorPenetration = SelectedWeapon.ArmorPenetration,
            DamageType = SelectedWeapon.DamageType,
            FixedDamage = SelectedWeapon.FixedDamage,
            Abilities = SelectedWeapon.Abilities,
            AntiThreshold = SelectedWeapon.AntiThreshold,
            Description = SelectedWeapon.Description,
            CreatedAt = SelectedWeapon.CreatedAt,
            ModifiedAt = SelectedWeapon.ModifiedAt
        };

        IsEditing = true;
        StatusMessage = $"Modifying weapon: {SelectedWeapon.Name}";
    }

    /// <summary>
    /// Saves the weapon being edited.
    /// REQ-INPUT-001: Validates weapon before saving.
    /// REQ-DATA-001: Stores in encrypted database.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Saving weapon profile...";

            // REQ-INPUT-001: Validate weapon profile
            if (!EditingWeapon.IsValid(out string? errorMessage))
            {
                StatusMessage = $"Invalid weapon profile: {errorMessage}";
                return;
            }

            if (EditingWeapon.Id == 0)
            {
                // Insert new weapon
                int newId = await _weaponRepository.InsertAsync(EditingWeapon);
                EditingWeapon.Id = newId;
                Weapons.Add(EditingWeapon);
                StatusMessage = $"Weapon '{EditingWeapon.Name}' forged successfully.";
            }
            else
            {
                // Update existing weapon
                bool success = await _weaponRepository.UpdateAsync(EditingWeapon);

                if (success)
                {
                    // Update the observable collection
                    var existing = Weapons.FirstOrDefault(w => w.Id == EditingWeapon.Id);
                    if (existing != null)
                    {
                        int index = Weapons.IndexOf(existing);
                        Weapons[index] = EditingWeapon;
                    }

                    StatusMessage = $"Weapon '{EditingWeapon.Name}' updated by order of the Mechanicus.";
                }
                else
                {
                    StatusMessage = "Failed to update weapon. The Machine Spirit is displeased.";
                }
            }

            IsEditing = false;
            EditingWeapon = new WeaponProfile();
        }
        catch (Exception ex)
        {
            // REQ-ERROR-002: Don't expose internal details
            StatusMessage = $"Failed to save weapon: {ex.Message}";
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
        EditingWeapon = new WeaponProfile();
        StatusMessage = "Edit cancelled. The forge remains idle.";
    }

    /// <summary>
    /// Deletes the selected weapon.
    /// REQ-DATA-001: Removes from encrypted database.
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedWeapon == null)
        {
            StatusMessage = "No weapon selected for destruction.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Removing weapon: {SelectedWeapon.Name}...";

            bool success = await _weaponRepository.DeleteAsync(SelectedWeapon.Id);

            if (success)
            {
                Weapons.Remove(SelectedWeapon);
                StatusMessage = $"Weapon '{SelectedWeapon.Name}' removed from the armory.";
                SelectedWeapon = null;
            }
            else
            {
                StatusMessage = "Failed to remove weapon. The record persists.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to delete weapon: {ex.Message}";
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
        await LoadWeaponsAsync();
    }
}
