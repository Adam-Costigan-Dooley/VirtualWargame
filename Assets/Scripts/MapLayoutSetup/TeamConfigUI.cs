using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for individual team configuration item
/// Should be attached to the team config item prefab
/// </summary>
public class TeamConfigUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Toggle enableToggle;
    [SerializeField] private TextMeshProUGUI teamNameText;
    [SerializeField] private TMP_Dropdown colorDropdown;
    [SerializeField] private TMP_InputField resourcesInput;
    
    private TeamType teamType;
    private MapLayoutSetupUI manager;
    
    public void Initialize(TeamType type, MapLayoutSetupUI setupManager)
    {
        teamType = type;
        manager = setupManager;
        
        // Set team name
        teamNameText.text = TeamConfig.GetTeamName(type);
        
        // Setup color dropdown
        colorDropdown.ClearOptions();
        List<string> colorOptions = new List<string>();
        foreach (TeamColor color in System.Enum.GetValues(typeof(TeamColor)))
        {
            colorOptions.Add(color.ToString());
        }
        colorDropdown.AddOptions(colorOptions);
        
        // Set default color
        TeamColor defaultColor = TeamConfig.GetDefaultColor(type);
        colorDropdown.value = (int)defaultColor;
        
        // Set default resources
        resourcesInput.text = "0";
        
        // Initially disabled
        enableToggle.isOn = false;
        SetInteractable(false);
        
        // Add listeners
        enableToggle.onValueChanged.AddListener(OnToggled);
        colorDropdown.onValueChanged.AddListener(OnColorChanged);
        resourcesInput.onEndEdit.AddListener(OnResourcesChanged);
    }
    
    private void OnToggled(bool enabled)
    {
        SetInteractable(enabled);
        manager.ToggleTeam(teamType, enabled);
    }
    
    private void OnColorChanged(int colorIndex)
    {
        TeamColor color = (TeamColor)colorIndex;
        manager.SetTeamColor(teamType, color);
    }
    
    private void OnResourcesChanged(string value)
    {
        if (int.TryParse(value, out int resources))
        {
            manager.SetTeamResources(teamType, resources);
        }
        else
        {
            resourcesInput.text = "0";
        }
    }
    
    private void SetInteractable(bool interactable)
    {
        colorDropdown.interactable = interactable;
        resourcesInput.interactable = interactable;
    }
}
